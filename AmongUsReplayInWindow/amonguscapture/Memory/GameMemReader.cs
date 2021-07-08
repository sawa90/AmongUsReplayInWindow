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

    public static class PlayerData
    {
        public const int MaxPlayerNum = 15;
        public const int MaxTaskNum = 15;

        public const int PlayerColorNum = 18;
        static readonly public Color[] ColorList = new Color[PlayerColorNum]{
            Color.FromArgb(245, 30, 34),//Color.Red,
            Color.FromArgb(29, 60, 233),//Color.Blue,
            Color.FromArgb(27, 145, 62),//Color.Green,
            Color.FromArgb(237, 84, 186),//Color.HotPink,
            Color.FromArgb(255, 141, 28),//Color.Orange,
            Color.FromArgb(255, 255, 103),//Color.Yellow,
            Color.FromArgb(30,  31,  38),//Color.FromArgb(63, 71, 78),//Color.Black,
            Color.FromArgb(255, 255, 255),//Color.White,
            Color.FromArgb(107, 49, 188),//Color.BlueViolet,
            Color.FromArgb(113, 73, 30),//Color.Brown,
            Color.FromArgb(68, 253, 245),//Color.Cyan,
            Color.FromArgb(80, 239, 57),//Color.Lime,
            Color.FromArgb(115, 27, 19),    //Maroon
            Color.FromArgb(236, 192, 211),//Rose = 13
            Color.FromArgb(255, 253, 190),  //Banana = 14
            Color.FromArgb(112, 132, 151),//Gray = 15
            Color.FromArgb(146, 135, 118),//Tan = 16
            Color.FromArgb(236, 117, 120),//Coral = 17
            };
        
        static readonly public Dictionary<int, Color> oldColor2newColorDict = new Dictionary<int, Color>
            {
                { Color.Red.ToArgb(),ColorList[0] },
                { Color.Blue.ToArgb(),ColorList[1] },
                { Color.Green.ToArgb(),ColorList[2] },
                { Color.HotPink.ToArgb(),ColorList[3]},
                { Color.Orange.ToArgb(),ColorList[4]},
                { Color.Yellow.ToArgb(),ColorList[5]},
                { Color.Black.ToArgb(),ColorList[6] },
                { Color.White.ToArgb(),ColorList[7]},
                { Color.BlueViolet.ToArgb(),ColorList[8]},
                { Color.Brown.ToArgb(), ColorList[9]},
                { Color.Cyan.ToArgb(),ColorList[10]},
                { Color.Lime.ToArgb(),ColorList[11] }
            };

        static readonly public Dictionary<int, string> ColorNameDict = new Dictionary<int, string>
            {
                { ColorList[0].ToArgb(),"Red" },
                { ColorList[1].ToArgb(),"Blue" },
                { ColorList[2].ToArgb(),"Green" },
                { ColorList[3].ToArgb(),"Pink" },
                { ColorList[4].ToArgb(),"Orange" },
                { ColorList[5].ToArgb(),"Yellow" },
                { ColorList[6].ToArgb(),"Black" },
                { ColorList[7].ToArgb(),"White" },
                { ColorList[8].ToArgb(),"Purple" },
                { ColorList[9].ToArgb(), "Brown" },
                { ColorList[10].ToArgb(),"Cyan" },
                { ColorList[11].ToArgb(),"Lime" },
                { ColorList[12].ToArgb(),"Maroon" },
                { ColorList[13].ToArgb(),"Rose" },
                { ColorList[14].ToArgb(), "Banana" },
                { ColorList[15].ToArgb(),"Gray" },
                { ColorList[16].ToArgb(),"Tan" },
                { ColorList[17].ToArgb(),"Coral"},
                { Color.Empty.ToArgb(),"Empty"}

            };

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
            Lime = 11,
            Maroon = 12,
            Rose = 13,
            Banana = 14,
            Gray = 15,
            Tan = 16,
            Coral = 17
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
            Polus = 2,
            dlekS = 3,
            AirShip = 4,
        }

        public static Vector2[] centerOfTable = { new Vector2(-1.0f, 1.1f), new Vector2(24.03f, 2.625f), new Vector2(19.5f, -16.876f), new Vector2(-1.0f, 1.1f), new Vector2(11, 15) };

        public static int notVote = -1;
        public static int notVoteEnd = -2;
    }

    public class GameMemReader
    {
        #region value
        private static readonly GameMemReader instance = new GameMemReader();
        private bool exileCausesEnd;

        private bool shouldReadLobby = false;
        private string LobbyCode = "";
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

        public event EventHandler<ChatMessageEventArgs> TextLogEvent;

        private bool cracked = false;

        public event EventHandler<PlayerMoveArgs> PlayerMove;
        public Vector2[] PlayerPoses = new Vector2[PlayerData.MaxPlayerNum];
        PlayerControl[] newPlayerCon = new PlayerControl[PlayerData.MaxPlayerNum]; // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        public Color[] PlayerColors = new Color[PlayerData.MaxPlayerNum];
        public PlayerData.PlayerColor[] PlayerColorsInt = new PlayerData.PlayerColor[PlayerData.MaxPlayerNum];
        public string[] PlayerNames = new string[PlayerData.MaxPlayerNum];

        //-10 disconnect, +-11 ejected, +/-29~+/-20 killed this/previous turn by ID:abs(num)-20, +/-15 killed by someone this/previous turn 
        public int[] playerIsDead = new int[PlayerData.MaxPlayerNum];
        public bool[] IsImpostorLis = new bool[PlayerData.MaxPlayerNum];


        int AllImposterNum = 0;
        int[] ImpostorId = new int[PlayerData.MaxPlayerNum];
        bool[] InVent = new bool[PlayerData.MaxPlayerNum];

        int[] TaskNum = new int[PlayerData.MaxPlayerNum];
        TaskInfo[][] taskInfos = new TaskInfo[PlayerData.MaxPlayerNum][];
        TaskInfo Sabotage;

        float[] TaskProgress = new float[PlayerData.MaxPlayerNum];
        bool[] Taskcompleted = new bool[PlayerData.MaxPlayerNum];
        GameState testState = GameState.TASKS;
        public static bool testflag = false;


        public bool playing = false;

        Dictionary<string, int> PlayerName2IDdict = new Dictionary<string, int>();

        long gameStartTime = 0;


       List<DeadBodyPos> DeadBodyPosList = new List<DeadBodyPos>();
        List<DeadLog> DeadLogList = new List<DeadLog>();

        Vector2[] centerOfTable = PlayerData.centerOfTable;

        GameOverReason gameOverReason = GameOverReason.Unknown;
        GameState exileCausesEndState = GameState.Unknown;

        PlayerData.PlayMap playMap = PlayerData.PlayMap.Skeld;
        public string filename = null;
        int myId = 0;
        int[] IdList = new int[PlayerData.MaxPlayerNum];

        Door[] doors;
        UInt32 doorsUint = 0;
        bool IsLeft = true;

        Int32 discussionEndTime = -10000;
        sbyte[] voteList = new sbyte[PlayerData.MaxPlayerNum];
        IntPtr lastChatBubblePtr = IntPtr.Zero;
        bool loadLastchatNextturn = false;
        bool firstdisc = false;
        #endregion

        public GameMemReader()
        {
            DeadLog.memReader = this;
            initConInfo();
        }


        private void initConInfo()
        {
            for (int i = 0; i < PlayerData.MaxPlayerNum; i++)
            {
                playerIsDead[i] = 0;
                PlayerColors[i] = Color.Empty;
                IsImpostorLis[i] = false;
                taskInfos[i] = new TaskInfo[PlayerData.MaxTaskNum];
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
                    if (testflag)
                    {
                        state = testState;
                        AllImposterNum = 1;
                    }


                    var allPlayersPtr =
                        ProcessMemory.getInstance()
                            .Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
                    var allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
                    var playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);
                    if (playerCount > PlayerData.MaxPlayerNum) playerCount = PlayerData.MaxPlayerNum;
                    var playerAddrPtr = allPlayers + 0x10;
                    var PlayerInfoPtrList = ProcessMemory.getInstance().ReadArray(playerAddrPtr, playerCount);

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
                                if (votePlayerCount > PlayerData.MaxPlayerNum * 2) votePlayerCount = PlayerData.MaxPlayerNum * 2;
                                voteInfoPtr += 0x10;
                                if (CurrentOffsets.StructVersion == 0)
                                {
                                    var voteAreaPtrList = ProcessMemory.getInstance().ReadArray(voteInfoPtr, votePlayerCount);
                                    Struct_2020_12_9s.v_PlayerVoteArea voteArea;
                                    for (int i = 0; i < votePlayerCount; i++)
                                    {
                                        voteArea = ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerVoteArea>(voteAreaPtrList[i], 0);

                                        int id = IdList[voteArea.Id_];
                                        //if (id != i)
                                        //    Console.Write($"{id}!={ i}   ");
                                        if (voteArea.didVote)
                                        {
                                            sbyte votedId = voteArea.votedFor;
                                            if (votedId >= 0 && votedId < PlayerData.MaxPlayerNum) votedId = (sbyte)IdList[votedId];
                                            voteList[id] = votedId;
                                        }
                                        else
                                        {
                                            if (voteArea.votedFor == PlayerData.notVoteEnd) voteList[id] = voteArea.votedFor;
                                            else voteList[id] = (sbyte)PlayerData.notVote;
                                        }
                                        if (voteArea.didReport) voteList[id] += 32;
                                    }
                                }
                                else
                                {
                                    var voteAreaPtrList = ProcessMemory.getInstance().ReadArray(voteInfoPtr, votePlayerCount);
                                    PlayerVoteArea voteArea;
                                    for (int i = 0; i < votePlayerCount; i++)
                                    {
                                        if (CurrentOffsets.StructVersion < 3)  voteArea = ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerVoteArea>(voteAreaPtrList[i], 0); 
                                        else if (CurrentOffsets.StructVersion < 4)  voteArea = ProcessMemory.getInstance().Read<Struct_2021_6_15s.v_PlayerVoteArea>(voteAreaPtrList[i], 0);
                                        else voteArea = ProcessMemory.getInstance().Read<Struct_2021_6_30s.v_PlayerVoteArea>(voteAreaPtrList[i], 0);

                                        int id = IdList[voteArea.Id_];
                                        //if (id != i)
                                        //    Console.Write($"{id}!={ i}   ");
                                        if (CurrentOffsets.StructVersion >= 3 || voteArea.didVote)
                                        {
                                            sbyte votedId = voteArea.votedFor;
                                            if (votedId >= 0 && votedId < PlayerData.MaxPlayerNum) votedId = (sbyte)IdList[votedId];
                                            voteList[id] = votedId;
                                        }
                                        else
                                        {
                                            if(voteArea.votedFor == PlayerData.notVoteEnd) voteList[id] = voteArea.votedFor;
                                            else voteList[id] = (sbyte)PlayerData.notVote;
                                        }
                                        if (voteArea.didReport) voteList[id] += 32;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < PlayerData.MaxPlayerNum; i++) voteList[i] = (sbyte)PlayerData.notVote;
                        }
                    }

                    // check if exile causes end
                    if (oldState == GameState.DISCUSSION && state != GameState.DISCUSSION)
                    {
                        string votingresult = "Voting Result:";
                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                        {
                            time = -1,
                            Sender = null,
                            Message = votingresult
                        });
                        for (int i = 0; i < playerCount; i++)
                        {
                            int votedId = voteList[i];
                            if (votedId > 20) votedId -= 32;
                            if (playerIsDead[i] == 0)
                            {
                                if (votedId == PlayerData.notVote || votedId == PlayerData.notVoteEnd)
                                    votingresult = $"->Not Vote";

                                else if (votedId >= PlayerData.MaxPlayerNum)
                                    votingresult = $"->Error ID:{votedId}";
                                else if (votedId >= 0)
                                    votingresult = $"->{PlayerNames[votedId]}/{PlayerColorsInt[votedId]}";
                                else if (votedId == -3)
                                    votingresult = $"->Skip";
                                else
                                    votingresult = $"->Error ID:{votedId}";
                                TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                {
                                    time = -1,
                                    Sender = PlayerNames[i],
                                    Message = votingresult
                                });
                                if (testflag) Console.WriteLine(PlayerNames[i] + votingresult);
                            }

                        }
                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                        {
                            time = -1,
                            Sender = null,
                            Message = ""
                        });

                        var exiledPlayerId = -1;
                        //if (state == GameState.TASKS) 
                        exiledPlayerId = ProcessMemory.getInstance().ReadWithDefault<byte>(GameAssemblyPtr, 255,
                            CurrentOffsets.ExiledPlayerIdOffsets);
                        int impostorCount = 0, innocentCount = 0;
                        bool disconnect = true;

                        for (var i = 0; i < playerCount; i++)
                        {
                            PlayerInfo pi;
                            if (CurrentOffsets.StructVersion == 0)
                                pi = ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                            else if (CurrentOffsets.StructVersion == 1)
                                pi = ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                            else if (CurrentOffsets.StructVersion < 3)
                                pi = ProcessMemory.getInstance().Read<Struct_2021_3_31_3s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                            else
                                pi = ProcessMemory.getInstance().Read<Struct_2021_6_15s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);



                            var id = IdList[pi.PlayerId];
                            if (pi.PlayerId == exiledPlayerId)
                            {
                                PlayerChanged?.Invoke(this, new PlayerChangedEventArgs
                                {
                                    Action = PlayerData.PlayerAction.Exiled,
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
                                    if (playing)
                                    {
                                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                        {
                                            time = -1,
                                            Sender = PlayerNames[id],
                                            Message = $" is ejected\n"
                                        });
                                    }
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
                    */
                    if (state != oldState && state == GameState.LOBBY)
                    {
                        shouldReadLobby = true; // will eventually transmit
                    }
                    

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
                        PlayerInfo pi;
                        if (CurrentOffsets.StructVersion == 0)
                            pi = ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                        else if(CurrentOffsets.StructVersion == 1)
                            pi = ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                        else if (CurrentOffsets.StructVersion < 3)
                            pi = ProcessMemory.getInstance().Read<Struct_2021_3_31_3s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                        else
                            pi = ProcessMemory.getInstance().Read<Struct_2021_6_15s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);

                        //string playerName = pi.GetPlayerName();
                        //if (playerName == null) playerName = "";
                        //if (playerName.Length == 0) continue;
                        int id = IdList[pi.PlayerId];
                        if (Math.Abs(playerIsDead[id]) < 10)
                        {
                            if (pi.GetIsDead()) playerIsDead[id] = 1;
                            else if (pi.GetIsDisconnected()) playerIsDead[id] = 10;
                            else playerIsDead[id] = 0;
                        }


                        PlayerControl pcontrol = CurrentOffsets.StructVersion >= 3 ? ProcessMemory.getInstance().Read<Struct_2021_6_15s.v_PlayerControl>(pi._object):(CurrentOffsets.StructVersion == 0 ? ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerControl>(pi._object) : ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerControl>(pi._object));

                        if ((!pi.GetIsDisconnected() && pi.IsImpostor != 1) || pcontrol.myLight_ != 0)
                        {
                            IntPtr tasklist = (IntPtr)ProcessMemory.getInstance().Read<Int32>((IntPtr)pcontrol.myTasks, 8);

                            TaskNum[id] = ProcessMemory.getInstance().Read<Int32>((IntPtr)pi.Tasks, 12);
                            if (TaskNum[id] > PlayerData.MaxTaskNum) TaskNum[id] = PlayerData.MaxTaskNum;
                            if (TaskNum[id] != 0)
                            {
                                TaskProgress[id] = 0;
                                var TaskPtrList = ProcessMemory.getInstance().ReadArray(tasklist + 0x10, TaskNum[id] + 3);
                                var IdxOffset = 0;
                                if (pcontrol.myLight_ != 0)
                                {
                                    if (pi.GetIsDead())
                                        IdxOffset += 1;
                                    if (pi.GetIsDead() && pi.IsImpostor == 1)
                                    {
                                        IntPtr p = TaskPtrList[IdxOffset];
                                        Sabotage = ProcessMemory.getInstance().Read<TaskInfo>(p);
                                    }
                                    else
                                    {
                                        var p = TaskPtrList[IdxOffset + TaskNum[id] + pi.IsImpostor];
                                        Sabotage = ProcessMemory.getInstance().Read<TaskInfo>(p);
                                        if (Sabotage.TaskType != TaskTypes.SubmitScan && TaskNum[id] != 0 && Sabotage.TaskType == taskInfos[id][TaskNum[id] - 1].TaskType)
                                        {
                                            IdxOffset += 1;
                                            p = TaskPtrList[IdxOffset + TaskNum[id] + pi.IsImpostor];
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
                                        var p = TaskPtrList[IdxOffset + j];
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



                        if (state != GameState.DISCUSSION && !pi.GetIsDisconnected() && (playerIsDead[id] == 0 || playerIsDead[id] < -10 || playerIsDead[id] == 11))
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
                                if (playing)
                                    TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                    {
                                        time = gametimeMili,
                                        Sender = PlayerNames[i],
                                        Message = $" is disconnected"
                                    });
                            }
                            if (playerIsDead[i] == 1)
                            {
                                var deadbody = new DeadBodyPos(i, PlayerPoses, ImpostorId, playerIsDead, AllImposterNum);
                                DeadBodyPosList.Insert(0, deadbody);
                                int killercount = deadbody.ImpostorDists.Count;
                                if (killercount == 0)
                                {
                                    playerIsDead[i] = 15;
                                    DeadBodyPosList.RemoveAt(0);
                                    if (playing)
                                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                        {
                                            time = gametimeMili,
                                            Sender = PlayerNames[i],
                                            Message = $" is killed by someone"
                                        });
                                }
                                else
                                {
                                    playerIsDead[i] = deadbody.ImpostorDists[0].Id + 20;
                                    string killer = PlayerNames[deadbody.ImpostorDists[0].Id] + "/" + PlayerColorsInt[deadbody.ImpostorDists[0].Id];
                                    if (killercount > 1)
                                    {
                                        killer += "(";
                                        for (int j = 1; j < killercount; j++)
                                        {
                                            killer += " or " + PlayerNames[DeadBodyPosList[0].ImpostorDists[j].Id] + "/" + PlayerColorsInt[deadbody.ImpostorDists[j].Id];
                                        }
                                        killer += ")";
                                    }
                                    if (playing)
                                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                        {
                                            time = gametimeMili,
                                            Sender = PlayerNames[i],
                                            Message = $" is killed by " + killer
                                        });
                                }

                            }
                        }
                    }


                    if (CurrentOffsets.ShipStatusPtr != null)
                    {
                        var shipStatusPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.ShipStatusPtr);
                        ShipStatus shipStatus = CurrentOffsets.StructVersion >=3 ? ProcessMemory.getInstance().Read<Struct_2021_6_15s.v_ShipStatus>(shipStatusPtr) : (CurrentOffsets.StructVersion == 0? ProcessMemory.getInstance().Read< Struct_2020_12_9s.v_ShipStatus>(shipStatusPtr) : ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_ShipStatus>(shipStatusPtr));
                        var doorsPtr = shipStatus.AllDoors;
                        int doorNum = ProcessMemory.getInstance().Read<Int32>(doorsPtr, 0xC);
                        if (doorNum > 100) doorNum = 100;
                        var doorsListPtr = doorsPtr + 0x10;
                        if (doors == null || doors.Length != doorNum) doors = new Door[doorNum];

                        bool[] doorOpen = new bool[doorNum];
                        for (int i = 0; i < doorNum; i++)
                        {
                            doors[i] = ProcessMemory.getInstance().Read<Door>(doorsListPtr, 4 * i, 0);
                            doorOpen[doors[i].Id] = doors[i].Open;
                        }
                        if (playMap == PlayerData.PlayMap.Skeld)
                            doorsUint = Doors.skeld.Doors2Uint(doors);
                        else if (playMap == PlayerData.PlayMap.Polus)
                            doorsUint = Doors.polus.Doors2Uint(doors);
                        else if (playMap == PlayerData.PlayMap.AirShip)
                        {
                            var platform = ProcessMemory.getInstance().Read<Doors.airship.MovingPlatformBehaviour>(shipStatus.GapPlatform);
                            IsLeft = platform.IsLeft;
                            doorsUint = Doors.airship.Doors2Uint(doors, IsLeft);
                        }
                    }

                    if (state != cachedOldState)
                    {
                        string statestr = $"GameState:{state}";
                        if (state == GameState.DISCUSSION)
                        {
                            TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                            {
                                time = -1,
                                Sender = null,
                                Message = ""
                            });
                        }
                        else if (state == GameState.TASKS) statestr += "\n";
                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                        {
                            time = gametimeMili,
                            Sender = null,
                            Message = statestr
                        });
                    }
                    //get chat
                    if (CurrentOffsets.ChatControllerPtr != null)
                    {
                        if (loadLastchatNextturn)
                        {
                            var msgPtrList = ProcessMemory.getInstance().ReadArray(lastChatBubblePtr + CurrentOffsets.ChatText, 2);
                            var msgPtr = ProcessMemory.getInstance().Read<IntPtr>(msgPtrList[1], CurrentOffsets.TextMeshPtr);
                            var senderPtr = ProcessMemory.getInstance().Read<IntPtr>(msgPtrList[0], CurrentOffsets.TextMeshPtr);
                            if (senderPtr != IntPtr.Zero || msgPtr != IntPtr.Zero)
                            {
                                var msgText = ProcessMemory.getInstance().ReadString(msgPtr);
                                var msgSender = ProcessMemory.getInstance().ReadString(senderPtr);
                                ChatMessageAdded?.Invoke(this, new ChatMessageEventArgs
                                {
                                    time = gametimeMili,
                                    Sender = msgSender,
                                    Message = msgText
                                });
                                if (msgText != "")
                                    Console.WriteLine(msgSender + ":\t" + msgText);
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
                            var chatBubblesInfos = ProcessMemory.getInstance().ReadArray(chatBubblesPtr + 0x8, 3);
                            var chatBubsVersion = (int)chatBubblesInfos[2];
                            if (chatBubsVersion != prevChatBubsVersion)
                            {
                                var poolSize = 20;//ProcessMemory.getInstance().Read<Int32>(chatPoolPtr, 0xc);
                                var numChatBubbles = (int)chatBubblesInfos[1];
                                var chatBubblesAddr = chatBubblesInfos[0] + 0x10;
                                var chatBubblePtrs = ProcessMemory.getInstance().ReadArray(chatBubblesAddr, numChatBubbles);
                                
                                var newMsgs = 0;
                                if (chatBubblesInfos[0] != IntPtr.Zero)
                                {
                                    
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
                                        if (chatBubsVersion != 0 && firstdisc)
                                            firstdisc = false;
                                        else
                                        {
                                            while (numChatBubbles - newMsgs > 0 && chatBubblePtrs[numChatBubbles - newMsgs - 1] != lastChatBubblePtr)
                                                newMsgs++;
                                        }
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
                                        var msgPtrList = ProcessMemory.getInstance().ReadArray(chatBubblePtrs[i] + CurrentOffsets.ChatText, 2);
                                        var msgText = ProcessMemory.getInstance()
                                            .ReadString(ProcessMemory.getInstance().Read<IntPtr>(msgPtrList[1], CurrentOffsets.TextMeshPtr));
                                        var msgSender = ProcessMemory.getInstance()
                                            .ReadString(ProcessMemory.getInstance().Read<IntPtr>(msgPtrList[0], CurrentOffsets.TextMeshPtr));
                                        ChatMessageAdded?.Invoke(this, new ChatMessageEventArgs
                                        {
                                            time = gametimeMili,
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

                    if (shouldReadLobby)
                    {
                        var gameCode = ProcessMemory.getInstance().ReadString(ProcessMemory.getInstance().Read<IntPtr>(
                            GameAssemblyPtr,
                            CurrentOffsets.GameCodeOffsets), CurrentOffsets.StringOffsets[0], CurrentOffsets.StringOffsets[1]);
                        string[] split;
                        if (!string.IsNullOrEmpty(gameCode) && (split = gameCode.Split('\n')).Length == 2)
                        {
                            LobbyCode = split[1];
                            Console.WriteLine("Code:" + LobbyCode);
                            shouldReadLobby = false;
                        }
                    }

                    //game start check and init
                    //if (oldAllImposterNum != AllImposterNum)
                    if (testflag)
                    {
                        AllImposterNum = 1;
                    }
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
                            firstdisc = true;
                        }
                    }

                    //send move data 

                    if (state == GameState.ENDED)
                    {
                        Thread.Sleep(1000);
                        int rawGameOverReason = ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.RawGameOverReasonOffsets);
                        gameOverReason = (GameOverReason)rawGameOverReason;
                        state = (GameState)(6 + rawGameOverReason);
                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                        {
                            time = gametimeMili,
                            Sender = null,
                            Message = $"GameState:{state}"
                        });
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
                    var Millinow = (Int32)((DateTime.Now.Ticks - gameStartTime) / TimeSpan.TicksPerMillisecond);
                    int frame_now = (int)Math.Round(Millinow / 100.0);

                    if (frame_now - frame < 5)
                    {
                        for (; frame < frame_now; frame++) PlayerMove?.Invoke(this, move);
                    }
                    else PlayerMove?.Invoke(this, move);
                    frame = frame_now;
                    Thread.Sleep((frame_now + 1) * 100 - Millinow);



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

            while (playerCount > PlayerData.MaxPlayerNum && repeatcount < 50)
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
                playerCount = PlayerData.MaxPlayerNum;
            }

            var allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
            var playerAddrPtr = allPlayers + 0x10;
            var PlayerInfoPtrList = ProcessMemory.getInstance().ReadArray(playerAddrPtr, playerCount);

            uint[] HatIds = new uint[PlayerData.MaxPlayerNum];
            uint[] PetIds = new uint[PlayerData.MaxPlayerNum];
            uint[] SkinIds = new uint[PlayerData.MaxPlayerNum];

            while (!dataCompleted)
            {
                gameStartTime = DateTime.Now.Ticks;
                filename = DateTime.Now.ToString("yyyyMMdd_HHmm_ss");
                ImposterNum = 0;
                for (var i = 0; i < playerCount; i++)
                {
                    PlayerInfo pi;
                    if (CurrentOffsets.StructVersion == 0)
                        pi = ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                    else if (CurrentOffsets.StructVersion == 1)
                        pi = ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                    else if (CurrentOffsets.StructVersion <3)
                        pi = ProcessMemory.getInstance().Read<Struct_2021_3_31_3s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);
                    else
                        pi = ProcessMemory.getInstance().Read<Struct_2021_6_15s.v_PlayerInfo>(PlayerInfoPtrList[i], 0);


                    playerAddrPtr += 4;
                    IdList[pi.PlayerId] = i;
                    PlayerNames[i] = pi.GetPlayerName();
                    if (PlayerNames[i] == null) PlayerNames[i] = "";
                    PlayerName2IDdict[PlayerNames[i]] = i;
                    IsImpostorLis[i] = pi.IsImpostor == 1;
                    var col = (int)pi.GetPlayerColor();
                    if (col >= 0 && col < PlayerData.PlayerColorNum)
                    {
                        PlayerColors[i] = PlayerData.ColorList[col];
                        PlayerColorsInt[i] = (PlayerData.PlayerColor)col;
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

                    PlayerControl pcontrol = CurrentOffsets.StructVersion >=3 ? ProcessMemory.getInstance().Read<Struct_2021_6_15s.v_PlayerControl>(pi._object) : (CurrentOffsets.StructVersion == 0 ? ProcessMemory.getInstance().Read<Struct_2020_12_9s.v_PlayerControl>(pi._object) : ProcessMemory.getInstance().Read<Struct_2021_3_5s.v_PlayerControl>(pi._object));

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
                    PlayerInfoPtrList = ProcessMemory.getInstance().ReadArray(playerAddrPtr, playerCount);
                }
            }

            if (repeatcount > 50) Console.WriteLine("Wrong data : Empty player color | Continue");

            var s = "";
            for (int i = 0; i < playerCount; i++)
            {
                s += $"{PlayerNames[i]}/{PlayerColorsInt[i]}, ";
            }
            Console.WriteLine(s);
            playMap = (PlayerData.PlayMap)ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.PlayMapOffsets);

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
                LobbyCode = LobbyCode,
                PlayMap = playMap,
                PlayerMove = move,
                PlayerColorsInt = PlayerColorsInt,
                HatIds = HatIds,
                PetIds = PetIds,
                SkinIds =SkinIds
        }) ;


        }



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



    public class PlayerCosmeticChangedEventArgs : EventArgs
    {
        public string Name { get; set; }
        public uint HatId { get; set; } 
        public uint SkinId { get; set; }
        public uint PetId { get; set; }
    }

    public class PlayerChangedEventArgs : EventArgs
    {
        public PlayerData.PlayerAction Action { get; set; }
        public string Name { get; set; }
        public bool IsDead { get; set; }
        public bool Disconnected { get; set; }
        public PlayerData.PlayerColor Color { get; set; }
    }

    public class ChatMessageEventArgs : EventArgs
    {
        public int time { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
    }

    public class LobbyEventArgs : EventArgs
    {
        public string LobbyCode { get; set; }
        public PlayerData.PlayRegion Region { get; set; }
        public PlayerData.PlayMap Map { get; set; }
    }

    public class GameOverEventArgs : EventArgs
    {
        public GameOverReason GameOverReason { get; set; }
        public ImmutablePlayer[] PlayerInfos { get; set; }
    }

    public class GameStartEventArgs : EventArgs
    {
        public string filename;
        public string LobbyCode;
        public PlayerData.PlayMap PlayMap;
        public PlayerMoveArgs PlayerMove;
        public PlayerData.PlayerColor[] PlayerColorsInt;
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
