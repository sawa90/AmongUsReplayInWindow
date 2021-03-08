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
        ImpostorWinByDisconnect,
        VotingResult
    }

    public class GameMemReader
    {
        #region value
        private static readonly GameMemReader instance = new GameMemReader();
        private bool exileCausesEnd;

        //private bool shouldReadLobby = false;
        private IntPtr GameAssemblyPtr = IntPtr.Zero;
        /*
        public Dictionary<string, PlayerInfo>
            newPlayerInfos =
                new Dictionary<string, PlayerInfo>(
                    10); // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed
        */
        private LobbyEventArgs latestLobbyEventArgs = null;
        /*
        public Dictionary<string, PlayerInfo>
            oldPlayerInfos =
                new Dictionary<string, PlayerInfo>(
                    10); // Important: this is making the assumption that player names are unique. They are, but for better tracking of players and to eliminate any ambiguity the keys of this probably need to be the players' network IDs instead

        private Dictionary<string, ImmutablePlayer> CachedPlayerInfos = new Dictionary<string, ImmutablePlayer>();
        */
        private GameState oldState = GameState.UNKNOWN;

        private int prevChatBubsVersion;
        //private bool shouldForceTransmitState;
        //private bool shouldForceUpdatePlayers;
        //private bool shouldTransmitLobby;

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

        Vector2[] centerOfTable = new Vector2[3] { new Vector2(-1.0f, 1.1f), new Vector2(24.03f,2.625f), new Vector2(19.5f,-16.876f)};

        GameOverReason gameOverReason = GameOverReason.Unknown;
        GameState exileCausesEndState = GameState.Unknown;

        PlayMap playMap = PlayMap.Skeld;
        public string filename = null;
        int myId = 0;
        int[] IdList = new int[10];

        Door[] doors;
        UInt32 doorsUint = 0;

        Int32 discussionEndTime = -10000;
        sbyte[] voteList = new sbyte[10];
        IntPtr lastChatBubblePtr = IntPtr.Zero;
        int lastChatBubbleIdx = 0;
        bool loadLastchatNextturn = false;
        #endregion

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
                voteList[i] = -3;
                discussionEndTime = -10000;
                IdList[i] = i;
            }
            Sabotage = new TaskInfo();
            DeadBodyPosList.Clear();
            DeadLogList.Clear();
            PlayerName2IDdict.Clear();
            gameOverReason = GameOverReason.Unknown;
            doorsUint = 0;
            lastChatBubblePtr = IntPtr.Zero;
            lastChatBubbleIdx = 0;
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

                    Int32 gametimeMili = (Int32)((DateTime.Now.Ticks - gameStartTime) / TimeSpan.TicksPerMillisecond);

                    //get voting state
                    if (state == GameState.DISCUSSION || (state == GameState.TASKS && (gametimeMili - discussionEndTime) < 7000))
                    {

                        if (state == GameState.DISCUSSION) discussionEndTime = gametimeMili;
                        else state = GameState.VotingResult;
                        if (meetingHud != IntPtr.Zero)
                        {
                            var voteInfoPtr = ProcessMemory.getInstance().Read<IntPtr>(meetingHud, CurrentOffsets.PlayerVoteAreaListPtr);
                            if (voteInfoPtr != IntPtr.Zero)
                            {
                                var votePlayerCount = ProcessMemory.getInstance().Read<int>((IntPtr)voteInfoPtr, 0xC);
                                voteInfoPtr += 0x10;
                                if (CurrentOffsets.before20201209s)
                                {
                                    Struct_2020_12_9s.v_PlayerVoteArea voteArea;
                                    for (int i = 0; i < votePlayerCount; i++)
                                    {
                                        voteArea = ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerVoteArea>(voteInfoPtr, 4 * i, 0);

                                        int id = IdList[voteArea.Id_];
                                        //if (id != i)
                                        //    Console.Write($"{id}!={ i}   ");
                                        if (voteArea.didVote)
                                        {
                                            sbyte votedId = voteArea.votedFor;
                                            if (votedId >= 0 && votedId < 10) votedId = (sbyte)IdList[votedId];
                                            voteList[id] = votedId;
                                        }
                                        else voteList[id] = -3;
                                        if (voteArea.didReport) voteList[id] += 32;
                                    }
                                }
                                else
                                {
                                    Struct_2021_3_5s.v_PlayerVoteArea voteArea;
                                    for (int i = 0; i < votePlayerCount; i++)
                                    {
                                        voteArea = ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerVoteArea>(voteInfoPtr, 4 * i, 0);

                                        int id = IdList[voteArea.Id_];
                                        //if (id != i)
                                        //    Console.Write($"{id}!={ i}   ");
                                        if (voteArea.didVote)
                                        {
                                            sbyte votedId = voteArea.votedFor;
                                            if (votedId >= 0 && votedId < 10) votedId = (sbyte)IdList[votedId];
                                            voteList[id] = votedId;
                                        }
                                        else voteList[id] = -3;
                                        if (voteArea.didReport) voteList[id] += 32;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 10; i++) voteList[i] = -3;
                        }
                    }

                    // check if exile causes end
                    if (oldState == GameState.DISCUSSION && state != GameState.DISCUSSION)
                    {
                        /*
                        for (int i = 0; i < playerCount; i++)
                        {
                            int votedId = voteList[i];
                            if (votedId > 20) votedId -= 32;
                            if (playerIsDead[i] == 0)
                            {
                                if (votedId == 14 || votedId == -2)
                                    Console.WriteLine($"{PlayerNames[i]}/{PlayerColors[i]}->Not Vote {votedId}");
                                
                                else if (votedId > 9)
                                    Console.WriteLine($"{PlayerNames[i]}/{PlayerColors[i]}->Error ID:{votedId}");
                                else if (votedId >= 0)
                                    Console.WriteLine($"{PlayerNames[i]}/{PlayerColors[i]}->{PlayerNames[votedId]}/{PlayerColorsInt[votedId]}");
                                else if (votedId == -1)
                                    Console.WriteLine($"{PlayerNames[i]}/{PlayerColors[i]}->Skip");
                                else
                                    Console.WriteLine($"{PlayerNames[i]}/{PlayerColors[i]}->Error ID:{votedId}");
                            }
                        }*/

                        var exiledPlayerId = -1;
                        //if (state == GameState.TASKS) 
                            exiledPlayerId = ProcessMemory.getInstance().ReadWithDefault<byte>(GameAssemblyPtr, 255,
                                CurrentOffsets.ExiledPlayerIdOffsets);
                        int impostorCount = 0, innocentCount = 0;
                        bool disconnect = true;
                        for (var i = 0; i < playerCount; i++)
                        {
                            PlayerInfo pi = CurrentOffsets.before20201209s ? ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerInfo>(playerAddrPtr, 0, 0) : ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerInfo>(playerAddrPtr, 0, 0);
                            playerAddrPtr += 4;
                            var id = IdList[pi.PlayerId];
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
                                if (playerIsDead[id] != 11 && playerIsDead[id] != -11)
                                {
                                    disconnect = false;
                                    playerIsDead[id] = 11;
                                    DeadLogList.Add(new DeadLog(gameStartTime, id, centerOfTable[(int)playMap]));
                                }
                                else playerIsDead[id] = -11;

                            }
                            else if (playerIsDead[id] > 0) playerIsDead[id] = -playerIsDead[id];

                            // skip invalid, dead and exiled players
                            if (pi.PlayerName == IntPtr.Zero || pi.PlayerId == exiledPlayerId || pi.IsDead == 1 ||
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
                    if (state != oldState)
                    {
                        GameStateChanged?.Invoke(this, new GameStateChangedEventArgs { NewState = state });
                        Console.WriteLine($"GameState Changed: {state}");
                    }
                    /*
                    if (state == oldState || shouldForceTransmitState)
                    {
                        GameStateChanged?.Invoke(this, new GameStateChangedEventArgs { NewState = state });
                        shouldForceTransmitState = false;
                        Console.WriteLine($"GameState Changed: {state}");
                    }

                    if (state != oldState && state == GameState.LOBBY)
                    {
                        shouldReadLobby = true; // will eventually transmit
                    }
                    */

                    if (oldState == GameState.ENDED && (state == GameState.LOBBY || state == GameState.MENU)) // game ended
                    {
                        int rawGameOverReason = ProcessMemory.getInstance()
                            .Read<int>(GameAssemblyPtr, CurrentOffsets.RawGameOverReasonOffsets);
                        gameOverReason = (GameOverReason)rawGameOverReason;
                        /*
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
                        */
                        Console.WriteLine($"{gameOverReason}");
                    }

                    GameState cachedOldState = oldState;

                    oldState = state;


                    //newPlayerInfos.Clear();
#endregion
                    playerAddrPtr = allPlayers + 0x10;
                    AllImposterNum = 0;





                    for (var i = 0; i < playerCount; i++)
                    {
                        PlayerInfo pi = CurrentOffsets.before20201209s ? ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerInfo>(playerAddrPtr, 0, 0) : ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerInfo>(playerAddrPtr, 0, 0);
                        playerAddrPtr += 4;
                        string playerName = pi.GetPlayerName();
                        if (playerName == null) playerName = "";
                        //if (playerName.Length == 0) continue;
                        int id = IdList[pi.PlayerId];
                        if (Math.Abs(playerIsDead[id]) < 10)
                        {
                            if (pi.GetIsDead()) playerIsDead[id] = 1;
                            else if (pi.GetIsDisconnected()) playerIsDead[id] = 10;
                            else playerIsDead[id] = 0;
                        }


                        PlayerControl pcontrol = CurrentOffsets.before20201209s ? ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerControl>(pi._object) : ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerControl>(pi._object);

                        if ((!pi.GetIsDisconnected() && pi.IsImpostor != 1) || pcontrol.myLight_ != 0)
                        {
                            IntPtr tasklist = (IntPtr)ProcessMemory.getInstance().Read<Int32>((IntPtr)pcontrol.myTasks, 8);
                            
                            TaskNum[id] = ProcessMemory.getInstance().Read<Int32>((IntPtr)pi.Tasks, 12);
                            if (TaskNum[id] != 0)
                            {
                                TaskProgress[id] = 0;
                                
                                if (pcontrol.myLight_ != 0)
                                {
                                    if (pi.GetIsDead())
                                         tasklist += 4;
                                    if (pi.GetIsDead() && pi.IsImpostor == 1)
                                    {
                                        IntPtr p = (IntPtr)ProcessMemory.getInstance().Read<Int32>(tasklist, 0x10);
                                        Sabotage = ProcessMemory.getInstance().Read<TaskInfo>(p);
                                    }
                                    else
                                    {
                                        IntPtr p = (IntPtr)ProcessMemory.getInstance().Read<Int32>(tasklist, 0x10 + 4 * (pi.IsImpostor + TaskNum[id]));
                                        Sabotage = ProcessMemory.getInstance().Read<TaskInfo>(p);
                                        if (Sabotage.TaskType != TaskTypes.SubmitScan && TaskNum[id] != 0 && Sabotage.TaskType == taskInfos[id][TaskNum[id] - 1].TaskType)
                                        {
                                            tasklist += 4;
                                            p = (IntPtr)ProcessMemory.getInstance().Read<Int32>(tasklist, 0x10 + 4 * (pi.IsImpostor + TaskNum[id]));
                                            Sabotage = ProcessMemory.getInstance().Read<TaskInfo>(p);
                                        }
                                    }
                                }
                                if (pi.IsImpostor == 1)
                                {
                                    TaskProgress[id] = 1;
                                }
                                else
                                {
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

                            }
                            Taskcompleted[id] = TaskProgress[id] == 1;
                        }
                        else
                        {
                            TaskProgress[id] = 1;
                            Taskcompleted[id] = true;
                        }



                        if (state != GameState.DISCUSSION && !pi.GetIsDisconnected() && (playerIsDead[id]==0 || playerIsDead[id]<-10 || playerIsDead[id] == 11 ))
                        {
                            int offset = 0x3C;
                            if (pcontrol.myLight_ != 0) offset = 0x50;
                            PlayerPoses[id] = ProcessMemory.getInstance().Read<Vector2>((IntPtr)(pcontrol.NetTransform + offset));
                            newPlayerCon[id] = pcontrol;
                            //IsImpostorLis[id] = pi.IsImpostor == 1;
                        }
                        if (pi.IsImpostor == 1) AllImposterNum++;
                        #region amonguscapture
                        /*
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
                        */
                    }
                    #endregion

                    //impostor set
                    if (newPlayerCon[0] != null)
                    {
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
                            }
                            else if (playerIsDead[i] == 10)
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
                                else playerIsDead[i] = DeadBodyPosList[0].ImpostorDists[0].Id + 20;
                            }
                        }
                    }


                    if (CurrentOffsets.ShipStatusPtr != null)
                    {
                        var shipStatusPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.ShipStatusPtr);
                        var doorsPtr = (IntPtr)ProcessMemory.getInstance().Read<Int32>((IntPtr)shipStatusPtr, CurrentOffsets.DoorsPtr);
                        int doorNum = ProcessMemory.getInstance().Read<Int32>(doorsPtr, 0xC);
                        var doorsListPtr = doorsPtr + 0x10;

                        if (doors==null || doors.Length != doorNum) doors = new Door[doorNum];
                        for(int i = 0; i < doorNum; i++)
                        {
                            doors[i] = ProcessMemory.getInstance().Read<Door>(doorsListPtr, 4*i, 0);
                        }
                        if (playMap == PlayMap.Skeld)
                            doorsUint = Doors.skeld.Doors2Uint(doors);
                        else if (playMap == PlayMap.Polus)
                            doorsUint = Doors.polus.Doors2Uint(doors);
                    }

                    if (CurrentOffsets.ChatControllerPtr != null)
                    {
                        if (loadLastchatNextturn)
                        {
                            var msgPtr = ProcessMemory.getInstance().Read<IntPtr>(lastChatBubblePtr, 0x20, 0x70);
                            var senderPtr = ProcessMemory.getInstance().Read<IntPtr>(lastChatBubblePtr, 0x1C, 0x70);
                            if (senderPtr != IntPtr.Zero || msgPtr != IntPtr.Zero)
                            {
                                var msgText = ProcessMemory.getInstance().ReadString(msgPtr, 0x10, 0x14);
                                var msgSender = ProcessMemory.getInstance().ReadString(senderPtr, 0x10, 0x14);
                                ChatMessageAdded?.Invoke(this, new ChatMessageEventArgs
                                {
                                    Sender = msgSender,
                                    Message = msgText
                                });
                                if (msgText != "")
                                    Console.WriteLine(msgSender + ":\t" +msgText);
                                else
                                    Console.WriteLine(msgSender);
                            }
                            loadLastchatNextturn = false;
                        }
                        var chatControllerPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.ChatControllerPtr);
                        if (chatControllerPtr != IntPtr.Zero)
                        {
                            var chatPoolPtr = ProcessMemory.getInstance().Read<IntPtr>(chatControllerPtr, 0xc);
                            var chatBubblesPtr = ProcessMemory.getInstance().Read<IntPtr>(chatPoolPtr, 0x14);
                            var chatBubsVersion = ProcessMemory.getInstance().Read<int>(chatBubblesPtr, 0x10);
                            if (chatBubsVersion != prevChatBubsVersion)
                            {
                                var poolSize = 20;//ProcessMemory.getInstance().Read<Int32>(chatPoolPtr, 0xc);
                                var numChatBubbles = ProcessMemory.getInstance().Read<int>(chatBubblesPtr, 0xC);
                                var chatBubblesAddr = ProcessMemory.getInstance().Read<IntPtr>(chatBubblesPtr, 0x8) + 0x10;
                                var chatBubblePtrs = ProcessMemory.getInstance().ReadArray(chatBubblesAddr, numChatBubbles);

                                var newMsgs = 0;

                                if (chatBubsVersion > prevChatBubsVersion) // new message has been sent
                                {
                                    if (chatBubsVersion > poolSize) // increments are twofold (push to and pop from pool)
                                    {
                                        if (prevChatBubsVersion > poolSize)
                                            newMsgs = (chatBubsVersion - prevChatBubsVersion) >> 1;
                                        else
                                            newMsgs = poolSize - prevChatBubsVersion + ((chatBubsVersion - poolSize) >> 1);
                                    }
                                    else // single increments
                                    {
                                        newMsgs = chatBubsVersion - prevChatBubsVersion;
                                    }
                                    while (numChatBubbles - newMsgs > 0 && chatBubblePtrs[numChatBubbles - newMsgs - 1] != lastChatBubblePtr)
                                        newMsgs++;
                                }
                                else if (chatBubsVersion < prevChatBubsVersion) // reset
                                {
                                    if (chatBubsVersion > poolSize) // increments are twofold (push to and pop from pool)
                                        newMsgs = poolSize + ((chatBubsVersion - poolSize) >> 1);
                                    else // single increments
                                        newMsgs = chatBubsVersion;
                                }

                                if (numChatBubbles - newMsgs < 0) newMsgs = numChatBubbles;
                                if (numChatBubbles > 0)
                                {
                                    lastChatBubblePtr = chatBubblePtrs[numChatBubbles - 1];
                                    loadLastchatNextturn = true;
                                }
                                prevChatBubsVersion = chatBubsVersion;

                                for (var i = numChatBubbles - newMsgs; i < numChatBubbles - 1; i++)
                                {
                                    var msgText = ProcessMemory.getInstance()
                                        .ReadString(ProcessMemory.getInstance().Read<IntPtr>(chatBubblePtrs[i], 0x20, 0x70), 0x10, 0x14);
                                    var msgSender = ProcessMemory.getInstance()
                                        .ReadString(ProcessMemory.getInstance().Read<IntPtr>(chatBubblePtrs[i], 0x1C, 0x70), 0x10, 0x14);
                                    ChatMessageAdded?.Invoke(this, new ChatMessageEventArgs
                                    {
                                        Sender = msgSender,
                                        Message = msgText
                                    });
                                    if (msgText != "")
                                        Console.WriteLine(msgSender + ":\t" + msgText);
                                    else
                                        Console.WriteLine(msgSender);
                                }
                            }
                        }
                    }



                    #region amonguscapture
                    /*
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
                    */
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
                        else if (!playing && AllImposterNum != 0)
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
                        doorsUint = doorsUint,
                        voteList = voteList
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
                    PlayerInfo pi = CurrentOffsets.before20201209s ? ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerInfo>(playerAddrPtr, 0, 0) : ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerInfo>(playerAddrPtr, 0, 0);
                    playerAddrPtr += 4;
                    IdList[pi.PlayerId] = i;
                    PlayerNames[i] = pi.GetPlayerName();
                    if (PlayerNames[i] == null) PlayerNames[i] = "";
                    PlayerName2IDdict[PlayerNames[i]] = i;
                    IsImpostorLis[i] = pi.IsImpostor == 1;
                    var col = (int)pi.GetPlayerColor();
                    if (col >= 0 && col < 12)
                    {
                        PlayerColors[i] = ColorList[col];
                        PlayerColorsInt[i] = (PlayerColor)col;
                    }
                    else PlayerColors[i] = Color.Empty;
                    if (pi.IsImpostor == 1)
                    {
                        filename += "_" + PlayerNames[i];
                        ImpostorId[ImposterNum] = i;
                        ImposterNum++;
                    }
                    HatIds[i] = pi.HatId;
                    PetIds[i] = pi.PetId;
                    SkinIds[i] = pi.SkinId;

                    PlayerControl pcontrol = CurrentOffsets.before20201209s ? ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerControl>(pi._object) : ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerControl>(pi._object);

                    int offset = 0x3C;
                    if (pcontrol.myLight_ != 0) offset = 0x50;
                    PlayerPoses[i] = ProcessMemory.getInstance().Read<Vector2>((IntPtr)(pcontrol.NetTransform + offset));
                    newPlayerCon[i] = pcontrol;
                    if (pcontrol.myLight_ != 0) myId = i;
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
                    Console.WriteLine("Wrong data : Empty player color | Retrying in 100ms");
                    Thread.Sleep(100);
                    allPlayersPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
                    allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
                    playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);
                    playerAddrPtr = allPlayers + 0x10; 
                }
            }

            if (repeatcount > 50) Console.WriteLine("Wrong data : Empty player color | Continue");

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
                doorsUint = doorsUint,
                voteList = voteList
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

        static public Dictionary<int, string> ColorNameDict = new Dictionary<int, string>
            {
                { Color.Red.ToArgb(),"Red" },
                { Color.Blue.ToArgb(),"Blue" },
                { Color.Green.ToArgb(),"Green" },
                { Color.HotPink.ToArgb(),"Pink" },
                { Color.Orange.ToArgb(),"Orange" },
                { Color.Yellow.ToArgb(),"Yellow" },
                { Color.Black.ToArgb(),"Black" },
                { Color.White.ToArgb(),"White" },
                { Color.BlueViolet.ToArgb(),"Purple" },
                { Color.Brown.ToArgb(), "Brown" },
                { Color.Cyan.ToArgb(),"Cyan" },
                { Color.Lime.ToArgb(),"Lime" },
            };

        /*
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
        */
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

        public sbyte[] voteList;

    }
   
 


    public class DeadLog
    {
        public static GameMemReader memReader;

        public int time;
        public int DeadID;
        public string DeadPlayerName;
        public string DeadPlayerColor;
        public Vector2 DeadPos;

        public DeadLog(long startTime, int deadId, Vector2 pos)
        {
            time = (Int32)((DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond);
            DeadID = deadId;
            DeadPlayerName = memReader.PlayerNames[deadId];
            DeadPlayerColor = memReader.PlayerColorsInt[deadId].ToString();
            DeadPos = memReader.PlayerPoses[deadId];
        }

        public override string ToString()
        {
            int m = (int)(time / 60000);
            int s = (int)(time / 1000) - m * 60;
            string st = $"{m,2}:{s,2}: {DeadPlayerName}/{DeadPlayerColor} ";
            int cause = Math.Abs(memReader.playerIsDead[DeadID]);

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
        public int KilledId;
        public int NearestImpostorId;
        public Vector2 BodyPos;
        public List<ImpostorDist> ImpostorDists = new List<ImpostorDist>();

        public int timeCountFromDeath = 0;


        public struct ImpostorDist
        {
            public int Id;
            public float distance;
        }

        public DeadBodyPos(int gKilledId, Vector2[] PosList,int[] ImpostorId,int[] IsDead, int AllImpNum)
        {
            KilledId = gKilledId;
            BodyPos = PosList[KilledId];
            for (int i = 0; i < AllImpNum; i++)
            {
                var imDist = new ImpostorDist();
                imDist.Id = ImpostorId[i];
                imDist.distance = (PosList[ImpostorId[i]] - BodyPos).LengthSquared();
                if (imDist.distance < 25.0f && IsDead[ImpostorId[i]] == 0 ) ImpostorDists.Add(imDist);
            }
            ImpostorDists.Sort((a, b) => (a.distance > b.distance) ? 1 : (a.distance < b.distance) ? -1 : 0);
            if (ImpostorDists.Count != 0) NearestImpostorId = ImpostorDists[0].Id;
        }
    }

    public class ProcessHookArgs : EventArgs
    {
        public int PID { get; set; }
    }
}
