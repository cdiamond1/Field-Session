from aiortc.contrib.signaling import candidate_to_sdp, candidate_from_sdp, BYE
from aiortc import RTCIceCandidate, RTCSessionDescription
import json
import websockets
from websockets import WebSocketClientProtocol
import logging

logger = logging.getLogger("signaling")

class signaling():
    
    def __init__(self):
        self.websocket_server: WebSocketClientProtocol = None
    
    async def start(self, URI):
        try:
            self.websocket_server = await websockets.connect(URI)
            logger.info("Connection established")
            return True
        except Exception as e:
            logger.info(f"Failed to connect: {e}")
            self.websocket_server = None
            return False
        
    async def close(self):
        await self.websocket_server.close()

    async def recv(self):
        obj = await self.websocket_server.recv()
        
        obj_json = json.loads(obj)
        
        if obj_json["type"] == "sdp":
            if "answer" in obj_json:
                return RTCSessionDescription(type="answer", sdp=obj_json["answer"])
            else:
                return RTCSessionDescription(type="offer", sdp=obj_json["offer"])
        elif obj_json["type"] == "ice" and obj_json["candidate"]:
            canidate = candidate_from_sdp(obj_json["candidate"].split(":", 1)[1])
            canidate.sdpMid = obj_json["sdpMid"]
            canidate.sdpMLineIndex = obj_json["sdpMLineindex"]
            return canidate
        elif obj_json["type"] == "bye":
            return BYE
        
    async def send(self, obj):
        if isinstance(obj, RTCSessionDescription):
            message = {
                "type": "sdp", 
                obj.type: obj.sdp
            }
        elif isinstance(obj, RTCIceCandidate):
            message = {
                "candidate": "candidate:" + candidate_to_sdp(obj),
                "sdpMid": obj.sdpMid,
                "sdpMLineindex": obj.sdpMLineIndex,
                "type": "ice",
            }
        elif isinstance(obj, BYE):
            message = {"type": "bye"}
        
        message_json = json.dumps(message, sort_keys=True)
        await self.websocket_server.send(message_json)