using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AmongUsCapture.Memory.Structs;
using AUOffsetManager;
using Newtonsoft.Json;
using System.Numerics;

namespace AmongUsCapture
{
    public enum GameState
    {
        LOBBY,
        TASKS,
        DISCUSSION,
        MENU,
        UNKNOWN,
        ENDED,
        HumansWinByVote,
        HumansWinByTask,
        ImpostorWinByVote,
        ImpostorWinByKill,
        ImpostorWinBySabotage,
        ImpostorDisconnect,
        HumansDisconnect,
        Unknown,
        HumansWinByDisconnect,
        ImpostorWinByDisconnect
    }

    public class GameMemReader
    {
        private static readonly GameMemReader instance = new GameMemReader();
        private bool exileCausesEnd;

        private bool shouldReadLobby = false;
        private IntPtr GameAssemblyPtr = IntPtr.Zero;

        public Dictionary<string, PlayerInfo>
            newPlayerInfos =
                new Dictionary<string, PlayerInfo>(
                    10); // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        private LobbyEventArgs latestLobbyEventArgs = null;

        public Dictionary<string, PlayerInfo>
            oldPlayerInfos =
                new Dictionary<string, PlayerInfo>(
                    10); // Important: this is making the assumption that player names are unique. They are, but for better tracking of players and to eliminate any ambiguity the keys of this probably need to be the players' network IDs instead

        private Dictionary<string, ImmutablePlayer> CachedPlayerInfos = new Dictionary<string, ImmutablePlayer>();

        private GameState oldState = GameState.UNKNOWN;

        private int prevChatBubsVersion;
        private bool shouldForceTransmitState;
        private bool shouldForceUpdatePlayers;
        private bool shouldTransmitLobby;

        public static GameMemReader getInstance()
        {
            return instance;
        }

        public OffsetManager offMan = new OffsetManager();
        public GameOffsets CurrentOffsets;
        public string GameHash = "";

        public event EventHandler<ValidatorEventArgs> GameVersionUnverified;
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;

        public event EventHandler<ChatMessageEventArgs> ChatMessageAdded;

        public event EventHandler<LobbyEventArgs> JoinedLobby;
        public event EventHandler<ProcessHookArgs> ProcessHook;
        public event EventHandler<ProcessHookArgs> ProcessUnHook;
        public event EventHandler<GameOverEventArgs> GameOver;

        public event EventHandler<PlayerCosmeticChangedEventArgs> PlayerCosmeticChanged;

        public event EventHandler<GameStartEventArgs> GameStart;


        private bool cracked = false;

        public event EventHandler<PlayerMoveArgs> PlayerMove;
        public Vector2[] PlayerPoses = new Vector2[10];
        PlayerControl[] newPlayerCon = new PlayerControl[10]; // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        public Color[] PlayerColors = new Color[10];
        public PlayerColor[] PlayerColorsInt = new PlayerColor[10];
        public string[] PlayerNames = new string[10];

        //-10 disconnect, +-11 ejected, +/-29~+/-20 killed this/previous turn by ID:abs(num)-20, +/-15 killed by someone this/previous turn 
        public int[] playerIsDead = new int[10];
        public bool[] IsImpostorLis = new bool[10];


        int AllImposterNum = 0;
        int[] ImpostorId = new int[3];
        bool[] InVent = new bool[3];

        int[] TaskNum = new int[10];
        TaskInfo[][] taskInfos = new TaskInfo[10][];
        TaskInfo Sabotage;

        float[] TaskProgress = new float[10];
        bool[] Taskcompleted = new bool[10];
        GameState testState = GameState.TASKS;
        bool testbool = false;


        public bool playing = false;

        Dictionary<string, int> PlayerName2IDdict = new Dictionary<string, int>();

        long gameStartTime = 0;


       List<DeadBodyPos> DeadBodyPosList = new List<DeadBodyPos>();
        List<DeadLog> DeadLogList = new List<DeadLog>();

        Vector2[] centerOfTable = new Vector2[3] { new Vector2(-1.0f, 1.1f), new Vector2(24.03f,2.625f), new Vector2(19.5f,-16.876f)
 };

        GameOverReason gameOverReason = GameOverReason.Unknown;
        GameState exileCausesEndState = GameState.Unknown;

        PlayMap playMap = PlayMap.Skeld;
        public string filename = null;
        int myId = 0;

        Door[] doors;
        UInt32 doorsUint = 0;

        public GameMemReader()
        {
            DeadLog.memReader = this;
            initConInfo();
        }


        private void initConInfo()
        {
            for (int i = 0; i < 10; i++)
            {
                playerIsDead[i] = 0;
                PlayerColors[i] = Color.Empty;
                IsImpostorLis[i] = false;
                taskInfos[i] = new TaskInfo[10];
                TaskProgress[i] = 0;
                Taskcompleted[i] = false;
                TaskNum[i] = 0;
                PlayerNames[i] = "";
            }
            Sabotage = new TaskInfo();
            DeadBodyPosList.Clear();
            DeadLogList.Clear();
            PlayerName2IDdict.Clear();
            gameOverReason = GameOverReason.Unknown;
            doorsUint = 0;
        }


