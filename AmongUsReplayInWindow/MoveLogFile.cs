using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUsCapture;
using System.IO;
using System.Drawing;
using System.Numerics;

namespace AmongUsReplayInWindow
{
    static public class MoveLogFile
    {
        public static List<WriteMoveLogFile> writeMoves = new List<WriteMoveLogFile>();
        private static string tempStorageFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsReplayInWindow\\temp");
        public static bool ClearTemp()
        {   
            if (Directory.Exists(tempStorageFolder) && !(Program.exeFolder == null || Program.exeFolder == string.Empty || Program.exeFolder == ""))
            {
                var directoryInfo = new DirectoryInfo(tempStorageFolder);
                directoryInfo.Attributes |= System.IO.FileAttributes.Hidden;
                directoryInfo.Attributes |= System.IO.FileAttributes.System;
                string folderPass = Program.exeFolder + "\\replay";
                if (!Directory.Exists(folderPass))
                {
                    Console.WriteLine($"create folder {folderPass}");
                    Directory.CreateDirectory(folderPass);
                }
                string[] tempfiles = Directory.GetFiles(tempStorageFolder);
                foreach(string tempfile in tempfiles)
                {
                    string name = Path.GetFileNameWithoutExtension(tempfile);
                    string time = name.Substring(0, 16);
                    try
                    {
                        DateTime filetime = DateTime.ParseExact(time, "yyyyMMdd_HHmm_ss", null);
                        if (filetime.AddMinutes(15) < DateTime.Now)
                        {
                            string destname = folderPass + "\\" + Path.GetFileName(tempfile);
                            if(File.Exists(destname)){
                                int num = 0;
                                string pathAndname = folderPass + "\\" + name + "(";
                                string ext =")" + Path.GetExtension(tempfile);
                                while (File.Exists(destname)){
                                    num++;
                                    destname = pathAndname + num.ToString() + ext;
                                }
                            }
                            try
                            {
                                File.Move(tempfile, destname, false);
                            }catch(Exception e) { Console.WriteLine(e); }
                        }
                    }
                    catch(FormatException e){ Console.WriteLine(e); }
                    
                }
            }
            return true;
        }

        public class WriteMoveLogFile
        {
            static int version = 4;
            string folderPass;
            private Stream stream;
            private BinaryWriter writer;
            internal int AllImposorNum;
            internal string tempfilename;
            public string filename;
            internal object lockObject = new object();
            internal PlayerMoveArgs move = null;

            public WriteMoveLogFile(GameStartEventArgs startArgs)
            {
                lock (lockObject)
                {
                    writeMoves.Add(this);
                    if (startArgs == null || startArgs.PlayerMove == null) return;
                    folderPass = Program.exeFolder + "\\replay";
                    move = startArgs.PlayerMove;
                    AllImposorNum = 0;
                    for (int i = 0; i < startArgs.PlayerMove.PlayerNum; i++)
                    {
                        if (startArgs.PlayerMove.IsImpostor[i])
                        {
                            AllImposorNum++;
                        }
                    }
                    if (!Directory.Exists(tempStorageFolder))
                    {
                        Directory.CreateDirectory(tempStorageFolder);
                        var directoryInfo = new DirectoryInfo(tempStorageFolder);
                        directoryInfo.Attributes |= System.IO.FileAttributes.Hidden;
                        directoryInfo.Attributes |= System.IO.FileAttributes.System;
                    }
                    if (!Directory.Exists(folderPass))
                    {
                        Console.WriteLine($"create folder {folderPass}");
                        Directory.CreateDirectory(folderPass);
                    }
                    this.filename = folderPass + "\\" + startArgs.filename + ".dat";
                    tempfilename = tempStorageFolder + "\\" + DateTime.Now.ToString("yyyyMMdd_HHmm_ss") + ".dat";
                    try
                    {
                        stream = File.Create(tempfilename);
                        writer = new BinaryWriter(stream);
                    } catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        writer?.Close();
                        stream?.Close();
                        writer = null;
                        stream = null;
                        return;
                    }
                    writePlayerData2bFile(startArgs);
                }
            }

