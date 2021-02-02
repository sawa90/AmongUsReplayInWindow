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
        public class Handler_WriteMoveLogFile
        {
            MoveLogFile.WriteMoveLogFile writer = null;
            public PlayerMoveArgs e = null;
            string filename;
            bool Playing = true;
            int discussionTime = -1000;
            GameMemReader instance;


            public Handler_WriteMoveLogFile(GameMemReader instance)
            {
                this.instance = instance;
                instance.PlayerMove += PlayerPosHandler;
                instance.GameStateChanged += GameStateChangedEventHandler;
                instance.GameStart += GameStartHandler;
            }

            ~Handler_WriteMoveLogFile()
            {
                Close();
            }

            public void Close()
            {
                if (instance!=null)
                {
                    instance.PlayerMove -= PlayerPosHandler;
                    instance.GameStateChanged -= GameStateChangedEventHandler;
                    instance.GameStart -= GameStartHandler;
                    instance = null;
                }
            }

            public void GameStartHandler(object? sender, GameStartEventArgs startArgs)
            {
                Playing = true;
                discussionTime = -1000;
                writer?.Close();
                writer = new MoveLogFile.WriteMoveLogFile(startArgs);
            }

            void finishWriter()
            {
                if (writer != null)
                {
                    writer.Close();
                    filename = writer.filename;
                    writer = null;
                }
            }
            public void GameStateChangedEventHandler(object? sender, GameStateChangedEventArgs stateArgs)
            {
                if (stateArgs != null)
                {
                    var newState = stateArgs.NewState;
                    if (newState == GameState.MENU || newState == GameState.LOBBY)
                    {
                        finishWriter();
                        Playing = false;
                    }
                    else if (!Playing)
                    {
                        Playing = true;
                    }

                }

            }



            public void PlayerPosHandler(object? sender, PlayerMoveArgs moveArgs)
            {

                if (Playing)
                {
                    e = moveArgs;
                    if (e.state != GameState.DISCUSSION) writer?.writeMove2bFile(moveArgs);
                    else if (e.time - discussionTime > 1000)
                    {
                        writer?.writeMove2bFile(moveArgs);
                        discussionTime = e.time;
                    }

                    if (e.state >= GameState.ENDED) finishWriter();
                }

            }


        }
        public class WriteMoveLogFile
        {
            static int version = 0;
            static string folderPass = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Among Us\\AmongUsReplayInWindow";
            private Stream stream;
            private BinaryWriter writer;
            int AllImposorNum;
            public string filename;
            private object lockObject = new object();

            public WriteMoveLogFile(GameStartEventArgs startArgs)
            {
                lock (lockObject)
                {
                    AllImposorNum = 0;
                    for (int i = 0; i < startArgs.PlayerMove.PlayerNum; i++)
                    {
                        if (startArgs.PlayerMove.IsImpostor[i])
                        {
                            AllImposorNum++;
                        }
                    }
                    if (!File.Exists(folderPass)) Directory.CreateDirectory(folderPass);
                    this.filename = folderPass + "\\" + startArgs.filename + ".dat";
                    stream = File.Create(this.filename);
                    writer = new BinaryWriter(stream);
                    writePlayerData2bFile(startArgs);
                }
            }

            ~WriteMoveLogFile()
            {
                Close();
            }

            public void Close()
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
                    } catch(ObjectDisposedException e)
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
                    if (writer != null)
                    {
                        writer.Write((Int32)e.time);
                        writer.Write((byte)e.state);
                        writer.Write((byte)e.Sabotage.TaskType);
                        writer.Write((byte)e.Sabotage.TaskStep);
                        writer.Write((byte)e.Sabotage.AllStepNum);
                        for (int i = 0; i < AllImposorNum; i++) writer.Write((bool)e.InVent[i]);

                        for (int i = 0; i < e.PlayerNum; i++)
                        {
                            writer.Write(e.PlayerPoses[i].X);
                            writer.Write(e.PlayerPoses[i].Y);
                            writer.Write((sbyte)(e.PlayerIsDead[i]));
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
                if (!File.Exists(filename)) return;
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

                    bytePerMove = 8 + AllImposorNum + 10 * PlayerNum;
                    PlayerDataByte = stream.Position;
                    maxMoveNum = (stream.Length - PlayerDataByte) / bytePerMove - 1;

                }
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
                        e.Sabotage.TaskStep = reader.ReadByte();
                        e.Sabotage.AllStepNum = reader.ReadByte();
                        for (int i = 0; i < AllImposorNum; i++) e.InVent[i] = reader.ReadBoolean();
                        for (int i = 0; i < PlayerNum; i++)
                        {
                            e.PlayerPoses[i].X = reader.ReadSingle();
                            e.PlayerPoses[i].Y = reader.ReadSingle();
                            e.PlayerIsDead[i] = reader.ReadSByte();
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

    }
}
