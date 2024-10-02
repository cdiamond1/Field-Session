# importing libraries 
from ultralytics import YOLO
import json
from BoundingBox import BoundingBox
import cv2
import asyncio

### run YOLO initialization function
# This function initializes the yolo model based on yolov8n.pt 
# and runs a test frame to tank the latency frontloaded on the first
# run of the model

# WARNING: dont know how relative path can be enforced
async def runYOLOinit():

    # Initialize yolo model outside of the main loop so analysis runs faster
    model = YOLO("yolov8n.pt")

    # First runthrough of a frame takes around 40-70ms longer than the rest, so run a dummy .jpg to overcome the hurdle
    #runYOLO("ImageDetection\\imgs\\factorywork.jpg", model)
    return model

### run YOLO function
# This funtion uses yolov8n.pt to analyze a png or jpg frame 
# and send the data of each box that surrounds an object specified
# in the yolo model
#
# WARNING: This program DOES NOT WORK with video files (mp4, etc), In
# order to run this program with video files, the programmer must place
# "stream= True" as an addition argument to the model(frame, ...) function on line 41
# and figure out how to access bounding box variables x1, y1, x2, y2, and item
# name. Not setting "Stream= True" and passing in a video file can lead to a 
# memory leak.
async def runYOLO(frame, model) -> str | None:

    # Place bounding boxes in an iterable container
    # and write to a single csv input
    boundingBoxArr = []

    # Run yolo model on a single frame with a standard confidence of 40% and an intersection over union of 0.7
    # Increasing the Confidence may reduce the amount of bounding boxes reported, but will report much more accurate findings
    # Increasing the IOU (0.5 is standard, 0.7 is strict) may decrease the amount of items reported, but will tighten the accuracy of the bounding box coordinates around an object
    results = model(frame, conf = 0.70, iou=0.7) 

    # Names of objects in YOLO are stored as integer values whose actual name can bo found in the model.names dictionary
    # So store the keys of the names of bounding boxes as well as their coordinates to later be iterated on
    nameIndexTensor = results[0].boxes.cls
    xyTensor = results[0].boxes.xyxy

    # Create bounding boxes using the names of items and coordinates of the upper left & lower right points of the bounding box
    counter = 0
    for item in xyTensor:
        newBB = BoundingBox(model.names[nameIndexTensor[counter].item()], tuple([item[0].item(), item[1].item()]), tuple([item[2].item(), item[3].item()]))
        
        # store bounding boxes in an array to be output to a json file
        boundingBoxArr.append(newBB)
        counter += 1
    
    json_str = sendBBDataToJSON(boundingBoxArr)
    
    return json_str


### Send Bounding Box Data To JSON function
# This function takes a list of bounding box objects and 
# returns a json string
def sendBBDataToJSON(boundingBoxArr):
     
     # IF the list is empty, then dont try to format anything
     if len(boundingBoxArr) != 0:
        
        json_list = []
        # For each bounding box, put its info in a dictionary to be fed into JSON
        for bb in boundingBoxArr:
            boundingBoxDict = {
                "x1": bb.upperLeft[0],
                "y1": bb.upperLeft[1],
                "x2": bb.bottomRight[0],
                "y2": bb.bottomRight[1],
                "color": bb.color,
                "type": "box",
                "label": bb.label,
            }
            json_str = json.dumps(boundingBoxDict)
            json_list.append(json_str)

        # return to a JSON string
        return json_list
