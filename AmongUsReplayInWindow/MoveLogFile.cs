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
            static int version = 2;
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
                        writer.Write((byte)0);
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
                    startArgs.PlayMap = (PlayerData.PlayMap)reader.ReadInt32();
                    PlayerNum = reader.ReadInt32();
                    e.myId = reader.ReadInt32();
                    AllImposorNum = reader.ReadInt32();
                    startArgs.filename = filename;

                    startArgs.HatIds = new uint[PlayerData.MaxPlayerNum];
                    startArgs.PetIds = new uint[PlayerData.MaxPlayerNum];
                    startArgs.SkinIds = new uint[PlayerData.MaxPlayerNum];

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
                    if (version > 0) e.voteList = new sbyte[PlayerData.MaxPlayerNum];

                    for (int i = 0; i < AllImposorNum; i++) e.ImpostorId[i] = reader.ReadInt32();
                    for (int i = 0; i < PlayerNum; i++)
                    {
                        e.PlayerNames[i] = reader.ReadString();
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
                    else
                        bytePerMove = 11 + AllImposorNum + 10 * PlayerNum;
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
                    if (version <= 1) return ReadFrombFileMove_v0_v1();
                    else return ReadFrombFileMove_v2();

                }
                return e;
            }


            public PlayerMoveArgs ReadFrombFileMove_v0_v1()
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
                        e.time = reader.ReadInt32();
                        e.state = (GameState)reader.ReadByte();
                        e.Sabotage.TaskType = (TaskTypes)reader.ReadByte();
                        reader.ReadByte();
                        e.doorsUint = reader.ReadUInt32();
                        for (int i = 0; i < AllImposorNum; i++) e.InVent[i] = reader.ReadBoolean();
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
                        sb.Append(m.PlayerNames[m.ImpostorId[i]] + "/" + PlayerData.ColorNameDict[m.PlayerColors[m.ImpostorId[i]].ToArgb()] + "\n");
                    }

                    sb.Append("\nCrewmate:\n");
                    for (int i = 0; i < playerNum; i++)
                    {
                        PlayerName2Id[m.PlayerNames[i]] = i;
                        string nameinfo = m.PlayerNames[i] + "/" + PlayerData.ColorNameDict[m.PlayerColors[i].ToArgb()];
                        Name2WithInfo[i] = nameinfo + ":\t";
                        if (!m.IsImpostor[i])
                            sb.Append(nameinfo + "\n");
                    }
                    for (int i = 0; i < AllImposorNum; i++)
                    {
                        Name2WithInfo[m.ImpostorId[i]] = "☆" + Name2WithInfo[m.ImpostorId[i]];
                    }
                    sb.Append("\n\n");
                    chatwriter.Write(sb.ToString());
                }
            }

            public void WriteChat(ChatMessageEventArgs chat)
            {
                if (!outputTextlog) return;
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
                            string nameinfo = Name2WithInfo[id];
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
                if (!outputTextlog) return;
                lock (lockObject)
                {
                    if (chat.Sender != null)
                    {
                        int id;
                        if (PlayerName2Id.TryGetValue(chat.Sender, out id))
                        {
                            string nameinfo = Name2WithInfo[id];
                            if (move.PlayerIsDead[id] != 0) nameinfo += "(Dead) ";
                            File.AppendAllText(chatfilename, nameinfo + chat.Message + "\n");
                        }
                        else
                            File.AppendAllText(chatfilename, chat.Sender + ":" + chat.Message + "\n");
                    }
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
                    if (File.Exists(tempChatfilename))
                        File.Move(tempChatfilename, chatfilename);
                }
            }
        }

    }
}
