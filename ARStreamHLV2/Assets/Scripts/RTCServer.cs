using UnityEngine;
using Microsoft.MixedReality.WebRTC;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using System;
using Microsoft.MixedReality.WebRTC.Unity;

public class RTCServer : MonoBehaviour
{
    Signaler signaler;
    Transceiver audioTransceiver = null;
    Transceiver videoTransceiver = null;
    Microsoft.MixedReality.WebRTC.AudioTrackSource audioTrackSource = null;
    Microsoft.MixedReality.WebRTC.VideoTrackSource videoTrackSource = null;
    LocalAudioTrack localAudioTrack = null;
    LocalVideoTrack localVideoTrack = null;

    WebcamSource webcamSource = null;

    IReadOnlyList<VideoCaptureDevice> deviceList = null;

    public bool NeedVideo = true;
    private bool NeedAudio = false;

    private uint VideoWidth = 640;
    private uint VideoHeight = 400;
    private uint VideoFps = 30;
    private string VideoProfileId = "";

    public string Ip = "";
    public int Port = 9999;
    public SignalerType ConnectionType = SignalerType.TCP;

    public bool UseRemoteStun = false;

    public BoundingBoxManager bbm;

    async void Start()
    {
        deviceList = await DeviceVideoTrackSource.GetCaptureDevicesAsync();
        Logger.Log($"Found {deviceList.Count} devices.");
        foreach (var device in deviceList)
        {
            Logger.Log($"Found webcam {device.name} (id: {device.id}):");
            var profiles = await DeviceVideoTrackSource.GetCaptureProfilesAsync(device.id);
            if (profiles.Count > 0)
            {
                foreach (var profile in profiles)
                {
                    string configstring = "";
                    Logger.Log($"+ Profile '{profile.uniqueId}'");
                    var configs = await DeviceVideoTrackSource.GetCaptureFormatsAsync(device.id, profile.uniqueId);
                    foreach (var config in configs)
                    {
                        configstring = configstring + ($"  - {config.width}x{config.height}@{config.framerate}");
                    }
                    Logger.Log(configstring);
                }
            }
            else
            {
                string configstring = "";
                var configs = await DeviceVideoTrackSource.GetCaptureFormatsAsync(device.id);
                foreach (var config in configs)
                {
                    configstring = configstring + ($"- {config.width}x{config.height}@{config.framerate}");
                }
                Logger.Log(configstring);
            }
        }

        // Setup signaling
        Logger.Log("Starting signaling...");
        switch (ConnectionType)
        {
            case SignalerType.TCP:
                signaler = new TCPSignaler(Port);
                break;
            case SignalerType.WebSocket:
                signaler = new WebSocketSignaler(Ip,Port);
                break;
            default:
                throw new System.Exception($"Signaler connection type {ConnectionType} is not valid!");
        }
        signaler.ClientConnected += OnClientConnected;
        signaler.ClientDisconnected += OnClientDisconnected;
        if (UseRemoteStun)
        {
            signaler.IceServers.Add(new IceServer { Urls = { "stun:stun.l.google.com:19302" } });
        }
        signaler.Start();
    }

    async void OnClientConnected()
    {


        Logger.Log("Client Connected");
        var pc = signaler.PeerConnection;


        // Record video from local webcam, and send to remote peer
        if (NeedVideo)
        {
            // For example, print them to the standard output

            var deviceSettings = new LocalVideoDeviceInitConfig
            {
                width = VideoWidth,
                height = VideoHeight,
            };
            if (VideoFps > 0)
            {
                deviceSettings.framerate = VideoFps;
            }
            if (VideoProfileId.Length > 0)
            {
                deviceSettings.videoProfileId = VideoProfileId;
            }

            //Logger.Log($"Attempt to grab Camera - {deviceSettings.videoProfileId}: {deviceSettings.width}x{deviceSettings.height}@{deviceSettings.framerate}fps");

            //videoTrackSource = await DeviceVideoTrackSource.CreateAsync(deviceSettings);
            webcamSource = GetComponent<WebcamSource>();
            
            //Logger.Log($"Create local video track... {videoTrackSource}");
            var trackSettings = new LocalVideoTrackInitConfig
            {
                trackName = "webcam_track"
            };
            localVideoTrack = LocalVideoTrack.CreateFromSource(webcamSource.Source, trackSettings);

            Logger.Log("Create video transceiver and add webcam track...");
            videoTransceiver = pc.AddTransceiver(MediaKind.Video);
            videoTransceiver.DesiredDirection = Transceiver.Direction.SendReceive;
            videoTransceiver.LocalVideoTrack = localVideoTrack;
        }

        // Record audio from local microphone, and send to remote peer
        if (NeedAudio)
        {
            Logger.Log("Opening local microphone...");
            audioTrackSource = await DeviceAudioTrackSource.CreateAsync();

            Logger.Log("Create local audio track...");
            var trackSettings = new LocalAudioTrackInitConfig { trackName = "mic_track" };
            localAudioTrack = LocalAudioTrack.CreateFromSource(audioTrackSource, trackSettings);

            Logger.Log("Create audio transceiver and add mic track...");
            audioTransceiver = pc.AddTransceiver(MediaKind.Audio);
            audioTransceiver.DesiredDirection = Transceiver.Direction.SendReceive;
            audioTransceiver.LocalAudioTrack = localAudioTrack;
        }

        // Start peer connection
        int numFrames = 0;
        pc.VideoTrackAdded += (RemoteVideoTrack track) =>
        {
            Logger.Log($"Attach Frame Listener...");
            track.I420AVideoFrameReady += (I420AVideoFrame frame) =>
            {
                ++numFrames;
                if (numFrames % 60 == 0)
                {
                    Logger.Log($"Received video frames: {numFrames}");
                }
            };
        };

        addChannel();

        // we need a short delay here for the video stream to settle...
        // I assume my Logitech webcam is sending some garbage frames in the beginning.
        await Task.Delay(200);
        pc.CreateOffer();
        Logger.Log("Send offer to remote peer");

        addChannelListener();
    }

    public void OnClientDisconnected()
    {
        localAudioTrack?.Dispose();
        localVideoTrack?.Dispose();
        audioTrackSource?.Dispose();
        videoTrackSource?.Dispose();
    }

    void OnDisable()
    {
        OnClientDisconnected();
        signaler?.Stop();
        Logger.Log("Program terminated.");
    }

    public enum SignalerType
    {
        TCP = 0,
        WebSocket = 1
    }

    async void addChannel()
    {
        var pc = signaler.PeerConnection;
        await pc.AddDataChannelAsync("channel2", true, false);
    }

    async void addChannelListener()
    {
        bool found = false;
        var pc = signaler.PeerConnection;
        int tries = 0;
        while (found == false && tries < 50)
        {
            //Debug.Log("#of datachannels: " + pc.DataChannels.Count);
            foreach (DataChannel dc in pc.DataChannels)
            {
                //Debug.Log("Data Channel Found: " + dc.Label);
                if (dc.Label == "channel")
                {
                    //add listener
                    found = true;
                    dc.MessageReceived += (byte[] message) =>
                    {
                        string text = System.Text.Encoding.UTF8.GetString(message);
                        //Debug.Log($"[remote] {text}\n");
                        bbm.addToJsonQueue(text);
                    };
                }
            }
            await Task.Delay(200);
            tries += 1;
        }
    }


}
