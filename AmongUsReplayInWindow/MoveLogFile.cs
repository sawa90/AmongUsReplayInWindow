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

        public class WriteMoveLogFile
        {
            static int version = 1;
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
                    folderPass = Program.exeFolder + "\\replay";
                    AllImposorNum = 0;
                    for (int i = 0; i < startArgs.PlayerMove.PlayerNum; i++)
                    {
                        if (startArgs.PlayerMove.IsImpostor[i])
                        {
                            AllImposorNum++;
                        }
                    }
                    if (!Directory.Exists(folderPass))
                    {
                        Console.WriteLine($"create folder {folderPass}");
                        Directory.CreateDirectory(folderPass);
                    }
                    this.filename = folderPass + "\\" + startArgs.filename + ".dat";
                    tempfilename = folderPass + "\\" + DateTime.Now.ToString("yyyyMMdd_HHmm_ss") + ".dat";
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
                Close();
            }

            public virtual void Close()
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
                        if (File.Exists(tempfilename))
                            File.Move(tempfilename, filename);
                    } catch (ObjectDisposedException e)
                    {
                        writer = null;
                        stream = null;
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
                        writer.Write((Int32)startArgs.PlayerMove.PlayerNum);
                        writer.Write((Int32)startArgs.PlayerMove.myId);
                        writer.Write((Int32)AllImposorNum);
                        for (int i = 0; i < AllImposorNum; i++) writer.Write((Int32)startArgs.PlayerMove.ImpostorId[i]);

                        for (int i = 0; i < startArgs.PlayerMove.PlayerNum; i++)
                        {
                            writer.Write(startArgs.PlayerMove.PlayerNames[i]);
                            writer.Write((Int32)startArgs.PlayerMove.PlayerColors[i].ToArgb());
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
                        writer.Write((UInt32)e.doorsUint);
                        for (int i = 0; i < AllImposorNum; i++) writer.Write((bool)e.InVent[i]);

                        for (int i = 0; i < e.PlayerNum; i++)
                        {
                            writer.Write(e.PlayerPoses[i].X);
                            writer.Write(e.PlayerPoses[i].Y);
                            writer.Write((sbyte)(e.PlayerIsDead[i]));
                            if (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote)
                                writer.Write((sbyte)e.voteList[i]);
                            else
                                writer.Write((byte)(e.TaskProgress[i] * 255));
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

            int version;
            int PlayerNum;
            int AllImposorNum;
            public PlayerMoveArgs e;
            long bytePerMove;
            public long maxMoveNum;
            long PlayerDataByte;
            public GameStartEventArgs startArgs;

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
                    startArgs.PlayMap = (PlayMap)reader.ReadInt32();
                    PlayerNum = reader.ReadInt32();
                    e.myId = reader.ReadInt32();
                    AllImposorNum = reader.ReadInt32();
                    startArgs.filename = filename;

                    startArgs.HatIds = new uint[10];
                    startArgs.PetIds = new uint[10];
                    startArgs.SkinIds = new uint[10];

                    e.PlayerNames = new string[10];

                    e.PlayerNum = PlayerNum;
                    e.PlayerColors = new Color[10];
                    e.IsImpostor = new bool[10];
                    e.PlayerPoses = new Vector2[10];
                    e.PlayerIsDead = new int[10];
                    e.ImpostorId = new int[3];
                    e.InVent = new bool[3];
                    e.TaskProgress = new float[10];
                    e.Sabotage = new TaskInfo();
                    if (version > 0) e.voteList = new sbyte[10];

                    for (int i = 0; i < AllImposorNum; i++) e.ImpostorId[i] = reader.ReadInt32();
                    for (int i = 0; i < PlayerNum; i++)
                    {
                        e.PlayerNames[i] = reader.ReadString();
                        e.PlayerColors[i] = Color.FromArgb(reader.ReadInt32());
                        e.IsImpostor[i] = reader.ReadBoolean();

                        startArgs.HatIds[i] = (uint)reader.ReadInt32();
                        startArgs.PetIds[i] = (uint)reader.ReadInt32();
                        startArgs.SkinIds[i] = (uint)reader.ReadInt32();

                    }
                    if (version == 0)
                        bytePerMove = 8 + AllImposorNum + 10 * PlayerNum;
                    else
                        bytePerMove = 10 + AllImposorNum + 10 * PlayerNum;
                    PlayerDataByte = stream.Position;
                    maxMoveNum = (stream.Length - PlayerDataByte) / bytePerMove - 1;

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
                    try
                    {
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
                        for (int i = 0; i < AllImposorNum; i++) e.InVent[i] = reader.ReadBoolean();
                        for (int i = 0; i < PlayerNum; i++)
                        {
                            e.PlayerPoses[i].X = reader.ReadSingle();
                            e.PlayerPoses[i].Y = reader.ReadSingle();
                            e.PlayerIsDead[i] = reader.ReadSByte();
                            if (version > 0 && (e.state == GameState.DISCUSSION || e.state == GameState.VotingResult || e.state == GameState.HumansWinByVote || e.state == GameState.ImpostorWinByVote))
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


        }

        public class WriteMoveLogFile_chatLogFile : WriteMoveLogFile
        {
            private string tempChatfilename;
            private Stream chatstream;
            private StreamWriter chatwriter;
            private Dictionary<string, int> PlayerName2Id = new Dictionary<string, int>();
            private Dictionary<string, string> Name2WithInfo = new Dictionary<string, string>();
            public WriteMoveLogFile_chatLogFile(GameStartEventArgs startArgs) :base(startArgs)
            {
                lock (lockObject)
                {
                    tempChatfilename = Path.ChangeExtension(filename, "txt");
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
                    sb.Append("Impostor:\n");
                    for (int i = 0; i < AllImposorNum; i++)
                    {
                        sb.Append(m.PlayerNames[m.ImpostorId[i]] + "/" + GameMemReader.ColorNameDict[m.PlayerColors[m.ImpostorId[i]].ToArgb()] + "\n");
                    }

                    sb.Append("\nCrewmate:\n");
                    int NameInfoMax = 0;
                    for (int i = 0; i < playerNum; i++)
                    {
                        PlayerName2Id[m.PlayerNames[i]] = i;
                        string nameinfo = m.PlayerNames[i] + "/" + GameMemReader.ColorNameDict[m.PlayerColors[i].ToArgb()];
                        Name2WithInfo[m.PlayerNames[i]] = nameinfo + ":";
                        if (nameinfo.Length > NameInfoMax) NameInfoMax = nameinfo.Length;
                        if (!m.IsImpostor[i])
                            sb.Append(nameinfo + "\n");
                    }
                    for (int i = 0; i < playerNum; i++)
                        Name2WithInfo[m.PlayerNames[i]] = Name2WithInfo[m.PlayerNames[i]].PadRight(NameInfoMax + 2);
                    for (int i = 0; i < AllImposorNum; i++)
                    {
                        Name2WithInfo[m.PlayerNames[m.ImpostorId[i]]] = "☆" + Name2WithInfo[m.PlayerNames[m.ImpostorId[i]]];
                    }
                    sb.Append("\n\n");
                    chatwriter.Write(sb.ToString());
                }
            }

            public void WriteChat(ChatMessageEventArgs chat)
            {
                lock (lockObject)
                {
                    if (chatwriter == null) return;
                    if (chat.Sender != null)
                    {
                        string nameinfo;
                        if (Name2WithInfo.TryGetValue(chat.Sender, out nameinfo))
                        {
                            chatwriter.WriteLine(nameinfo + chat.Message);
                            return;
                        }
                    }
                    chatwriter.WriteLine(chat.Sender + chat.Message);
                }
            }


            override public void Close()
            {
                base.Close();
                lock (lockObject)
                {
                    chatwriter?.Close();
                    chatstream?.Close();
                    chatwriter = null;
                    chatstream = null;
                }
            }
        }

    }
}