        public void RunLoop()
        {
            var tokenSource = new CancellationTokenSource();
            var cancelToken = tokenSource.Token;
            RunLoop(cancelToken);
        }

        public void RunLoop(CancellationToken cancelToken)
        {
            while (true)
            {
                if (cancelToken.IsCancellationRequested) break;
                try
                {
                    #region amonguscapture
                    {
                        if (!ProcessMemory.getInstance().IsHooked || ProcessMemory.getInstance().process is null ||
                            ProcessMemory.getInstance().process.HasExited)
                        {
                            if (!ProcessMemory.getInstance().HookProcess("Among Us"))
                            {
                                Thread.Sleep(1000);
                                continue;
                            }
                            Console.WriteLine($"Connected to Among Us process ({ProcessMemory.getInstance().process.Id})");


                            var foundModule = false;

                            while (true)
                            {
                                foreach (var module in ProcessMemory.getInstance().modules)
                                    if (module.Name.Equals("GameAssembly.dll", StringComparison.OrdinalIgnoreCase))
                                    {
                                        GameAssemblyPtr = module.BaseAddress;
                                        if (!GameVerifier.VerifySteamHash(module.FileName))
                                        {
                                            cracked = true;
                                            Console.WriteLine($"Client verification: FAIL.", "GameVerifier");
                                        }
                                        else
                                        {
                                            cracked = false;
                                            Console.WriteLine($"Client verification: PASS.", "GameVerifier");
                                        }

                                        using (SHA256Managed sha256 = new SHA256Managed())
                                        {
                                            using (FileStream fs = new FileStream(module.FileName, FileMode.Open,
                                                FileAccess.Read))
                                            {
                                                using (var bs = new BufferedStream(fs))
                                                {
                                                    var hash = sha256.ComputeHash(bs);
                                                    StringBuilder GameAssemblyhashSb = new StringBuilder(2 * hash.Length);
                                                    foreach (byte byt in hash)
                                                    {
                                                        GameAssemblyhashSb.AppendFormat("{0:X2}", byt);
                                                    }

                                                    Console.WriteLine(
                                                        $"GameAssembly Hash: {GameAssemblyhashSb.ToString()}");
                                                    GameHash = GameAssemblyhashSb.ToString();
                                                    CurrentOffsets = offMan.FetchForHash(GameAssemblyhashSb.ToString());
                                                    if (CurrentOffsets is not null)
                                                    {
                                                        Console.WriteLine("GameMemReader", $"Loaded offsets: {CurrentOffsets.Description}");
                                                        ProcessHook?.Invoke(this, new ProcessHookArgs { PID = ProcessMemory.getInstance().process.Id });
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine(
                                                            $"No offsets found for: {GameAssemblyhashSb.ToString()}.", "GameMemReader");

                                                    }

                                                }
                                            }
                                        }

                                        foundModule = true;
                                        break;
                                    }

                                if (!foundModule)
                                {
                                    Console.WriteLine(
                                        "Still looking for modules...", "GameMemReader");
                                    //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "Still looking for modules..."); // TODO: This still isn't functional, we need to re-hook to reload module addresses
                                    Thread.Sleep(500); // delay and try again
                                    ProcessMemory.getInstance().LoadModules();
                                }
                                else
                                {
                                    break; // we have found all modules
                                }
                            }

                            try
                            {
                                if (CurrentOffsets is not null)
                                {
                                }

                                // prevGameOverReason = ProcessMemory.getInstance().Read<GameOverReason>(GameAssemblyPtr, _gameOffsets.TempDataOffset, 0x5c, 4);
                            }
                            catch
                            {
                                Console.WriteLine("Outdated version of the game.", "ERROR");
                            }

                        }

                        if (cracked && ProcessMemory.getInstance().IsHooked)
                        {
                            Console.WriteLine("CrackDetected", "ERROR");
                            Environment.Exit(0);

                            continue;
                        }

                        if (CurrentOffsets is null) continue;
                    }






                    GameState state;
                    //int meetingHudState = /*meetingHud_cachePtr == 0 ? 4 : */ProcessMemory.ReadWithDefault<int>(GameAssemblyPtr, 4, 0xDA58D0, 0x5C, 0, 0x84); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                    var meetingHud = ProcessMemory.getInstance()
                        .Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.MeetingHudPtr);
                    var meetingHud_cachePtr = meetingHud == IntPtr.Zero
                        ? 0
                        : ProcessMemory.getInstance().Read<uint>(meetingHud, CurrentOffsets.MeetingHudCachePtrOffsets);
                    var meetingHudState =
                        meetingHud_cachePtr == 0
                            ? 4
                            : ProcessMemory.getInstance().ReadWithDefault(meetingHud, 4, CurrentOffsets.MeetingHudStateOffsets
                                ); // 0 = Discussion, 1 = NotVoted, 2 = Voted, 3 = Results, 4 = Proceeding
                    var gameState =
                        ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.GameStateOffsets); // 0 = NotJoined, 1 = Joined, 2 = Started, 3 = ENDED (during "defeat" or "victory" screen only)
#endregion
                    switch (gameState)
                    {
                        case 0:
                            state = GameState.MENU;
                            exileCausesEnd = false;
                            break;
                        case 1:
                            state = GameState.LOBBY;
                            exileCausesEnd = false;
                            break;
                        case 3:
                            state = GameState.ENDED;
                            exileCausesEnd = false;
                            break;
                        default:
                            {

                                if (exileCausesEnd)
                                    state = exileCausesEndState;
                                else if (meetingHudState < 4)
                                    state = GameState.DISCUSSION;
                                else
                                    state = GameState.TASKS;

                                break;
                            }
                    }
                    //Console.WriteLine($"Got state: {state}");
                    if (testbool) state = testState;


                    var allPlayersPtr =
                        ProcessMemory.getInstance()
                            .Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
                    var allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
                    var playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);

