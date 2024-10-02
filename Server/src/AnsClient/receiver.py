from aiortc.contrib.media import MediaStreamTrack
import asyncio
import logging

logger = logging.getLogger("recevier")

class VideoFrameReceiver(MediaStreamTrack):
    kind = "video"

    def __init__(self, track: MediaStreamTrack):
        super().__init__()
        self.track = track
        
    async def start(self):
        asyncio.create_task(self.__run_track(self.track))
        
    async def __run_track(self, track):
        while True:
            await self.recv()

    async def recv(self):
        frame = await self.track.recv()
        logger.debug("gotten frame")
        
        nd_frame = frame.to_ndarray(format="bgr24")
        logger.debug("converted frame to ndarray")
        return nd_frame