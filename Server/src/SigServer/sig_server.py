#!/usr/bin/env python

import asyncio
import binascii
import os

import websockets

import logging

logging.basicConfig(level=logging.INFO)

clients = {}


async def echo(websocket, path):
    client_id = binascii.hexlify(os.urandom(8)).decode('utf-8')
    clients[client_id] = websocket
    logging.info(f"Client {client_id} connected")

    try:
        async for message in websocket:
            logging.info(f"Received message from {client_id}: {message}")
            for c_id, c in clients.items():
                if c != websocket:
                    try:
                        await c.send(message)
                        logging.info(f"Sent message to {c_id}")
                    except websockets.ConnectionClosed:
                        logging.warning(f"Failed to send message to {c_id}: Connection closed")
                        pass
    except websockets.ConnectionClosed:
        logging.info(f"Client {client_id} disconnected")
    finally:
        clients.pop(client_id, None)
        logging.info(f"Client {client_id} removed")


async def main():
    server = await websockets.serve(echo, "0.0.0.0", 7236)
    logging.info("WebSocket server started")
    await server.wait_closed()

if __name__ == "__main__":
    asyncio.run(main())