                    var playerAddrPtr = allPlayers + 0x10;

                    // check if exile causes end
                    if (oldState == GameState.DISCUSSION && state != GameState.DISCUSSION)
                    {
                        var exiledPlayerId = -1;
                        if (state == GameState.TASKS) 
                            exiledPlayerId = ProcessMemory.getInstance().ReadWithDefault<byte>(GameAssemblyPtr, 255,
                                CurrentOffsets.ExiledPlayerIdOffsets);
                        int impostorCount = 0, innocentCount = 0;
                        bool disconnect = true;
                        for (var i = 0; i < playerCount; i++)
                        {
                            var pi = ProcessMemory.getInstance().Read<PlayerInfo>(playerAddrPtr, 0, 0);
                            playerAddrPtr += 4;

                            if (pi.PlayerId == exiledPlayerId)
                            {
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerAction.Exiled,
                                    Name = pi.GetPlayerName(),
                                    IsDead = pi.GetIsDead(),
                                    Disconnected = pi.GetIsDisconnected(),
                                    Color = pi.GetPlayerColor()
                                });
                                if (playerIsDead[exiledPlayerId] != 11 && playerIsDead[exiledPlayerId] != -11)
                                {
                                    disconnect = false;
                                    playerIsDead[pi.PlayerId] = 11;
                                    PlayerPoses[pi.PlayerId] = centerOfTable[(int)playMap];
                                    DeadLogList.Add(new DeadLog(gameStartTime, pi.PlayerId, centerOfTable[(int)playMap]));
                                }
                                else playerIsDead[exiledPlayerId] = -11;

                            }
                            else if (playerIsDead[pi.PlayerId] > 0) playerIsDead[pi.PlayerId] = -playerIsDead[pi.PlayerId];

                            // skip invalid, dead and exiled players
                            if (pi.PlayerName == 0 || pi.PlayerId == exiledPlayerId || pi.IsDead == 1 ||
                                pi.Disconnected == 1) continue;

                            if (pi.IsImpostor == 1)
                                impostorCount++;
                            else
                                innocentCount++;
                        }

                        if (impostorCount == 0 || impostorCount >= innocentCount)
                        {
                            exileCausesEnd = true;

                            if (impostorCount == 0)
                            {
                                if (disconnect)
                                    exileCausesEndState = GameState.HumansWinByDisconnect;
                                else
                                    exileCausesEndState = GameState.HumansWinByVote;
                            }
                            else
                            {
                                if (disconnect)
                                    exileCausesEndState = GameState.ImpostorWinByDisconnect;
                                else
                                    exileCausesEndState = GameState.ImpostorWinByVote;
                            }
                            state = exileCausesEndState;
                        }
                    }
                    #region amonguscapture
                    if (state != oldState || shouldForceTransmitState)
                    {
                        GameStateChanged?.Invoke(this, new GameStateChangedEventArgs { NewState = state });
                        shouldForceTransmitState = false;
                        Console.WriteLine($"GameState Changed: {state}");
                    }

                    if (state != oldState && state == GameState.LOBBY)
                    {
                        shouldReadLobby = true; // will eventually transmit
                    }


                    if (oldState == GameState.ENDED && (state == GameState.LOBBY || state == GameState.MENU)) // game ended
                    {
                        int rawGameOverReason = ProcessMemory.getInstance()
                            .Read<int>(GameAssemblyPtr, CurrentOffsets.RawGameOverReasonOffsets);
                        gameOverReason = (GameOverReason)rawGameOverReason;

                        bool humansWon = rawGameOverReason <= 1 || rawGameOverReason == 5;
                        if (humansWon) // we will be reading humans data, so set all to simps
                        {
                            foreach (string playerName in CachedPlayerInfos.Keys)
                            {
                                try
                                {
                                    CachedPlayerInfos[playerName].IsImpostor = true;
                                }
                                catch (KeyNotFoundException e)
                                {
                                    Console.WriteLine($"Could not find User: \"{playerName}\" in CachedPlayerinfos");
                                }

                            }
                        }

                        var winningPlayersPtr = ProcessMemory.getInstance()
                            .Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.WinningPlayersPtrOffsets);
                        var winningPlayers = ProcessMemory.getInstance().Read<IntPtr>(winningPlayersPtr, CurrentOffsets.WinningPlayersOffsets);
                        var winningPlayerCount = ProcessMemory.getInstance().Read<int>(winningPlayersPtr, CurrentOffsets.WinningPlayerCountOffsets);

                        var winnerAddrPtr = winningPlayers + 0x10;

                        for (var i = 0; i < winningPlayerCount; i++)
                        {
                            WinningPlayerData wpi = ProcessMemory.getInstance()
                                .Read<WinningPlayerData>(winnerAddrPtr, 0, 0);
                            winnerAddrPtr += 4;
                            try
                            {
                                CachedPlayerInfos[wpi.GetPlayerName()].IsImpostor = wpi.IsImpostor;
                            }
                            catch (KeyNotFoundException e)
                            {
                                Console.WriteLine($"Could not find player with name \"{wpi.GetPlayerName()}\" in CachedPlayerInfos. JSON: {JsonConvert.SerializeObject(CachedPlayerInfos, Formatting.Indented)}");
                            }

                        }

                        ImmutablePlayer[] endingPlayerInfos = new ImmutablePlayer[CachedPlayerInfos.Count];
                        CachedPlayerInfos.Values.CopyTo(endingPlayerInfos, 0);

                        GameOver?.Invoke(this, new GameOverEventArgs
                        {
                            GameOverReason = gameOverReason,
                            PlayerInfos = endingPlayerInfos
                        });
                        Console.WriteLine($"{gameOverReason}");
                    }

                    GameState cachedOldState = oldState;

                    oldState = state;


                    newPlayerInfos.Clear();