            ~WriteMoveLogFile()
            {
                UnexpectedClose();
            }


            public virtual void UnexpectedClose()
            {
                lock (lockObject)
                {
                    try
                    {
                        writer?.Flush();
                        writer?.Close();
                        stream?.Close();
                        writer = null;
                        stream = null;
                    }
                    catch (ObjectDisposedException e)
                    {
                        writer = null;
                        stream = null;
                    }
                    writeMoves.Remove(this);
                }
            }

            public virtual void Close()
            {
                UnexpectedClose();
                lock (lockObject)
                {
                    try
                    {
                        if (File.Exists(tempfilename))
                            File.Move(tempfilename, filename);
                    } catch (Exception e)
                    {
                    }
                }
            }

            public bool writePlayerData2bFile(GameStartEventArgs startArgs)
            {
                lock (lockObject)
                {
                    move = startArgs.PlayerMove;
                    if (writer != null)
                    {
                        writer.Write((Int32)version);
                        writer.Write((Int32)startArgs.PlayMap);
                        writer.Write((UInt32)startArgs.electricalDoors);
                        writer.Write((Int32)startArgs.PlayerMove.PlayerNum);
                        writer.Write((Int32)startArgs.PlayerMove.myId);
                        writer.Write((Int32)AllImposorNum);
                        for (int i = 0; i < AllImposorNum; i++) writer.Write((Int32)startArgs.PlayerMove.ImpostorId[i]);

                        for (int i = 0; i < startArgs.PlayerMove.PlayerNum; i++)
                        {
                            writer.Write(startArgs.PlayerMove.PlayerNames[i]);
                            writer.Write((byte)startArgs.RoleType[i]);
                            writer.Write((sbyte)move.RemainingEmergencies[i]);
                            writer.Write((Int32)startArgs.PlayerColorsInt[i]);
                            writer.Write(startArgs.PlayerMove.IsImpostor[i]);
                            writer.Write((Int32)startArgs.HatIds[i]);
                            writer.Write((Int32)startArgs.PetIds[i]);
                            writer.Write((Int32)startArgs.SkinIds[i]);
                        }
                        return true;
                    }
                    return false;
                }
            }

