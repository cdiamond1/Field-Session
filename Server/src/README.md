# Server
This is how to setup and manage the server.

## Requirements
* A server running some form of Linux with an Nvidia GPU  
* Docker
* Docker Compose plugin
* NVIDIA Container Toolkit

## How to Run
1. `cd` into `Server/src`
2. Run `docker compose up --build` (after building once on the machine, you can run `docker compose up`)
3. This should run both containers and a `sig server connected` message should be seen from the `ans_client` container
4. Now you can start the headset client and they should connect

## Troubleshooting
**Always make sure to change the logging level to debug so you get all logs**

## Future Expansion Ideas
* Using the broadcast IP to advertise to the network the IP of the server rather than manual input on the headset
* Using the data channel to send info relating to the todo list and lesson plans. For example the headset sends a code asking for a certain todo list from the server, the server can send back the todo list via the channel.

## Contact
For help with deployment issues or connection issues reach out via email: \
cowley@mines.edu \
colonreyeskenneth@mines.edu
