<div>
    <p align="center">
        <img src="ReadMe%20images/streamicon.png" alt="Icon" width="450">
    </p>
</div>

# AR Stream

Colorado School of Mines 2024 Summer Field Sessions project (Belviranli 2)

## How it works

The camera feed is taken from the Microsoft Hololens 2 then sent a server through WebRTC which is then processed through the YOLO image detection algorith. The algorithm then returns data back through the WebRTC datastream to the headset which uses the data to draw bounding boxes in real time around recognized objects.

## How to run
[Headset instructions](/ARStreamHLV2/README.md) \
[Server instructions](/Server/src/README.md)

## Applications

The original application of this project is to be used in clean rooms to help instructors train lab technitions more easily through premade instructions using this system.

<p align="center">
    <img src="ReadMe images/example.png" alt="Icon" width="450">
    <br>
    <caption>Figure 0: Example created image from project proposition document</caption>
</p>

## Future Work
- Forward and Back buttons on instructions
- Voice commands for simple actions
- Trained image recognition models
- Multiple headset streams at once
- Portability to other AR headsets
- Swapping hands on the menu
- Other fields
- Optomization

## Sprint progress
### Sprint 1 (5/13/2024 - 5/17/2024)
- GitHub setup
- Team Contract
- Requirements document
- Research differences in headsets (Oculus quest 2, Magic Leap 1, Hololens 2)
- Start WebRTC research
- Start Docker research
- Access Magic Leap 1 camera
- Oculus quest 2 would be too difficult to work with because the pass-through camera is in greyscale and would potentially need to be sideloaded or jailbroken
- Accessed Magic Leap 1 camera
- Magic Leap 1 planned obsolescence as of 12/31/2024

<p align="center">
    <br>
    <br>
    <img src="ReadMe images/magic leap comera.jpg" alt="Icon" width="450">
    <br>
    <caption>Figure 1: Magic Leap 1 Camera Output</caption>
</p>

### Sprint 2 (5/20/2024 - 5/24/2024)
- Concluded to focus on the Hololens 2
- Began writing our own WebRTC function for local network
- YOLO algorithm example created
- Gained camera access from the Hololens 2
- Created sample instructions that follow left hand in Hololens 2
- Using Unity SphereCasting to utilize the Hololens 2's spatial mapping mesh in order to place bounding boxes on environment
<p align="center">
    <br>
    <br>
    <img src="ReadMe images/pre-set bounding box.jpg" alt="Icon" width="450">
    <br>
    <caption>Figure 2: Hololens 2 Camera Output with instructions and preset bounding box</caption> 
</p>
<br>
<br>
<p align="center">
    <img src="ReadMe images/bounding box button.jpg" alt="Icon" width="450">
    <br>
    <caption>Figure 3: Hololens 2 Camera Output with instructions and button to create bounding boxes</caption>
</p>

### Sprint 3 (5/27/2024 - 5/31/2024)
- Headset can now take in JSON file and parse bounding box data
- Image detection JSON output and heatset JSON expection now mostly synced up
- Sped up image recognition with YOLO
- Parse in TO-DO/Instructions data through JSON
- Added comments to unity code (BoundingBox.cs)
- WebRTC able to ping between unity and server (Still working on video)

<br>
<p align="center">
    <img src="ReadMe images/5_28_2024 Hololens progress - Made with Clipchamp.gif"></img>
    <br>
    <caption>5/28/2024 Progress</caption>
    <br>
    <caption>Box A, B, and C are completely random while Example box is parsed from an example JSON</caption>
</p>

### Sprint 4 (6/3/2024 - 6/7/2024)
- WebRTC connection working consistantly (YIPPEE!)
- Sending data over WebRTC
- Headset able to recieve random bounding boxes through JSONs
- Video stream working (mostly)

### Sprint 5 (6/10/2024 - 6/14/2024)
- Clean up GitHub
- Working on docker container
- Streaming works now
- Polishing box location on headset via offsets and remap
- Finalizing headset build
- Finalizing documentation
- Finalizing report

## Contact
Feel free to reach out with any questions or comments: \
colonreyeskenneth@mines.edu (server)\
cowley@mines.edu (server)\
cdiamond1@mines.edu (headset)\
zacharysnyder@mines.edu (headset)