            public bool writeMove2bFile(PlayerMoveArgs e)
            {
                lock (lockObject)
                {
                    move = e;
                    if (writer != null)
                    {
                        writer.Write((Int32)e.time);
                        writer.Write((byte)e.state);
                        writer.Write((byte)e.Sabotage.TaskType);
                        if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote) writer.Write((sbyte)e.ReportTarget);
                        else writer.Write((sbyte)Math.Max(sbyte.MinValue, Math.Ceiling(e.EmergencyCooldown)));
                        writer.Write(e.CameraOn);
                        writer.Write((UInt32)e.doorsUint);
                        //for (int i = 0; i < AllImposorNum; i++) writer.Write((bool)e.InVent[i]);
                        uint inVent = 0;
                        uint isGuardian = 0;
                        uint protectedByGuardian = 0;
                        for (int i = 0; i < e.PlayerNum; i++)
                        {
                            if(e.InVent[i]) inVent |= ((uint)1 << i);
                            if(e.IsGuardian[i]) isGuardian |= ((uint)1 << i);
                            if(e.protectedByGuardian[i])protectedByGuardian |= ((uint)1 << i);
                        }
                        writer.Write(inVent);
                        writer.Write(isGuardian);
                        writer.Write(protectedByGuardian);
                        for (int i = 0; i < e.PlayerNum; i++)
                        {
                            writer.Write(e.PlayerPoses[i].X);
                            writer.Write(e.PlayerPoses[i].Y);
                            writer.Write((sbyte)(e.PlayerIsDead[i]));
                            if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote)
                            { 
                                writer.Write((sbyte)e.voteList[i]);
                                writer.Write(e.RemainingEmergencies[i]);

                            }
                            else
                            { 
                                writer.Write((byte)(e.TaskProgress[i] * 255));
                                writer.Write(e.shapeId[i]);
                            }
                            
                        }
                        return true;
                    }
                    return false;
                }
            }
        }

        public class ReadMoveLogFile
        {
            Stream stream = null;
            public BinaryReader reader = null;

            public string filename;

            public int version;
            int PlayerNum;
            int AllImposorNum;
            public PlayerMoveArgs e;
            long bytePerMove;
            public long maxMoveNum;
            long PlayerDataByte;
            public GameStartEventArgs startArgs;

            public List<int[]> discFrames = new List<int[]>();
            public List<int[]> deadList = new List<int[]>();
            public List<DrawMove.DeadPos> deadOrderList = new List<DrawMove.DeadPos>();
            public bool[] displayVote;

            public ReadMoveLogFile(string filename)
            {
                this.filename = filename;
                if (!File.Exists(filename))
                {
                    Console.WriteLine($"{filename} not exist");
                    return;
                }
                stream = File.OpenRead(filename);
                reader = new BinaryReader(stream);

                e = new PlayerMoveArgs();
                if (reader != null)
                {
                    startArgs = new GameStartEventArgs();
                    startArgs.PlayerMove = e;


                    version = reader.ReadInt32();
                    startArgs.PlayMap = (PlayerData.PlayMap)reader.ReadInt32();
                    if (version >= 4) startArgs.electricalDoors = reader.ReadUInt32();
                    PlayerNum = reader.ReadInt32();
                    e.myId = reader.ReadInt32();
                    AllImposorNum = reader.ReadInt32();
                    startArgs.filename = filename;

                    startArgs.HatIds = new uint[PlayerData.MaxPlayerNum];
                    startArgs.PetIds = new uint[PlayerData.MaxPlayerNum];
                    startArgs.SkinIds = new uint[PlayerData.MaxPlayerNum];
                    startArgs.RoleType = new RoleTypes[PlayerData.MaxPlayerNum];

                    e.PlayerNames = new string[PlayerData.MaxPlayerNum];

                    e.PlayerNum = PlayerNum;
                    e.PlayerColors = new Color[PlayerData.MaxPlayerNum];
                    e.IsImpostor = new bool[PlayerData.MaxPlayerNum];
                    e.PlayerPoses = new Vector2[PlayerData.MaxPlayerNum];
                    e.PlayerIsDead = new int[PlayerData.MaxPlayerNum];
                    e.ImpostorId = new int[PlayerData.MaxPlayerNum];
                    e.InVent = new bool[PlayerData.MaxPlayerNum];
                    e.TaskProgress = new float[PlayerData.MaxPlayerNum];
                    e.Sabotage = new TaskInfo();
                    e.IsGuardian = new bool[PlayerData.MaxPlayerNum];
                    e.protectedByGuardian = new bool[PlayerData.MaxPlayerNum];
                    e.shapeId = new sbyte[PlayerData.MaxPlayerNum];
                    e.RemainingEmergencies = new sbyte[PlayerData.MaxPlayerNum];
                    e.ReportTarget = -1;
                    e.EmergencyCooldown = 0;
                    if (version > 0) e.voteList = new sbyte[PlayerData.MaxPlayerNum];

                    for (int i = 0; i < PlayerNum; i++)
                    {
                        startArgs.RoleType[i] = RoleTypes.Crewmate;
                        e.shapeId[i] = -1;
                    }
                    for (int i = 0; i < AllImposorNum; i++)
                    {
                        e.ImpostorId[i] = reader.ReadInt32();
                        startArgs.RoleType[e.ImpostorId[i]] = RoleTypes.Impostor;
                    }
                    for (int i = 0; i < PlayerNum; i++)
                    {
                        e.PlayerNames[i] = reader.ReadString();
                        if (version > 2)
                        {
                            startArgs.RoleType[i] = (RoleTypes)reader.ReadByte();
                            e.RemainingEmergencies[i] = reader.ReadSByte();
                        }
                        if (version >= 2) {
                            int colorid = reader.ReadInt32();
                            if (colorid >= 0 && colorid < PlayerData.PlayerColorNum)
                                e.PlayerColors[i] = PlayerData.ColorList[colorid];
                            else
                                e.PlayerColors[i] = Color.Empty;
                        }
                        else
                            e.PlayerColors[i] = PlayerData.oldColor2newColorDict.GetValueOrDefault(reader.ReadInt32());
                        e.IsImpostor[i] = reader.ReadBoolean();

                        startArgs.HatIds[i] = (uint)reader.ReadInt32();
                        startArgs.PetIds[i] = (uint)reader.ReadInt32();
                        startArgs.SkinIds[i] = (uint)reader.ReadInt32();
                    }
                    if (version == 0)
                        bytePerMove = 8 + AllImposorNum + 10 * PlayerNum;
                    else if (version == 1)
                        bytePerMove = 10 + AllImposorNum + 10 * PlayerNum;
                    else if (version == 2)
                        bytePerMove = 11 + AllImposorNum + 10 * PlayerNum;
                    else if (version == 3)
                        bytePerMove = 23 + 11 * PlayerNum;
                    else if (version > 3)
                        bytePerMove = 24 + 11 * PlayerNum;
                    PlayerDataByte = stream.Position;
                    maxMoveNum = (stream.Length - PlayerDataByte) / bytePerMove - 1;
                    displayVote = new bool[maxMoveNum + 1];
                    getFrameData();
                }
                else { Console.WriteLine($"BinaryReader is null"); }
            }

            ~ReadMoveLogFile()
            {
                Close();
            }

            public void Close()
            {
                reader?.Close();
                reader = null;
                stream?.Close();
                stream = null;
            }



            public void seek(int frame)
            {
                if (stream != null) stream.Seek(PlayerDataByte + bytePerMove * frame, 0);
            }

            public PlayerMoveArgs ReadFrombFileMove()
            {
                if (reader != null)
                {
                    if (version <= 1) return ReadFrombFileMove_v0_v1();
                    else if (version == 2) return ReadFrombFileMove_v2();
                    else if (version == 3) return ReadFrombFileMove_v3();
                    else return ReadFrombFileMove_v4();

                }
                return e;
            }


            public PlayerMoveArgs ReadFrombFileMove_v0_v1()
            {
                if (reader != null)
                {
                    try
                    {
                        e.displayVote = displayVote[getFrame()];
                        e.time = reader.ReadInt32();
                        e.state = (GameState)reader.ReadByte();
                        e.Sabotage.TaskType = (TaskTypes)reader.ReadByte();
                        if (version == 0)
                        {
                            e.Sabotage.TaskStep = reader.ReadByte();
                            e.Sabotage.AllStepNum = reader.ReadByte();
                        } else
                        {
                            e.doorsUint = reader.ReadUInt32();
                        }
                        for (int i = 0; i < AllImposorNum; i++) e.InVent[e.ImpostorId[i]] = reader.ReadBoolean();
                        for (int i = 0; i < PlayerNum; i++)
                        {
                            e.PlayerPoses[i].X = reader.ReadSingle();
                            e.PlayerPoses[i].Y = reader.ReadSingle();
                            e.PlayerIsDead[i] = reader.ReadSByte();
                            if (version > 0 && (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote))
                            {
                                sbyte votedFor = reader.ReadSByte();
                                bool reported = false;
                                if (votedFor > 20)
                                {
                                    votedFor -= 32;
                                    reported = true;
                                }

                                if (votedFor == 14) e.voteList[i] = -2; // not vote
                                else if (votedFor == -2) e.voteList[i] = -1; // not vote
                                else if (votedFor == -3) e.voteList[i] = -4; //Error
                                else if (votedFor == -1) e.voteList[i] = -3; //skip
                                else e.voteList[i] = votedFor;
                                if (reported) e.voteList[i] += 32;
                            } 
                            else
                                e.TaskProgress[i] = reader.ReadByte() / 255.0f;
                        }
                    }
                    catch (EndOfStreamException er)
                    {

                    }
                }
                return e;
            }

            public PlayerMoveArgs ReadFrombFileMove_v2()
            {
                if (reader != null)
                {
                    try
                    {
                        e.displayVote = displayVote[getFrame()];
                        e.time = reader.ReadInt32();
                        e.state = (GameState)reader.ReadByte();
                        e.Sabotage.TaskType = (TaskTypes)reader.ReadByte();
                        reader.ReadByte();
                        e.doorsUint = reader.ReadUInt32();
                        for (int i = 0; i < AllImposorNum; i++) e.InVent[e.ImpostorId[i]] = reader.ReadBoolean();
                        for (int i = 0; i < PlayerNum; i++)
                        {
                            e.PlayerPoses[i].X = reader.ReadSingle();
                            e.PlayerPoses[i].Y = reader.ReadSingle();
                            e.PlayerIsDead[i] = reader.ReadSByte();
                            if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote)
                                e.voteList[i] = reader.ReadSByte();
                            else
                                e.TaskProgress[i] = reader.ReadByte() / 255.0f;
                        }
                    }
                    catch (EndOfStreamException er)
                    {

                    }
                }
                return e;
            }

            public PlayerMoveArgs ReadFrombFileMove_v3()
            {
                if (reader != null)
                {
                    try
                    {
                        e.displayVote = displayVote[getFrame()];
                        e.time = reader.ReadInt32();
                        e.state = (GameState)reader.ReadByte();
                        e.Sabotage.TaskType = (TaskTypes)reader.ReadByte();
                        if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote) e.ReportTarget = reader.ReadSByte();
                        else e.EmergencyCooldown = reader.ReadSByte();
                        e.doorsUint = reader.ReadUInt32();
                        uint inVent = reader.ReadUInt32();
                        uint isGuardian = reader.ReadUInt32();
                        uint protectedByGuardian = reader.ReadUInt32();
                        for (int i = 0; i < e.PlayerNum; i++)
                        {
                            e.InVent[i] = (inVent & ((uint)1 << i)) != 0;
                            e.IsGuardian[i] = (isGuardian & ((uint)1 << i)) != 0;
                            e.protectedByGuardian[i] = (protectedByGuardian & ((uint)1 << i)) != 0;
                        }
                        for (int i = 0; i < PlayerNum; i++)
                        {
                            e.PlayerPoses[i].X = reader.ReadSingle();
                            e.PlayerPoses[i].Y = reader.ReadSingle();
                            e.PlayerIsDead[i] = reader.ReadSByte();
                            if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote)
                            {
                                e.voteList[i] = reader.ReadSByte();
                                e.RemainingEmergencies[i] = reader.ReadSByte();
                            }
                            else
                            {
                                e.TaskProgress[i] = reader.ReadByte() / 255.0f;
                                e.shapeId[i] = reader.ReadSByte();
                            }
                            
                        }
                    }
                    catch (EndOfStreamException er)
                    {

                    }
                }
                return e;
            }

            public PlayerMoveArgs ReadFrombFileMove_v4()
            {
                if (reader != null)
                {
                    try
                    {
                        e.displayVote = displayVote[getFrame()];
                        e.time = reader.ReadInt32();
                        e.state = (GameState)reader.ReadByte();
                        e.Sabotage.TaskType = (TaskTypes)reader.ReadByte();
                        if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote) e.ReportTarget = reader.ReadSByte();
                        else e.EmergencyCooldown = reader.ReadSByte();
                        e.CameraOn = reader.ReadBoolean();
                        e.doorsUint = reader.ReadUInt32();
                        uint inVent = reader.ReadUInt32();
                        uint isGuardian = reader.ReadUInt32();
                        uint protectedByGuardian = reader.ReadUInt32();
                        for (int i = 0; i < e.PlayerNum; i++)
                        {
                            e.InVent[i] = (inVent & ((uint)1 << i)) != 0;
                            e.IsGuardian[i] = (isGuardian & ((uint)1 << i)) != 0;
                            e.protectedByGuardian[i] = (protectedByGuardian & ((uint)1 << i)) != 0;
                        }
                        for (int i = 0; i < PlayerNum; i++)
                        {
                            e.PlayerPoses[i].X = reader.ReadSingle();
                            e.PlayerPoses[i].Y = reader.ReadSingle();
                            e.PlayerIsDead[i] = reader.ReadSByte();
                            if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote)
                            {
                                e.voteList[i] = reader.ReadSByte();
                                e.RemainingEmergencies[i] = reader.ReadSByte();
                            }
                            else
                            {
                                e.TaskProgress[i] = reader.ReadByte() / 255.0f;
                                e.shapeId[i] = reader.ReadSByte();
                            }
                            
                        }
                    }
                    catch (EndOfStreamException er)
                    {

                    }
                }
                return e;
            }

            public int getFrame()
            {
                int frame = (int)((reader.BaseStream.Position - PlayerDataByte) / bytePerMove);
                if (frame < 0) frame = 0;
                return frame;
            }

            public void getFrameData()
            {

                if (reader == null) return;
                discFrames.Clear();
                deadList.Clear();
                deadOrderList.Clear();


                long readerPos = reader.BaseStream.Position;
                seek(0);
                e = ReadFrombFileMove();
                seek(0);
                GameState oldState = GameState.UNKNOWN;

                int playerNum = e.PlayerNum;
                int[] oldPlayerIsDead = new int[playerNum];
                displayVote = new bool[maxMoveNum + 1];


                int discFrame = 0;
                for (int i = 0; i <= maxMoveNum; i++)
                {
                    e = ReadFrombFileMove();
                    if (oldState != e.state)
                    {
                        if (e.state == GameState.DISCUSSION)
                            discFrame = i;
                        else if (oldState == GameState.DISCUSSION)
                            discFrames.Add(new int[2] { discFrame, i });

                    }
                    oldState = e.state;
                    for (int j = 0; j < playerNum; j++)
                    {
                        if (e.PlayerIsDead[j] != 0 && oldPlayerIsDead[j] == 0)
                        {
                            deadList.Add(new int[2] { i, j });
                            deadOrderList.Add(new DrawMove.DeadPos(j, e.PlayerPoses[j]));
                        }
                        oldPlayerIsDead[j] = e.PlayerIsDead[j];
                    }
                    if (e.state == GameState.DISCUSSION && i - discFrame <= 40)
                    {
                        bool voted = false;
                        for (int j = 0; j < e.PlayerNum; j++)
                        {
                            sbyte vote = e.voteList[j];
                            if (vote > 20) vote -= 32;
                            if (e.PlayerIsDead[j] == 0 && (vote <= -2 || vote >= 0) && vote < PlayerData.MaxPlayerNum) voted = true;
                        }
                        displayVote[i] = voted;
                    }
                    else if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote)
                        displayVote[i] = true;
                    else displayVote[i] = false;
                }
                if (e.state == GameState.DISCUSSION)
                    discFrames.Add(new int[2] { discFrame, (int)maxMoveNum });
                reader.BaseStream.Position = readerPos;
                e = ReadFrombFileMove();
            }

        }

        public class WriteMoveLogFile_chatLogFile : WriteMoveLogFile
        {
            private string tempChatfilename;
            private string chatfilename;
            private Stream chatstream = null;
            private StreamWriter chatwriter = null;
            private Dictionary<string, int> PlayerName2Id = new Dictionary<string, int>();
            private string[] Name2WithInfo = new string[PlayerData.MaxPlayerNum];
            private readonly bool outputTextlog = false;
            public WriteMoveLogFile_chatLogFile(GameStartEventArgs startArgs, bool outputTextlog = true) :base(startArgs)
            {
                this.outputTextlog = outputTextlog;
                if (!outputTextlog) return;
                lock (lockObject)
                {
                    tempChatfilename = Path.ChangeExtension(tempfilename, "txt");
                    chatfilename = Path.ChangeExtension(filename, "txt");
                    try
                    {
                        chatstream = File.Create(tempChatfilename);
                        chatwriter = new StreamWriter(chatstream);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        chatwriter?.Close();
                        chatstream?.Close();
                        chatwriter = null;
                        chatstream = null;
                        return;
                    }
                    int playerNum = startArgs.PlayerMove.PlayerNum;
                    var m = startArgs.PlayerMove;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Code:" + startArgs.LobbyCode + "\n\n");
                    sb.Append("☆Impostor:\n");
                    for (int i = 0; i < AllImposorNum; i++)
                    {
                        sb.Append(m.PlayerNames[m.ImpostorId[i]] + "/" + PlayerData.ColorNameDict[m.PlayerColors[m.ImpostorId[i]].ToArgb()]);
                        if (startArgs.RoleType[m.ImpostorId[i]] != RoleTypes.Impostor) sb.Append("\t :" + startArgs.RoleType[m.ImpostorId[i]].ToString());
                        sb.Append("\n");
                    }

                    sb.Append("\nCrewmate:\n");
                    for (int i = 0; i < playerNum; i++)
                    {
                        PlayerName2Id[m.PlayerNames[i]] = i;
                        string nameinfo = m.PlayerNames[i] + "/" + PlayerData.ColorNameDict[m.PlayerColors[i].ToArgb()];
                        Name2WithInfo[i] = nameinfo + ":";
                        if (!m.IsImpostor[i])
                        {
                            sb.Append(nameinfo);
                            if (startArgs.RoleType[i] != RoleTypes.Crewmate)
                            {
                                sb.Append("\t :" + startArgs.RoleType[i].ToString());
                                Name2WithInfo[i] = Name2WithInfo[i] + startArgs.RoleType[i].ToString().Substring(0, 1) + ":";
                            }
                            sb.Append("\n");
                        }
                    }
                    for (int i = 0; i < AllImposorNum; i++)
                    {
                        int id = m.ImpostorId[i];
                        Name2WithInfo[id] = "☆" + Name2WithInfo[id];
                        if (startArgs.RoleType[id] != RoleTypes.Impostor)
                            Name2WithInfo[id] = Name2WithInfo[id] + startArgs.RoleType[id].ToString().Substring(0, 1) + ":";
                    }
                    sb.Append("\n\n");
                    chatwriter.Write(sb.ToString());
                }
            }

            public void WriteChat(ChatMessageEventArgs chat)
            {
                if (!outputTextlog || chat == null) return;
                lock (lockObject)
                {
                    if (chatwriter == null) return;
                    int m = chat.time / (1000 * 60);
                    int s = chat.time / 1000 - m * 60;
                    string timestr = $"{m:00}:{s:00} ";
                    if (chat.time < 0) timestr = "";
                    if (chat.Sender != null)
                    {
                        int id;
                        if (PlayerName2Id.TryGetValue(chat.Sender, out id))
                        {
                            string nameinfo = Name2WithInfo[id] + "\t";
                            if (move.PlayerIsDead[id] != 0) nameinfo += "(Dead) ";
                            chatwriter.WriteLine(timestr + nameinfo + chat.Message);
                            return;
                        }
                    }
                    chatwriter.WriteLine(timestr + chat.Sender + chat.Message);
                }
            }

            public void WritePostGameChat(ChatMessageEventArgs chat)
            {
                if (!outputTextlog || chat == null) return;
                lock (lockObject)
                {
                    if (chat.Sender != null)
                    {
                        int id;
                        if (PlayerName2Id.TryGetValue(chat.Sender, out id))
                        {
                            string nameinfo = Name2WithInfo[id] + "\t";
                            if (move.PlayerIsDead[id] != 0) nameinfo += "(Dead) ";
                            File.AppendAllText(chatfilename, nameinfo + chat.Message + "\n");
                            return;
                        }
                    }
                    File.AppendAllText(chatfilename, chat.Sender + ":" + chat.Message + "\n");
                }
            }

            ~WriteMoveLogFile_chatLogFile()
            {
                UnexpectedClose();
            }
            override public void UnexpectedClose()
            {
                base.UnexpectedClose();
                lock (lockObject)
                {
                    try{
                        chatwriter?.Close();
                        chatstream?.Close();
                        chatwriter = null;
                        chatstream = null;
                    } catch (ObjectDisposedException e) { }
                }
            }

            override public void Close()
            {
                base.Close();
                UnexpectedClose();
                lock (lockObject)
                {
                    try
                    {
                        if (File.Exists(tempChatfilename))
                            File.Move(tempChatfilename, chatfilename);
                    }
                    catch (Exception e) { }
                }
            }
        }

    }
}
