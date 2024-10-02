#!/usr/bin/env python

import argparse
import asyncio
import logging
import cv2

from signaling import signaling
from receiver import VideoFrameReceiver
from imageDetection import runYOLOinit, runYOLO

from aiortc import RTCConfiguration, RTCIceServer, RTCPeerConnection, RTCSessionDescription, RTCIceCandidate
from aiortc.contrib.media import MediaRelay
from aiortc.contrib.signaling import BYE

logger = logging.getLogger("ans_client")

STUN_SERVER = ["stun:stun.l.google.com:19302",]
WEBSOCKET_URI = "ws://127.0.0.1:7236"
CONNECTION_COMPLETE = False
RECEIVER = VideoFrameReceiver(None)

async def app(pc: RTCPeerConnection, sig: signaling, relay: MediaRelay, model):
    global STUN_SERVER, WEBSOCKET_URI, CONNECTION_COMPLETE
    
    # connect to our signalling server
    await sig.start(WEBSOCKET_URI)
    logger.info("sig server connected")
    
    # creates a data channel of name "channel". negotiated is set to false as we did not negotiate the channel out of band.
    channel = pc.createDataChannel("channel", ordered=True, negotiated=False)
    
    # the peerconnection event "track" is fired when a new track is received
    # we check the track type and add it to our receiver object
    @pc.on("track")
    async def on_track(track):
        global RECEIVER
        logger.debug(f"got a track!! {track.kind} {type(track.kind)}")
        if track.kind == "video":
            RECEIVER = VideoFrameReceiver(relay.subscribe(track))
            pc.addTrack(RECEIVER)
            logger.debug("track added to RECEIVER")
    
    # the peerconnection event "iceconnectionstatechange" is fired when the iceconnection state changes value
    # we can use this to handel errors in connection
    @pc.on("iceconnectionstatechange")
    async def on_iceconnectionstatechange():
        logger.debug(f"ICE Connection is now: {pc.iceConnectionState}")
    
    # similar to "iceconnectionstatechange", we can use this to handel errors that come about in the connection process
    @pc.on("connectionstatechange")
    async def on_connectionstatechange():
        global CONNECTION_COMPLETE
        logger.debug(f"Connection state has changed: {pc.connectionState}")
        if pc.connectionState == "connected":
            CONNECTION_COMPLETE = True
            asyncio.ensure_future(check_queue())
            logger.debug("ensure_future on check_queue")
        elif pc.connectionState == "failed":
            logger.info("connection failed")
    
    # fires when the channel that we made is open by the remote peer
    @channel.on("open")
    def on_open():
        logger.info("datachannel channel is open")
    
    # fires when a new message is received from a remote peer to our channel
    @channel.on("message")
    def on_message(msg):
        logger.info(f"message recieved: {msg}")
    
    # this is where we take each frame in and pass to YOLO.
    # at the moment we sleep for half a second before going into the loop 
    # to make sure video packets are being sent after a connection is complete.
    # then in the try block we pass in the frame and model to the runYOLO function.
    # the model is made in the main function using runYOLOinit.
    async def check_queue():
        await asyncio.sleep(0.5)
        while CONNECTION_COMPLETE:
            frame = await RECEIVER.recv()
            try:
                bounding_str_ls = await runYOLO(frame, model)
                for bounding_str in bounding_str_ls:
                    if bounding_str is not None:
                        channel.send(bounding_str)
                        logger.info("bounding box string sent over channel")
                        logger.debug(bounding_str)
            except Exception as e:
                logger.debug(f"Error found in check_queue try block: {e}")
            
            await asyncio.sleep(0.01) # this is to make sure our loop allows other coroutine to run at the same time
    
    # signaling loop that takes in the SDP object made by the signaling server.
    # we assume we are waiting for an offer from the headset before we send our answer.
    # ice candidates are sent by the library aioRTC automatically once SDP offers and then answers are sent
    while not CONNECTION_COMPLETE:
        obj = await sig.recv()
        logger.debug(f"Received signaling object: {obj}")
        
        if isinstance(obj, RTCSessionDescription):
            logger.debug(f"Received RTCSessionDescription: {obj.type}")
            await pc.setRemoteDescription(obj)
            logger.debug(f"Set remote description: {obj.type}")
            
            # if this needs an answer
            if obj.type == "offer":
                answer = await pc.createAnswer()
                await pc.setLocalDescription(answer)
                await sig.send(pc.localDescription)
                logger.debug(f"Sent local description: {pc.localDescription.sdp}")
        elif isinstance(obj, RTCIceCandidate):
            logger.debug("Received ICE Candidate")
            await pc.addIceCandidate(obj)
            logger.debug(f"Added ICE candidate: {obj}")
        elif obj is BYE:
            logger.debug("Receive BYE, exiting")
            break


async def main():
    config = RTCConfiguration(
        iceServers=[RTCIceServer(urls=STUN_SERVER)])
    pc = RTCPeerConnection(config)
    
    logger.info("peer connection with config constructed")
    
    sig = signaling()

    model = await runYOLOinit()
    
    relay = MediaRelay()
    
    await asyncio.gather(
        app(pc, sig, relay, model),
    )

if __name__ == "__main__":
    # we dont have any arguments currently set up, 
    # but this is where you would parse them and pass in options to your functions that need them
    parser = argparse.ArgumentParser(description="ans_client")
    options = parser.parse_args()

    logging.basicConfig(level=logging.DEBUG)
    
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        pass
    finally:
        # cleanup here
        logger.info("cleaning up and shutting down (not really)")
        # main.py method to close all processes
        # need to make a signaling class obj so i can shut it down from here or else where
        
