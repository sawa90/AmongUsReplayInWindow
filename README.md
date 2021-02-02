# AmongUsReplayInWindow
![screenshot](https://github.com/sawa90/AmongUsReplayInWindow/blob/images/scrnshot.png)

This project add a replay feature to Among Us. The replay will appear in the game window when you return to the lobby after the game.  
In the replay, you can see everyone's location, who killed them and when, the progress of the task, if they are in the vent, and sabotage.  

# Requirement
[.NET 5 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/5.0)

# Usage
- Download zip from [latest release](https://github.com/sawa90/AmongUsReplayInWindow/releases/latest) page and unpack it
- Run Among Us
- Run `AmongUsReplayInWindow.exe`
- Click "Get Among Us Window" button
- Play game

When the game is over, a replay of the last game will be displayed in the game window.  
Use the control keys to open and close the replay, and drag the track bar or use the arrow keys to control it.  
The replays will be saved in "C:\Program Files (x86)\Steam\steamapps\common\Among Us\AmongUsReplayInWindow" folder, and you can also view them by clicking the "Open" button to open the file.  

# Note
- Who killed a crew is determined by the imposter who was closest to the crew, so it is possible to make a mistake.
- The winner may be displayed incorrectly when the game is decided by disconnect.

# License
MIT  

# References
[amonguscapture](https://github.com/automuteus/amonguscapture) by Denver Quane  
[among-us-replay-mod](https://github.com/Smertig/among-us-replay-mod) by Smertig  