#endregion
                    playerAddrPtr = allPlayers + 0x10;
                    Int32 gametimeMili = (Int32)((DateTime.Now.Ticks - gameStartTime) / TimeSpan.TicksPerMillisecond);
                    AllImposterNum = 0;
                    var moveable = false;



                    for (var i = 0; i < playerCount; i++)
                    {
                        var pi = ProcessMemory.getInstance().Read<PlayerInfo>(playerAddrPtr, 0, 0);
                        playerAddrPtr += 4;
                        string playerName = pi.GetPlayerName();
                        if (playerName == null) playerName = "";
                        //if (playerName.Length == 0) continue;
                        int id = pi.PlayerId;
                        if (Math.Abs(playerIsDead[id]) < 10)
                        {
                            if (pi.GetIsDead()) playerIsDead[id] = 1;
                            else if (pi.GetIsDisconnected()) playerIsDead[id] = 10;
                            else playerIsDead[id] = 0;
                        }


                        var pcontrol = ProcessMemory.getInstance().Read<PlayerControl>((IntPtr)pi._object);
                        if ((!pi.GetIsDisconnected() && pi.IsImpostor != 1) || pcontrol.myLight_ != 0)
                        {
                            IntPtr tasklist = (IntPtr)ProcessMemory.getInstance().Read<Int32>((IntPtr)pcontrol.myTasks, 8);

                            TaskNum[id] = ProcessMemory.getInstance().Read<Int32>((IntPtr)pi.Tasks, 12);
                            if (TaskNum[id] != 0)
                            {
                                TaskProgress[id] = 0;
                                
                                if (pcontrol.myLight_ != 0)
                                {
                                    if (pi.GetIsDead()) tasklist += 4;
                                    IntPtr p = (IntPtr)ProcessMemory.getInstance().Read<Int32>(tasklist, 0x10 + 4 * (pi.IsImpostor + TaskNum[id]));
                                    Sabotage = ProcessMemory.getInstance().Read<TaskInfo>(p);
                                    if (Sabotage.TaskType != TaskTypes.SubmitScan && TaskNum[id] != 0 && Sabotage.TaskType == taskInfos[id][TaskNum[id] - 1].TaskType)
                                    {
                                        tasklist += 4;
                                        p = (IntPtr)ProcessMemory.getInstance().Read<Int32>(tasklist, 0x10 + 4 * (pi.IsImpostor + TaskNum[id]));
                                        Sabotage = ProcessMemory.getInstance().Read<TaskInfo>(p);
                                    }
                                }
                                for (int j = 0; j < TaskNum[id]; j++)
                                {
                                    IntPtr p = (IntPtr)ProcessMemory.getInstance().Read<Int32>(tasklist, 0x10 + 4 * j);
                                    var oldinfo = taskInfos[id][j].TaskType;
                                    taskInfos[id][j] = ProcessMemory.getInstance().Read<TaskInfo>(p);


                                    if (taskInfos[id][j].AllStepNum != 0)
                                        TaskProgress[id] += (float)taskInfos[id][j].TaskStep / taskInfos[id][j].AllStepNum;

                                }
                                TaskProgress[id] /= TaskNum[id];

                            }
                            Taskcompleted[id] = TaskProgress[id] == 1;
                        }
                        else
                        {
                            TaskProgress[id] = 1;
                            Taskcompleted[id] = true;
                        }



                        if (state != GameState.DISCUSSION && !pi.GetIsDisconnected() && (playerIsDead[id]==0 || playerIsDead[id]<-10))
                        {
                            int offset = 0x3C;
                            if (pcontrol.myLight_ != 0) offset = 0x50;
                            PlayerPoses[id] = ProcessMemory.getInstance().Read<Vector2>((IntPtr)(pcontrol.NetTransform + offset));
                            newPlayerCon[id] = pcontrol;
                            moveable = pcontrol.moveable;
                            //IsImpostorLis[pi.PlayerId] = pi.IsImpostor == 1;
                        }
                        if (playerIsDead[id] == 11) PlayerPoses[id] = centerOfTable[(int)playMap];
                        if (pi.IsImpostor == 1) AllImposterNum++;
                        #region amonguscapture
                        newPlayerInfos[playerName] = pi; // add to new playerinfos for comparison later

                        if (!oldPlayerInfos.ContainsKey(playerName)) // player wasn't here before, they just joined
                        {
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                            {
                                Action = PlayerAction.Joined,
                                Name = playerName,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                            PlayerCosmeticChanged?.Invoke(this, new PlayerCosmeticChangedEventArgs
                            {
                                Name = playerName,
                                HatId = pi.HatId,
                                SkinId = pi.SkinId,
                                PetId = pi.PetId
                            });
                        }
                        else
                        {
                            // player was here before, we have an old playerInfo to compare against
                            var oldPlayerInfo = oldPlayerInfos[playerName];
                            if (!oldPlayerInfo.GetIsDead() && pi.GetIsDead()) // player just died
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerAction.Died,
                                    Name = playerName,
                                    IsDead = pi.GetIsDead(),
                                    Disconnected = pi.GetIsDisconnected(),
                                    Color = pi.GetPlayerColor()
                                });

                            if (oldPlayerInfo.ColorId != pi.ColorId)
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerAction.ChangedColor,
                                    Name = playerName,
                                    IsDead = pi.GetIsDead(),
                                    Disconnected = pi.GetIsDisconnected(),
                                    Color = pi.GetPlayerColor()
                                });

                            if (!oldPlayerInfo.GetIsDisconnected() && pi.GetIsDisconnected())
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerAction.Disconnected,
                                    Name = playerName,
                                    IsDead = pi.GetIsDead(),
                                    Disconnected = pi.GetIsDisconnected(),
                                    Color = pi.GetPlayerColor()
                                });

                            if (oldPlayerInfo.HatId != pi.HatId || oldPlayerInfo.SkinId != pi.SkinId || oldPlayerInfo.PetId != pi.PetId)
                                PlayerCosmeticChanged?.Invoke(this, new PlayerCosmeticChangedEventArgs
                                {
                                    Name = playerName,
                                    HatId = pi.HatId,
                                    SkinId = pi.SkinId,
                                    PetId = pi.PetId
                                });
                        }
                    }
                    #endregion

                    //impostor set
                    for (int i = 0; i < AllImposterNum; i++)
                    {
                        var id = ImpostorId[i];

                        InVent[i] = newPlayerCon[id].inVent;
                    }

                    //dead set

                    for (var i = 0; i < playerCount; i++)
                    {
                        if (Math.Abs(playerIsDead[i]) == 1)
                        {
                            DeadLogList.Add(new DeadLog(gameStartTime, i, PlayerPoses[i]));
                        } else if(playerIsDead[i] == 10)
                        {
                            DeadLogList.Add(new DeadLog(gameStartTime, i, PlayerPoses[i]));
                            playerIsDead[i] = -10;
                        }
                        if (playerIsDead[i] == 1)
                        {
                            DeadBodyPosList.Insert(0, new DeadBodyPos(i, PlayerPoses, ImpostorId, playerIsDead, AllImposterNum));
                            if (DeadBodyPosList[0].ImpostorDists.Count == 0)
                            {
                                playerIsDead[i] = 15;
                                DeadBodyPosList.RemoveAt(0);
                            }
                            else playerIsDead[i] = DeadBodyPosList[0].ImpostorDists[0].PlayerId + 20;
                        }
                    }


                    if (CurrentOffsets.ShipStatusPtr != null)
                    {
                        var shipStatus = ProcessMemory.getInstance().Read<ShipStatus>(GameAssemblyPtr, CurrentOffsets.ShipStatusPtr);
                        var doorsPtr = (IntPtr)(shipStatus.AllDoors + 0x10);
                        int doorNum = ProcessMemory.getInstance().Read<Int32>((IntPtr)shipStatus.AllDoors, 0xC);
                        if (doors==null || doors.Length != doorNum) doors = new Door[doorNum];
                        for(int i = 0; i < doorNum; i++)
                        {
                            doors[i] = ProcessMemory.getInstance().Read<Door>(doorsPtr, 4*i, 0);
                        }
                        if (playMap == PlayMap.Skeld)
                            doorsUint = Doors.skeld.Doors2Uint(doors);
                        else if (playMap == PlayMap.Polus)
                            doorsUint = Doors.polus.Doors2Uint(doors);
                    }


                    #region amonguscapture
                    foreach (var kvp in oldPlayerInfos)
                    {
                        var pi = kvp.Value;
                        var playerName = kvp.Key;
                        if (!newPlayerInfos.ContainsKey(playerName)) // player was here before, isn't now, so they left
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                            {
                                Action = PlayerAction.Left,
                                Name = playerName,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                    }

                    oldPlayerInfos.Clear();

                    var emitAll = false;
                    if (shouldForceUpdatePlayers)
                    {
                        shouldForceUpdatePlayers = false;
                        emitAll = true;
                    }


                    if (state != cachedOldState && (state == GameState.DISCUSSION || state == GameState.TASKS)
                    ) // game started, or at least we're still in game
                    {
                        CachedPlayerInfos.Clear();
                        foreach (var kvp in newPlayerInfos
                        ) // do this instead of assignment so they don't point to the same object
                        {
                            var pi = kvp.Value;
                            string playerName = pi.GetPlayerName();
                            CachedPlayerInfos[playerName] = new ImmutablePlayer()
                            {
                                Name = playerName,
                                IsImpostor = false
                            };
                        }
                    }

                    foreach (var kvp in newPlayerInfos
                    ) // do this instead of assignment so they don't point to the same object
                    {
                        var pi = kvp.Value;
                        oldPlayerInfos[kvp.Key] = pi;
                        if (emitAll)
                            PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                            {
                                Action = PlayerAction.ForceUpdated,
                                Name = kvp.Key,
                                IsDead = pi.GetIsDead(),
                                Disconnected = pi.GetIsDisconnected(),
                                Color = pi.GetPlayerColor()
                            });
                    }

                    if (shouldReadLobby)
                    {
                        var gameCode = ProcessMemory.getInstance().ReadString(ProcessMemory.getInstance().Read<IntPtr>(
                            GameAssemblyPtr,
                            CurrentOffsets.GameCodeOffsets), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                        string[] split;
                        if (!string.IsNullOrEmpty(gameCode) && (split = gameCode.Split('\n')).Length == 2)
                        {
                            PlayRegion region = (PlayRegion)((4 - (ProcessMemory.getInstance()
                                .Read<int>(GameAssemblyPtr, CurrentOffsets.PlayRegionOffsets) & 0b11)) % 3); // do NOT ask

                            //Recheck for GameOptionsOffset
                            PlayMap map = (PlayMap)ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.PlayMapOffsets);

                            playMap = map;
                            this.latestLobbyEventArgs = new LobbyEventArgs()
                            {
                                LobbyCode = split[1],
                                Region = region,
                                Map = map,
                            };
                            shouldReadLobby = false;
                            shouldTransmitLobby = true; // since this is probably new info
                        }
                    }

                    if (shouldTransmitLobby)
                    {
                        if (this.latestLobbyEventArgs != null)
                        {
                            JoinedLobby?.Invoke(this, this.latestLobbyEventArgs);
                        }

                        shouldTransmitLobby = false;
                    }
