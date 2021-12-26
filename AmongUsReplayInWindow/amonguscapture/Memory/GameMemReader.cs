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
        public const int MaxTaskNum = 30;

        public const int PlayerColorDefaultNum = 18;
        public const int PlayerColorNum = 35;
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
            Color.FromArgb(239, 191, 192),//Salmon = 18,
            Color.FromArgb(109, 7, 26),//Bordeaux = 19,
            Color.FromArgb(154, 140, 61),//Olive = 20,
            Color.FromArgb(22, 132, 176),//Turqoise = 21,
            Color.FromArgb(111, 192, 156),//Mint = 22,
            Color.FromArgb(173, 126, 201),//Lavender = 23,
            Color.FromArgb(160, 101, 56),//Nougat = 24,
            Color.FromArgb(255, 164, 119),//Peach = 25,
            Color.FromArgb(112, 143, 46),//Wasabi = 26,
            Color.FromArgb(255, 51, 102),//Hot_Pink = 27,
            Color.FromArgb(0, 99, 105),//Petrol = 28,
            Color.FromArgb(0xDB, 0xFD, 0x2F),//Lemon = 29,
            Color.FromArgb(0xF7, 0x44, 0x17),//Signal_Orange = 30,
            Color.FromArgb(0x25, 0xB8, 0xBF),//Teal = 31,
            Color.FromArgb(0x59, 0x3C, 0xD6),//Blurple = 32,
            Color.FromArgb(0xFF, 0xCA, 0x19),//Sunrise = 33,
            Color.FromArgb(0xA8, 0xDF, 0xFF),//Ice = 34
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
                { ColorList[18].ToArgb(),"Salmon"},
                { ColorList[19].ToArgb(),"Bordeaux" },
                { ColorList[20].ToArgb(),"Olive"},
                { ColorList[21].ToArgb(),"Turqoise"},
                { ColorList[22].ToArgb(),"Mint"},
                { ColorList[23].ToArgb(),"Lavender"},
                { ColorList[24].ToArgb(),"Nougat"},
                { ColorList[25].ToArgb(),"Peach"},
                { ColorList[26].ToArgb(),"Wasabi"},
                { ColorList[27].ToArgb(),"Hot_Pink"},
                { ColorList[28].ToArgb(),"Petrol"},
                { ColorList[29].ToArgb(),"Lemon"},
                { ColorList[30].ToArgb(),"Signal_Orange"},
                { ColorList[31].ToArgb(),"Teal"},
                { ColorList[32].ToArgb(),"Blurple"},
                { ColorList[33].ToArgb(),"Sunrise"},
                { ColorList[34].ToArgb(),"Ice"},
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
            Empty = -1,
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
            Coral = 17,
            Salmon = 18,
            Bordeaux = 19,
            Olive = 20,
            Turqoise = 21,
            Mint = 22,
            Lavender = 23,
            Nougat = 24,
            Peach = 25,
            Wasabi = 26,
            Hot_Pink = 27,
            Petrol = 28,
            Lemon = 29,
            Signal_Orange = 30,
            Teal = 31,
            Blurple = 32,
            Sunrise = 33,
            Ice = 34
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

        public enum VoteState : sbyte
        {
            HasNotVoted = -1,
            MissedVote = -2,
            SkippedVote = -3,
            DeadVote = -4,
        }
        //public static int notVote = -1;
        //public static int notVoteEnd = -2;

        public enum DeadState
        {
            Disconnect = 10,
            Disconnected = -10,
            ejected = -11,
            eject = 11,
            killer_offset = 20,
            killedBySomeone = 15,
            living = 0,
            killed_now = 1,
        }
    }

    public class GameMemReader
    {
        #region value
        private static readonly GameMemReader instance = new GameMemReader();
        private bool exileCausesEnd;

        private bool shouldReadLobby = false;
        private string LobbyCode = "";
        private IntPtr GameAssemblyPtr = IntPtr.Zero;

        private LobbyEventArgs latestLobbyEventArgs = null;
        
        private GameState oldState = GameState.UNKNOWN;

        private int prevChatBubsVersion;

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

        RoleTypes[] RoleType = new RoleTypes[PlayerData.MaxPlayerNum];
        bool[] IsGuardian = new bool[PlayerData.MaxPlayerNum];
        bool[] protectedByGuardian = new bool[PlayerData.MaxPlayerNum];
        sbyte[] shapeId = new sbyte[PlayerData.MaxPlayerNum];
        Dictionary<PlayerData.PlayerColor,sbyte> Color2Id = new Dictionary<PlayerData.PlayerColor, sbyte>();
        sbyte[] RemainingEmergencies = new sbyte[PlayerData.MaxPlayerNum];
        float EmergencyCooldown;
        int Reporter;
        int ReportTarget;
        bool discussion_end;
        long discussionStartTime = 100000;
        #endregion

        public GameMemReader()
        {
            DeadLog.memReader = this;
            initInfo();
        }


        private void initInfo()
        {
            for (int i = 0; i < PlayerData.MaxPlayerNum; i++)
            {
                playerIsDead[i] = (int)PlayerData.DeadState.living;
                PlayerColors[i] = Color.Empty;
                PlayerColorsInt[i] = PlayerData.PlayerColor.Empty;
                IsImpostorLis[i] = false;
                taskInfos[i] = new TaskInfo[PlayerData.MaxTaskNum];
                for (int j = 0; j < PlayerData.MaxTaskNum; j++) taskInfos[i][j] = new TaskInfo();
                TaskProgress[i] = 0;
                Taskcompleted[i] = false;
                TaskNum[i] = 0;
                PlayerNames[i] = "";
                voteList[i] = -3;
                discussionEndTime = -10000;
                IdList[i] = i;
                RoleType[i] = RoleTypes.Crewmate;
                IsGuardian[i] = false;
                protectedByGuardian[i] = false;
                shapeId[i] = -1;
                RemainingEmergencies[i] = 0;
            }
            Sabotage = new TaskInfo();
            DeadBodyPosList.Clear();
            DeadLogList.Clear();
            PlayerName2IDdict.Clear();
            gameOverReason = GameOverReason.Unknown;
            doorsUint = 0;
            lastChatBubblePtr = IntPtr.Zero;
            Color2Id.Clear();
            EmergencyCooldown = 0;
            ReportTarget = -1;
            Reporter = -1;
            discussion_end = true;
            discussionStartTime = 100000;
        }


        public void RunLoop()
        {
            var tokenSource = new CancellationTokenSource();
            var cancelToken = tokenSource.Token;
            RunLoop(cancelToken);
            tokenSource.Dispose();
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
                    var playerAddrPtr = allPlayers + CurrentOffsets.PlayerListPtr;

                    Int32 gametimeMili = (Int32)((DateTime.Now.Ticks - gameStartTime) / TimeSpan.TicksPerMillisecond);


                    //get voting state and report
                    if (state == GameState.DISCUSSION || (state == GameState.TASKS && !discussion_end ))
                    {
                        if (state == GameState.DISCUSSION)
                        {
                            discussionEndTime = gametimeMili;
                        }
                        else state = GameState.VotingResult;
                        if (meetingHud != IntPtr.Zero)
                        {
                            var voteInfoPtr = ProcessMemory.getInstance().Read<IntPtr>(meetingHud, CurrentOffsets.VoteAreaOffsets.PlayerVoteAreaListPtr);
                            if (voteInfoPtr != IntPtr.Zero)
                            {
                                var votePlayerCount = ProcessMemory.getInstance().Read<int>((IntPtr)voteInfoPtr, CurrentOffsets.PlayerCountOffsets);
                                if (votePlayerCount > PlayerData.MaxPlayerNum * 2) votePlayerCount = PlayerData.MaxPlayerNum * 2;
                                voteInfoPtr += CurrentOffsets.PlayerListPtr;
                                {
                                    var voteAreaPtrList = ProcessMemory.getInstance().ReadArray(voteInfoPtr, votePlayerCount);
                                    PlayerVoteArea voteArea = new PlayerVoteArea();
                                    for (int i = 0; i < votePlayerCount; i++)
                                    {
                                        voteArea.set(voteAreaPtrList[i], ProcessMemory.getInstance(), CurrentOffsets); 

                                         int id = IdList[voteArea.Id_];

                                            sbyte votedId = voteArea.votedFor;
                                            if (votedId >= 0 && votedId < PlayerData.MaxPlayerNum) votedId = (sbyte)IdList[votedId];
                                            voteList[id] = votedId;

                                        if (voteArea.didReport)
                                        {
                                            Reporter = id;
                                            voteList[id] += 32;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < PlayerData.MaxPlayerNum; i++) voteList[i] = (sbyte)PlayerData.VoteState.HasNotVoted;
                        }

                        //get report
                        if (discussion_end && gametimeMili - discussionStartTime > 500 && state == GameState.DISCUSSION && oldState == GameState.DISCUSSION)
                        {
                            discussion_end = false;
                            if (CurrentOffsets.MeetingRoomManagerOffsets != null)
                            {
                                var MeetingRoomManagerPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.MeetingRoomManagerOffsets.MeetingRoomManager_Offsets);
                                String Sender = null;
                                if (Reporter >= 0 && Reporter < PlayerData.MaxPlayerNum) Sender = PlayerNames[Reporter];

                                //get target
                                var targetInfoPtr = ProcessMemory.getInstance().Read<IntPtr>(MeetingRoomManagerPtr, CurrentOffsets.MeetingRoomManagerOffsets.target);
                                ReportTarget = -1;
                                PlayerInfo targetInfo = null;
                                if (targetInfoPtr != IntPtr.Zero)
                                {
                                    targetInfo = new PlayerInfo(MeetingRoomManagerPtr + CurrentOffsets.MeetingRoomManagerOffsets.target, ProcessMemory.getInstance(), CurrentOffsets);
                                    if (targetInfo.PlayerId >= 0 && targetInfo.PlayerId < PlayerData.MaxPlayerNum) ReportTarget = IdList[targetInfo.PlayerId];
                                }
                                    if (ReportTarget == -1)
                                    {
                                        if (Reporter != myId && Reporter >= 0 && Reporter < PlayerData.MaxPlayerNum)
                                            RemainingEmergencies[Reporter] -= 1;
                                     /*
                                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                        {
                                            time = -1,
                                            Sender = Sender,
                                            Message = "called an emergency meeting"
                                        });
                                     */
                                    }else{
                                    /*
                                        TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                        {
                                            time = -1,
                                            Sender = Sender,
                                            Message = "reported " + targetInfo?.PlayerName + "/" + targetInfo?.ColorId.ToString() + "'s body"
                                        });
                                    */
                                    }
                            }
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
                            if (playerIsDead[i] == (int)PlayerData.DeadState.living)
                            {
                                if (votedId == (int)PlayerData.VoteState.HasNotVoted || votedId == (int)PlayerData.VoteState.MissedVote)
                                    votingresult = $"->Not Vote";

                                else if (votedId >= PlayerData.MaxPlayerNum)
                                    votingresult = $"->Error ID:{votedId}";
                                else if (votedId >= 0)
                                    votingresult = $"->{PlayerNames[votedId]}/{PlayerColorsInt[votedId]}";
                                else if (votedId == (int)PlayerData.VoteState.SkippedVote)
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
                            PlayerInfo pi = new PlayerInfo(playerAddrPtr+ CurrentOffsets.AddPlayerPtr*i, ProcessMemory.getInstance(), CurrentOffsets);


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
                                if (playerIsDead[id] != (int)PlayerData.DeadState.eject && playerIsDead[id] != (int)PlayerData.DeadState.ejected)
                                {
                                    disconnect = false;
                                    playerIsDead[id] = (int)PlayerData.DeadState.eject;
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
                                else playerIsDead[id] = (int)PlayerData.DeadState.ejected;

                            }
                            else if (playerIsDead[id] > (int)PlayerData.DeadState.living) playerIsDead[id] = -playerIsDead[id];

                            // skip invalid, dead and exiled players
                            if (pi.PlayerName == null || pi.PlayerId == exiledPlayerId || pi.IsDead ||
                                pi.Disconnected ) continue;

                            if (pi.IsImpostor)
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
                    
                    if (state != oldState && state == GameState.LOBBY)
                    {
                        shouldReadLobby = true; // will eventually transmit
                    }
                    

                    if (oldState == GameState.ENDED && (state == GameState.LOBBY || state == GameState.MENU)) // game ended
                    {
                        int rawGameOverReason = ProcessMemory.getInstance()
                            .Read<int>(GameAssemblyPtr, CurrentOffsets.RawGameOverReasonOffsets);
                        gameOverReason = (GameOverReason)rawGameOverReason;
                        
                        Console.WriteLine($"{gameOverReason}");
                    }

                    GameState cachedOldState = oldState;

                    oldState = state;


                    //newPlayerInfos.Clear();
                    #endregion
                    playerAddrPtr = allPlayers + CurrentOffsets.PlayerListPtr;
                    AllImposterNum = 0;


                    //get current state
                    for (var i = 0; i < playerCount; i++)
                    {
                        PlayerInfo pi = new PlayerInfo(playerAddrPtr + CurrentOffsets.AddPlayerPtr * i, ProcessMemory.getInstance(), CurrentOffsets);

                        int id = IdList[pi.PlayerId];
                        if (Math.Abs(playerIsDead[id]) < (int)PlayerData.DeadState.Disconnect)
                        {
                            if (pi.GetIsDead()) playerIsDead[id] = (int)PlayerData.DeadState.killed_now;
                            else if (pi.GetIsDisconnected()) playerIsDead[id] = (int)PlayerData.DeadState.Disconnect;
                            else playerIsDead[id] = (int)PlayerData.DeadState.living;
                        }
                        if (pi.shaping) shapeId[id] = Color2Id.GetValueOrDefault<PlayerData.PlayerColor, sbyte>(pi.shapeColorId, -1);
                        else shapeId[id] = -1;
                        if (pi.ColorId != PlayerColorsInt[id]) shapeId[id] = Color2Id.GetValueOrDefault<PlayerData.PlayerColor,sbyte>(pi.ColorId, -1);
                        if (pi.RoleType == RoleTypes.GuardianAngel)
                        {
                            if (pi.Disconnected) IsGuardian[id] = false ;
                            else
                            {
                                if (!IsGuardian[id])
                                {
                                    IsGuardian[id] = true;
                                    TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                    {
                                        time = gametimeMili,
                                        Sender = PlayerNames[id],
                                        Message = $" became a Guardian Angel"
                                    });
                                }
                            }
                        }
                        else IsGuardian[id] = false;

                        PlayerControl pcontrol = new PlayerControl(pi._object, ProcessMemory.getInstance(), CurrentOffsets);
                        protectedByGuardian[id] = pcontrol.protectedByGuardian;
                        if (id == myId)
                        {
                            RemainingEmergencies[id] = (sbyte)pcontrol.RemainingEmergencies;
                        }
                        InVent[id] = pcontrol.inVent;
                        if ((!pi.GetIsDisconnected() && !pi.IsImpostor) || pcontrol.myLight_ != 0)
                        {
                            IntPtr tasklist = (IntPtr)ProcessMemory.getInstance().Read<Int32>((IntPtr)pcontrol.myTasks, CurrentOffsets.AllPlayersOffsets);

                            TaskNum[id] = ProcessMemory.getInstance().Read<Int32>((IntPtr)pi.Tasks, CurrentOffsets.PlayerCountOffsets);
                            if (TaskNum[id] > PlayerData.MaxTaskNum) TaskNum[id] = PlayerData.MaxTaskNum;
                            if (TaskNum[id] != 0)
                            {
                                TaskProgress[id] = 0;
                                var TaskPtrList = ProcessMemory.getInstance().ReadArray(tasklist + CurrentOffsets.PlayerListPtr, TaskNum[id] + 3);
                                var IdxOffset = 0;
                                if (pcontrol.myLight_ != 0)
                                {
                                    if (pi.IsDead) IdxOffset++;
                                    int sabotage_task = ProcessMemory.getInstance().Read<int>((IntPtr)pcontrol.myTasks, CurrentOffsets.PlayerCountOffsets);
                                    if (sabotage_task > 0 && sabotage_task <= TaskNum[id] + 3) Sabotage.set(TaskPtrList[sabotage_task - 1], ProcessMemory.getInstance(), CurrentOffsets);
                                    else Sabotage.TaskType = TaskTypes.SubmitScan;
                                    if (!Sabotage.IsSabotage()) Sabotage.TaskType = TaskTypes.SubmitScan;
                                }
                                if (pi.IsImpostor)
                                {
                                    TaskProgress[id] = 1;
                                }
                                else
                                {
                                    for (int j = 0; j < TaskNum[id]; j++)
                                    {
                                        var p = TaskPtrList[IdxOffset + j];
                                        var oldinfo = taskInfos[id][j].TaskType;
                                        taskInfos[id][j].set(p, ProcessMemory.getInstance(), CurrentOffsets);


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



                        if (state != GameState.DISCUSSION && !pi.GetIsDisconnected())
                        {
                            int offset = CurrentOffsets.NetTransformOffsets.targetSyncPosition;
                            if (pcontrol.myLight_ != 0) offset = CurrentOffsets.NetTransformOffsets.prevPosSent;
                            PlayerPoses[id] = ProcessMemory.getInstance().Read<Vector2>((IntPtr)(pcontrol.NetTransform + offset));
                            newPlayerCon[id] = pcontrol;
                            //IsImpostorLis[id] = pi.IsImpostor == 1;
                        }
                        if (pi.IsImpostor) AllImposterNum++;
                        #region amonguscapture
                       
                    }
                    #endregion

                    //impostor set
                    if (newPlayerCon[0] != null)
                    {
                        for (int i = 0; i < AllImposterNum; i++)
                        {
                            var id = ImpostorId[i];
                        }


                        //dead set

                        for (var i = 0; i < playerCount; i++)
                        {
                            if (Math.Abs(playerIsDead[i]) == (int)PlayerData.DeadState.killed_now)
                            {
                                DeadLogList.Add(new DeadLog(gameStartTime, i, PlayerPoses[i]));
                            }
                            else if (playerIsDead[i] == (int)PlayerData.DeadState.Disconnect)
                            {
                                DeadLogList.Add(new DeadLog(gameStartTime, i, PlayerPoses[i]));
                                playerIsDead[i] = (int)PlayerData.DeadState.Disconnected;
                                if (playing)
                                    TextLogEvent?.Invoke(this, new ChatMessageEventArgs
                                    {
                                        time = gametimeMili,
                                        Sender = PlayerNames[i],
                                        Message = $" is disconnected"
                                    });
                            }
                            if (playerIsDead[i] == (int)PlayerData.DeadState.killed_now)
                            {
                                var deadbody = new DeadBodyPos(i, PlayerPoses, ImpostorId, playerIsDead, AllImposterNum);
                                DeadBodyPosList.Insert(0, deadbody);
                                int killercount = deadbody.ImpostorDists.Count;
                                if (killercount == 0)
                                {
                                    playerIsDead[i] = (int)PlayerData.DeadState.killedBySomeone;
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
                                    playerIsDead[i] = deadbody.ImpostorDists[0].Id + (int)PlayerData.DeadState.killer_offset;
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

                    if (CurrentOffsets.ShipStatusOffsets == null)
                    {
                        if ((gametimeMili - discussionEndTime) < 7000) discussion_end = true;
                    }
                    else
                    {
                        var shipStatusPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.ShipStatusOffsets.ShipStatusPtr);
                        ShipStatus shipStatus = new ShipStatus(GameAssemblyPtr, ProcessMemory.getInstance(), CurrentOffsets);
                        if (EmergencyCooldown < shipStatus.EmergencyCooldown) discussion_end = true;
                        EmergencyCooldown = shipStatus.EmergencyCooldown;
                        var doorsPtr = shipStatus.AllDoors;
                        int doorNum = ProcessMemory.getInstance().Read<Int32>(doorsPtr, CurrentOffsets.PlayerCountOffsets);
                        if (doorNum > 100) doorNum = 100;
                        var doorsListPtr = doorsPtr + CurrentOffsets.PlayerListPtr;
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
                            ReportTarget = -1;
                            discussionStartTime = gametimeMili;
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
                    if (CurrentOffsets.ChatOffsets != null)
                    {
                        ChatOffsets offsets = CurrentOffsets.ChatOffsets;
                        if (loadLastchatNextturn)
                        {
                            var msgPtrList = ProcessMemory.getInstance().ReadArray(lastChatBubblePtr + offsets.ChatText, 2);
                            var msgPtr = ProcessMemory.getInstance().Read<IntPtr>(msgPtrList[1], offsets.TextMeshPtr);
                            var senderPtr = ProcessMemory.getInstance().Read<IntPtr>(msgPtrList[0], offsets.TextMeshPtr);
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
                        
                        var HudManagerPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.HudManagerOffset);
                        var chatControllerPtr = ProcessMemory.getInstance().Read<IntPtr>(HudManagerPtr, offsets.chatControllerPtr);
                        if (chatControllerPtr != IntPtr.Zero)
                        {
                            var chatPoolPtr = ProcessMemory.getInstance().Read<IntPtr>(chatControllerPtr, offsets.chatPoolPtr);
                            var chatBubblesPtr = ProcessMemory.getInstance().Read<IntPtr>(chatPoolPtr, offsets.chatBubblesPtr);
                            var chatBubsVersion =  ProcessMemory.getInstance().Read<int>(chatBubblesPtr, offsets.chatBubsVersionPtr);
                            if (chatBubsVersion != prevChatBubsVersion)
                            {
                                var poolSize = 20;//ProcessMemory.getInstance().Read<Int32>(chatPoolPtr, 0xc);
                                var numChatBubbles = ProcessMemory.getInstance().Read<int>(chatBubblesPtr, offsets.numChatBubblesPtr);
                                var chatBubblesAddr = ProcessMemory.getInstance().Read<IntPtr>(chatBubblesPtr, offsets.chatBubblesAddrPtr) + CurrentOffsets.PlayerListPtr;
                                var chatBubblePtrs = ProcessMemory.getInstance().ReadArray(chatBubblesAddr, numChatBubbles);
                                
                                var newMsgs = 0;
                                if (chatBubblesAddr != (IntPtr)CurrentOffsets.PlayerListPtr)
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
                                        var msgPtrList = ProcessMemory.getInstance().ReadArray(chatBubblePtrs[i] + offsets.ChatText, 2);
                                        var msgText = ProcessMemory.getInstance()
                                            .ReadString(ProcessMemory.getInstance().Read<IntPtr>(msgPtrList[1], offsets.TextMeshPtr));
                                        var msgSender = ProcessMemory.getInstance()
                                            .ReadString(ProcessMemory.getInstance().Read<IntPtr>(msgPtrList[0], offsets.TextMeshPtr));
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
                                initInfo();
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
                        voteList = voteList,
                        shapeId = shapeId,
                        IsGuardian = IsGuardian,
                        protectedByGuardian = protectedByGuardian,
                        RemainingEmergencies = RemainingEmergencies,
                        EmergencyCooldown = EmergencyCooldown,
                        ReportTarget = ReportTarget
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
            initInfo();
            Console.WriteLine("\n\nStart\n\n");
            frame = -1;
            var ImposterNum = 0;

            while (!PlayerControl.RoleAssigned(GameAssemblyPtr, CurrentOffsets) && repeatcount < 50)
            {
                Console.WriteLine($"Wrong data : Roles have not been assigned yet. | Retrying in 100ms");
                Thread.Sleep(100);   
                repeatcount++;
            }
            var allPlayersPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
            var playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);
            if (playerCount > PlayerData.MaxPlayerNum)
            {
                Console.WriteLine($"Wrong data : Player count is {playerCount}. | Continue");
                playerCount = PlayerData.MaxPlayerNum;
            }

            var allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
            var playerAddrPtr = allPlayers + CurrentOffsets.PlayerListPtr;

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
                    PlayerInfo pi = new PlayerInfo(playerAddrPtr + CurrentOffsets.AddPlayerPtr * i, ProcessMemory.getInstance(), CurrentOffsets);

                    IdList[pi.PlayerId] = i;
                    PlayerNames[i] = pi.GetPlayerName();
                    if (PlayerNames[i] == null) PlayerNames[i] = "";
                    PlayerName2IDdict[PlayerNames[i]] = i;
                    IsImpostorLis[i] = pi.IsImpostor;
                    var col = (int)pi.GetPlayerColor();
                    if (col >= 0 && col < PlayerData.PlayerColorNum)
                    {
                        PlayerColors[i] = PlayerData.ColorList[col];
                        PlayerColorsInt[i] = (PlayerData.PlayerColor)col;
                    }
                    else PlayerColors[i] = Color.Empty;
                    if (pi.IsImpostor)
                    {
                        filename += "_" + PlayerNames[i];
                        ImpostorId[ImposterNum] = i;
                        ImposterNum++;
                    }
                    HatIds[i] = pi.HatId;
                    PetIds[i] = pi.PetId;
                    SkinIds[i] = pi.SkinId;

                    RoleType[i] = pi.RoleType;
                    if (pi.RoleType == RoleTypes.GuardianAngel && !pi.Disconnected) IsGuardian[i] = true;
                    Color2Id[pi.ColorId] = (sbyte)i;

                    PlayerControl pcontrol = new PlayerControl(pi._object, ProcessMemory.getInstance(), CurrentOffsets);
                    
                    int offset = CurrentOffsets.NetTransformOffsets.targetSyncPosition;
                    if (pcontrol.myLight_ != 0) offset = CurrentOffsets.NetTransformOffsets.prevPosSent;
                    PlayerPoses[i] = ProcessMemory.getInstance().Read<Vector2>((IntPtr)(pcontrol.NetTransform + offset));
                    newPlayerCon[i] = pcontrol;
                    if (pcontrol.myLight_ != 0)
                    {
                        myId = i;
                        RemainingEmergencies[i] = (sbyte)pcontrol.RemainingEmergencies;
                        for (int j = 0; j < playerCount; j++) RemainingEmergencies[j] = RemainingEmergencies[i];
                    }

                    if (pcontrol.protectedByGuardian) protectedByGuardian[i] = true;
                }

                playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);
                dataCompleted = true;
                for (int i = 0; i < playerCount; i++)
                {
                    if (PlayerColors[i] == Color.Empty) dataCompleted = false;
                }

                playMap = (PlayerData.PlayMap)ProcessMemory.getInstance().Read<int>(GameAssemblyPtr, CurrentOffsets.PlayMapOffsets);
                if (CurrentOffsets.ShipStatusOffsets != null)
                {
                    var shipStatusPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.ShipStatusOffsets.ShipStatusPtr);
                    ShipStatus shipStatus = new ShipStatus(GameAssemblyPtr, ProcessMemory.getInstance(), CurrentOffsets);
                    EmergencyCooldown = shipStatus.EmergencyCooldown;
                    var doorsPtr = shipStatus.AllDoors;
                    int doorNum = ProcessMemory.getInstance().Read<Int32>(doorsPtr, CurrentOffsets.PlayerCountOffsets);
                    if (doorNum > 100) doorNum = 100;
                    var doorsListPtr = doorsPtr + CurrentOffsets.PlayerListPtr;
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

                if (!dataCompleted)
                {
                    repeatcount++;
                    if (repeatcount > 50) break;
                    Console.WriteLine("Wrong data : Empty player color | Retrying in 100ms");
                    Thread.Sleep(100);
                    allPlayersPtr = ProcessMemory.getInstance().Read<IntPtr>(GameAssemblyPtr, CurrentOffsets.AllPlayerPtrOffsets);
                    allPlayers = ProcessMemory.getInstance().Read<IntPtr>(allPlayersPtr, CurrentOffsets.AllPlayersOffsets);
                    playerCount = ProcessMemory.getInstance().Read<int>(allPlayersPtr, CurrentOffsets.PlayerCountOffsets);
                    playerAddrPtr = allPlayers + CurrentOffsets.PlayerListPtr;
                }
            }

            if (repeatcount > 50) Console.WriteLine("Wrong data : Empty player color | Continue");

            var s = "";
            for (int i = 0; i < playerCount; i++)
            {
                s += $"{PlayerNames[i]}/{PlayerColorsInt[i]}, ";
            }
            Console.WriteLine(s);
            
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
                    voteList = voteList,
                    shapeId = shapeId,
                    IsGuardian = IsGuardian,
                    protectedByGuardian = protectedByGuardian,
                    RemainingEmergencies = RemainingEmergencies,
                    EmergencyCooldown = EmergencyCooldown,
                    ReportTarget = ReportTarget
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
                SkinIds =SkinIds,
                RoleType = RoleType
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

        public RoleTypes[] RoleType;
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

        public bool[] IsGuardian;
        public bool[] protectedByGuardian;
        public sbyte[] shapeId;
        public sbyte[] RemainingEmergencies;
        public float EmergencyCooldown;
        public int ReportTarget;
        public bool displayVote = false;
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
                if (imDist.distance < 25.0f && IsDead[ImpostorId[i]] == (int)PlayerData.DeadState.living ) ImpostorDists.Add(imDist);
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
