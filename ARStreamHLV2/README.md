## Requirements

- Microsoft Hololens 2 headset
- Windows PC

## How to run

1. Clone git repo
2. Open ARStreamHLV2 in unity (version 2022.3.19f1)
3. Install universal windows platform build support
4. Plug in Hololens 2&TRADE; to a windows PC, turn on, and unlock headset
5. Check the IP address in unity matches the server IP address
6. Build to the Hololens 2 (Settings pictured below)
    <p align="center">
        <br>
        <img src="../ReadMe images/unity build settings for hololens.png" alt="Icon" width="450">
        <br>
        <caption>Unity 2022.3.19f1 build settings</caption>
    </p>
7. Open .sln file in Visual Studio&TRADE; 2019
8. Visual Studio build settings at the top of the window
    <p align="center">
        <br>
        <img src="../ReadMe images/visual studio solution settings.png" width="450">
        <br>
        <caption>Visual Studio deploy settings</caption>
    </p>
9. Deploy solution to the Hololens 2 (During first time deployment, Visual Studio will ask for a password pin on the Hololens 2)
    <p align="center">
        <br>
        <img src="../ReadMe images/visual studio deploy solution.png" width="450">
        <br>
        <caption>Visual Studio deploy settings</caption>
    </p>
10. Run the docker file (see server readme for details)
11. Open the app on the headset