#endregion



                    //game start check and init
                    //if (oldAllImposterNum != AllImposterNum)
                    {
                        if (state == GameState.MENU || state == GameState.LOBBY)
                        {
                            if (playing)
                            {
                                playing = false;
                                foreach (var log in DeadLogList) Console.WriteLine(log.ToString());
                                initConInfo();
                            }
                        }
                        else if (!playing && AllImposterNum != 0 && moveable)
                        {
                            playing = true;
                            BeginWrite();
                            gametimeMili = 0;
                        }
                    }

                    //send move data 

                    if (state == GameState.ENDED)
                    {
                        Thread.Sleep(1000);
                        int rawGameOverReason = ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.RawGameOverReasonOffsets);
                        gameOverReason = (GameOverReason)rawGameOverReason;
                        state = (GameState)(6 + rawGameOverReason);
                    }
                    var move = new PlayerMoveArgs
                    {
                        time = gametimeMili,
                        state = state,
                        PlayerNum = playerCount,
                        PlayerColors = PlayerColors,
                        PlayerNames = PlayerNames,
                        IsImpostor = IsImpostorLis,
                        PlayerPoses = PlayerPoses,
                        PlayerIsDead = playerIsDead,
                        ImpostorId = ImpostorId,
                        InVent = InVent,
                        TaskProgress = TaskProgress,
                        Sabotage = Sabotage,
                        myId = myId,
                        doorsUint = doorsUint
                    };
                    int frame_now = (int)Math.Round(gametimeMili / 100.0);
                    if (frame_now - frame < 5)
                        for (; frame < frame_now; frame++) PlayerMove?.Invoke(this, move);
                    else PlayerMove?.Invoke(this, move);
                    frame = frame_now;
                   Thread.Sleep((frame_now + 1) * 100 - gametimeMili);

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Message: {e.Message} | stack: {e.StackTrace} | Retrying in 1000ms.", "ERROR");
                    Console.WriteLine(e);
                    Thread.Sleep(1000);
                }
            }
        }
        int frame = 0;
        //writer開始、playerIsDead,DeadBodyPosList,DeadLogList初期化、PlayerNames,IsImpostorLis,PlayerColors,PlayerColorsInt,ImpostorId設定
        private void BeginWrite()
        {
            bool dataCompleted = false;
            int repeatcount = 0;
            Thread.Sleep(100);
            initConInfo();
            Console.WriteLine("\n\nStart\n\n");
            frame = -1;
            var ImposterNum = 0;

            var allPlayersPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
            var playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);

            while (playerCount > 10 && repeatcount < 50)
            {
                Console.WriteLine($"Wrong data : Player count is {playerCount}. | Retrying in 100ms");
                Thread.Sleep(100);
                allPlayersPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);      
                playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);     
                repeatcount++;
            }
            if (repeatcount > 50)
            {
                Console.WriteLine($"Wrong data : Player count is {playerCount}. | Continue");
                playerCount = 10;
            }

            var allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
            var playerAddrPtr = allPlayers + 0x10;

            uint[] HatIds = new uint[10];
            uint[] PetIds = new uint[10];
            uint[] SkinIds = new uint[10];

            while (!dataCompleted)
            {
                gameStartTime = DateTime.Now.Ticks;
                filename = DateTime.Now.ToString("yyyyMMdd_HHmm_ss");
                ImposterNum = 0;
                for (var i = 0; i < playerCount; i++)
                {
                    var pi = ProcessMemory.getInstance().Read<PlayerInfo>(playerAddrPtr, 0, 0);
                    playerAddrPtr += 4;
                    int id = pi.PlayerId;
                    PlayerNames[id] = pi.GetPlayerName();
                    if (PlayerNames[id] == null) PlayerNames[id] = "";
                    PlayerName2IDdict[PlayerNames[id]] = pi.PlayerId;
                    IsImpostorLis[id] = pi.IsImpostor == 1;
                    var col = (int)pi.GetPlayerColor();
                    if (col >= 0 && col < 12)
                    {
                        PlayerColors[id] = ColorList[col];
                        PlayerColorsInt[id] = (PlayerColor)col;
                    }
                    else PlayerColors[id] = Color.Empty;
                    if (pi.IsImpostor == 1)
                    {
                        filename += "_" + PlayerNames[id];
                        ImpostorId[ImposterNum] = id;
                        ImposterNum++;
                    }
                    HatIds[id] = pi.HatId;
                    PetIds[id] = pi.PetId;
                    SkinIds[id] = pi.SkinId;

                    var pcontrol = ProcessMemory.getInstance().Read<PlayerControl>((IntPtr)pi._object);

                    int offset = 0x3C;
                    if (pcontrol.myLight_ != 0) offset = 0x50;
                    PlayerPoses[id] = ProcessMemory.getInstance().Read<Vector2>((IntPtr)(pcontrol.NetTransform + offset));
                    newPlayerCon[id] = pcontrol;
                    if (pcontrol.myLight_ != 0) myId = id;
                }

                playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);
                dataCompleted = true;
                for (int i = 0; i < playerCount; i++)
                {
                    if (PlayerColors[i] == Color.Empty) dataCompleted = false;
                }
                
                if (!dataCompleted)
                {
                    repeatcount++;
                    if (repeatcount > 50) break;
                    Console.WriteLine($"Wrong data : Empty player color | Retrying in 100ms");
                    Thread.Sleep(100);
                    allPlayersPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
                    allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
                    playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);
                    playerAddrPtr = allPlayers + 0x10; 
                }
            }

            if (repeatcount > 50) Console.WriteLine($"Wrong data : Empty player color | Continue");

            var s = "";
            for (int i = 0; i < playerCount; i++)
            {
                s += $"{PlayerNames[i]}/{PlayerColorsInt[i]}, ";
            }
            Console.WriteLine(s);
            playMap = (PlayMap)ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.PlayMapOffsets);


            var move = new PlayerMoveArgs
            {
                time = 0,
                state = GameState.TASKS,
                PlayerNum = playerCount,
                PlayerColors = PlayerColors,
                PlayerNames = PlayerNames,
                IsImpostor = IsImpostorLis,
                PlayerPoses = PlayerPoses,
                PlayerIsDead = playerIsDead,
                ImpostorId = ImpostorId,
                InVent = InVent,
                TaskProgress = TaskProgress,
                Sabotage = Sabotage,
                myId = myId,
                doorsUint = doorsUint
            };


            GameStart?.Invoke(this, new GameStartEventArgs
            {
                filename = filename,
                PlayMap = playMap,
                PlayerMove = move,
                HatIds = HatIds,
                PetIds = PetIds,
                SkinIds =SkinIds
        }) ;


        }

        static public Color[] ColorList = new Color[12]
        {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.HotPink,
            Color.Orange,
            Color.Yellow,
            Color.Black,
            Color.White,
            Color.BlueViolet,
            Color.Brown,
            Color.Cyan,
            Color.Lime
        };


        public void ForceTransmitLobby()
        {
            shouldTransmitLobby = true;
        }

        public void ForceUpdatePlayers()
        {
            shouldForceUpdatePlayers = true;
        }

        public void ForceTransmitState()
        {
            shouldForceTransmitState = true;
        }
    }

    public class GameStateChangedEventArgs : EventArgs
    {
        public GameState NewState { get; set; }
    }

    public enum PlayerAction
    {
        Joined,
        Left,
        Died,
        ChangedColor,
        ForceUpdated,
        Disconnected,
        Exiled
    }

    public enum PlayerColor
    {
        Red = 0,
        Blue = 1,
        Green = 2,
        Pink = 3,
        Orange = 4,
        Yellow = 5,
        Black = 6,
        White = 7,
        Purple = 8,
        Brown = 9,
        Cyan = 10,
        Lime = 11
    }

    public enum PlayRegion
    {
        NorthAmerica = 0,
        Asia = 1,
        Europe = 2
    }
    
    public enum PlayMap
    {
        Skeld = 0,
        Mira = 1,
        Polus = 2
    }

    public class PlayerCosmeticChangedEventArgs : EventArgs
    {
        public string Name { get; set; }
        public uint HatId { get; set; } 
        public uint SkinId { get; set; }
        public uint PetId { get; set; }
    }

    public class PlayerChangedEventArgs : EventArgs
    {
        public PlayerAction Action { get; set; }
        public string Name { get; set; }
        public bool IsDead { get; set; }
        public bool Disconnected { get; set; }
        public PlayerColor Color { get; set; }
    }

    public class ChatMessageEventArgs : EventArgs
    {
        public string Sender { get; set; }
        public PlayerColor Color { get; set; }
        public string Message { get; set; }
    }

    public class LobbyEventArgs : EventArgs
    {
        public string LobbyCode { get; set; }
        public PlayRegion Region { get; set; }
        public PlayMap Map { get; set; }
    }

    public class GameOverEventArgs : EventArgs
    {
        public GameOverReason GameOverReason { get; set; }
        public ImmutablePlayer[] PlayerInfos { get; set; }
    }

    public class GameStartEventArgs : EventArgs
    {
        public string filename;
        public PlayMap PlayMap;
        public PlayerMoveArgs PlayerMove;
        public uint[] HatIds;
        public uint[] PetIds;
        public uint[] SkinIds;
    }





    public class PlayerMoveArgs : EventArgs
    {
        public Int32 time;

        public GameState state;
        public int PlayerNum { get; set; }
        public Color[] PlayerColors { get; set; }
        public string[] PlayerNames { get; set; }
        public bool[] IsImpostor { get; set; }
        public Vector2[] PlayerPoses { get; set; }
        public int[] PlayerIsDead { get; set; }
        public int[] ImpostorId { get; set; }
        public bool[] InVent { get; set; }
        public float[] TaskProgress { get; set; }

        public TaskInfo Sabotage;

        public int myId;

        public UInt32 doorsUint;

    }
   
 


    public class DeadLog
    {
        public static GameMemReader memReader;

        public int time;
        public int DeadPlayerID;
        public string DeadPlayerName;
        public string DeadPlayerColor;
        public Vector2 DeadPos;

        public DeadLog(long startTime, int deadId, Vector2 pos)
        {
            time = (Int32)((DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond);
            DeadPlayerID = deadId;
            DeadPlayerName = memReader.PlayerNames[deadId];
            DeadPlayerColor = memReader.PlayerColorsInt[deadId].ToString();
            DeadPos = memReader.PlayerPoses[deadId];
        }

        public override string ToString()
        {
            int m = (int)(time / 60000);
            int s = (int)(time / 1000) - m * 60;
            string st = $"{m,2}:{s,2}: {DeadPlayerName}/{DeadPlayerColor} ";
            int cause = Math.Abs(memReader.playerIsDead[DeadPlayerID]);

            if (cause == 11) st += "was ejected";
            else if (cause >= 20) st += $"was killed by {memReader.PlayerNames[cause - 20]}/{memReader.PlayerColorsInt[cause - 20]}";
            else if (cause == 15) st += "was killed by someone";
            else if (cause > 0) st += "was disconnected";
            else st += "was alive";

            return st;
        }
    }
    public class DeadBodyPos
    {
        public int KilledPlayerId;
        public int NearestImpostorId;
        public Vector2 BodyPos;
        public List<ImpostorDist> ImpostorDists = new List<ImpostorDist>();

        public int timeCountFromDeath = 0;


        public struct ImpostorDist
        {
            public int PlayerId;
            public float distance;
        }

        public DeadBodyPos(int gKilledPlayerId, Vector2[] PosList,int[] ImpostorId,int[] IsDead, int AllImpNum)
        {
            KilledPlayerId = gKilledPlayerId;
            BodyPos = PosList[KilledPlayerId];
            for (int i = 0; i < AllImpNum; i++)
            {
                var imDist = new ImpostorDist();
                imDist.PlayerId = ImpostorId[i];
                imDist.distance = (PosList[ImpostorId[i]] - BodyPos).LengthSquared();
                if (imDist.distance < 25.0f && IsDead[ImpostorId[i]] == 0 ) ImpostorDists.Add(imDist);
            }
            ImpostorDists.Sort((a, b) => (a.distance > b.distance) ? 1 : (a.distance < b.distance) ? -1 : 0);
            if (ImpostorDists.Count != 0) NearestImpostorId = ImpostorDists[0].PlayerId;
        }
    }

    public class ProcessHookArgs : EventArgs
    {
        public int PID { get; set; }
    }
}
