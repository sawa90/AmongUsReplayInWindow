using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AmongUsCapture;
using System.Numerics;
using System.Runtime.InteropServices;
using AmongUsReplayInWindow.setOwnerWindow;
using System.IO;

namespace AmongUsReplayInWindow
{
    public partial class OverlayWindow : Form
    {
        #region variable
        public static bool open = false;

        private CancellationTokenSource cancelTokenSource = null;
        public object lockObject = new object();

        public MoveLogFile.ReadMoveLogFile logReader = null;
        MoveLogFile.WriteMoveLogFile writer = null;


        public IntPtr ownerHandle = IntPtr.Zero;
        public int ownerProcessId;
        public TrackBarWin trackwin;

        delegate void voidDelegate();
        delegate void void_intDelegate(int i);
        delegate bool bool_stringDelegate(string str);

        AdjustToOwnerWindow sizeChange = null;

        PaintEventHandler drawTrackBar = null;
        PaintEventHandler backgroundPaint = null;

        internal System.Windows.Forms.Timer drawTimer = new System.Windows.Forms.Timer();

        string[] mapFilename = new string[3] { "skeld.png", "mira.png", "polus.png" };
        Image MapImage;
        public int mapId = 0;

        public PlayerMoveArgs moveArg = null;
        string filename;
        bool Playing = true;
        int discussionTime = -1000;


        public List<int[]> discFrames = new List<int[]>();
        List<int[]> deadList = new List<int[]>();
        List<int> deadOrderList = new List<int>();



        public const int ULW_COLORKEY = 1;
        public const int ULW_ALPHA = 2;
        public const int WS_THICKFRAME = 0x00040000;
        public const int WS_BORDER = 0x00800000;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TOPMOST = 0x00000008;
        internal ConfigWindow configWindow;
        internal bool drawIcon;

        #endregion

        #region Initialize
        public OverlayWindow(ConfigWindow configWindow, CancellationTokenSource tokenSource, System.Diagnostics.Process ownerProcess)
        {
            Console.WriteLine("Init Overlay Window...");
            try
            {
                this.configWindow = configWindow;
                Init();
                cancelTokenSource = tokenSource;
                SetLayeredWindowAttributes(this.Handle, ToCOLORREF(Color.Snow), 210, ULW_COLORKEY | ULW_ALPHA);

                if (ownerProcess != null)
                {
                    ownerHandle = ownerProcess.MainWindowHandle;
                    ownerProcessId = ownerProcess.Id;
                    sizeChange = new AdjustForm1ToOwnerWindow(this, ownerHandle);
                    sizeChange.Start(1000);
                    sizeChange.resize();
                    int processId;
                    int threadId = GetWindowThreadProcessId(ownerHandle, out processId);
                    if (threadId != 0)
                    {
                        Console.WriteLine("Set Keyboad hook...");
                        SetKeyboardHook(threadId, Handle, trackwin.Handle);
                        SetKeyboardEnable(Playing, false);
                        return;

                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            Close();
        }

        ~OverlayWindow()
        {
            overlay_FormClosing(null, null);
        }


        private void Init()
        {
            open = true;
            InitializeComponent();
            pictureBox1.Width = ClientSize.Width;
            pictureBox1.Height = ClientSize.Height;
            pictureBox1.Paint += new PaintEventHandler(Draw);
            FormClosing += new FormClosingEventHandler(overlay_FormClosing);
            trackwin = new TrackBarWin(this);
            setMapImage();
            backgroundPaint = new PaintEventHandler(DrawBackground);
            Paint += backgroundPaint;
            drawTimer = new System.Windows.Forms.Timer();
            drawTimer.Interval = configWindow.interval;
            drawTimer.Tick += new EventHandler(DrawTimerHandler);
            Visible = false;
            drawIcon = configWindow.drawIcon;
        }

        void setMapImage()
        {
            if (File.Exists(Program.exeFolder + "\\map\\" + mapFilename[mapId]))
                MapImage = Image.FromFile(Program.exeFolder + "\\map\\" + mapFilename[mapId]);
            else
            {
                switch (mapId)
                {
                    case 0:
                        MapImage = Properties.Resources.skeld;
                        break;
                    case 1:
                        MapImage = Properties.Resources.mira;
                        break;
                    case 2:
                        MapImage = Properties.Resources.polus;
                        break;
                    default:
                        Console.WriteLine($"Not found map image ID={mapId}");
                        throw new FileNotFoundException();
                }
            }
        }
        #endregion

        private void overlay_FormClosing(Object sender, FormClosingEventArgs ev)
        {
            ResetKeyboardHook();
            cancelTokenSource?.Cancel();
            removeReader();
            drawTimer?.Dispose();
            writer?.Close();
            MapImage?.Dispose();
            sizeChange?.Stop();
            trackwin?.Close();

            drawTimer = null;
            writer = null;
            MapImage = null;
            cancelTokenSource = null;
            sizeChange = null;
            trackwin = null;

            open = false;
        }


        #region set reader

        bool drawPlaying = false;
        public bool setReader(string filename)
        {
            Console.WriteLine("Set reader");
            lock (lockObject)
            {
                logReader = new MoveLogFile.ReadMoveLogFile(filename);
                if (logReader?.reader == null)
                {
                    logReader?.Close();
                    logReader = null;
                    Console.WriteLine("Failed to set reader");
                    return false;
                }
                MapChange((int)logReader.startArgs.PlayMap);
                getFrameData();
            }
            drawTrackBar = new PaintEventHandler(DrawBar);
            pictureBox2.Paint += drawTrackBar;
            pictureBox2.Invalidate();
            StartDraw();
            trackwin.Invoke(new voidDelegate(trackwin.setHandler));
            return true;
        }



        private void removeReader()
        {
            Console.WriteLine("Remove reader");
            trackwin.Invoke(new voidDelegate(trackwin.deleteHandler));
            lock (lockObject)
            {
                logReader?.Close();
                logReader = null;
            }
            if (drawTrackBar != null)
            {
                pictureBox2.Paint -= drawTrackBar;
                drawTrackBar = null;
            }
            if (!drawPlaying) StopDraw();
            discFrames.Clear();
            deadList.Clear();
            deadOrderList.Clear();
        }

        private void getFrameData()
        {

            if (logReader?.reader == null) return;
            discFrames.Clear();
            deadList.Clear();
            deadOrderList.Clear();


            long readerPos = logReader.reader.BaseStream.Position;
            logReader.seek(0);

            GameState oldState = GameState.UNKNOWN;


            int playerNum = logReader.e.PlayerNum;
            int[] oldPlayerIsDead = new int[playerNum];


            int discFrame = 0;
            for (int i = 0; i <= logReader.maxMoveNum; i++)
            {
                moveArg = logReader.ReadFrombFileMove();
                if (oldState != moveArg.state)
                {
                    if (moveArg.state == GameState.DISCUSSION)
                        discFrame = i;
                    else if (oldState == GameState.DISCUSSION)
                        discFrames.Add(new int[2] { discFrame, i });

                }
                oldState = moveArg.state;
                for (int j = 0; j < playerNum; j++)
                {
                    if (moveArg.PlayerIsDead[j] != 0 && oldPlayerIsDead[j] == 0)
                    {
                        deadList.Add(new int[2] { i, j });
                        deadOrderList.Add(j);
                    }
                    oldPlayerIsDead[j] = moveArg.PlayerIsDead[j];
                }
            }
            if (moveArg.state == GameState.DISCUSSION)
                discFrames.Add(new int[2] { discFrame, (int)logReader.maxMoveNum });
            logReader.reader.BaseStream.Position = readerPos;

        }

        #endregion


        #region Handler

        public void GameStartHandler(object? sender, GameStartEventArgs startArgs)
        {
            Console.WriteLine("game start");
            Playing = true;
            discussionTime = -1000;
            MapChange((int)startArgs.PlayMap);
            Invoke(new voidDelegate(removeReader));
            writer?.Close();
            writer = new MoveLogFile.WriteMoveLogFile(startArgs);
            using (var g = CreateGraphics())
                g.FillRectangle(Brushes.Snow, pictureBox1.Location.X, pictureBox1.Location.Y, pictureBox1.Size.Width, pictureBox1.Size.Height);

            using (var g = pictureBox2.CreateGraphics())
                g.FillRectangle(Brushes.Snow, 0, 0, pictureBox2.Size.Width, pictureBox2.Size.Height);

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
                    Invoke(new voidDelegate(removeReader));
                    if (filename != null) Invoke(new bool_stringDelegate(setReader), filename);
                }
                else if (!Playing)
                {
                    Playing = true;
                    Invoke(new voidDelegate(removeReader));
                }

            }

        }

        public void DrawTimerHandler(object? sender, EventArgs eArgs)
        {
            pictureBox1.Invalidate();
        }
        public void PlayerPosHandler(object? sender, PlayerMoveArgs moveArgs)
        {
            if (Playing)
            {
                moveArg = moveArgs;
                if (moveArg.state != GameState.DISCUSSION) writer?.writeMove2bFile(moveArgs);
                else if (moveArg.time - discussionTime > 1000)
                {
                    writer?.writeMove2bFile(moveArgs);
                    discussionTime = moveArg.time;
                }

                if (moveArg.state >= GameState.ENDED) finishWriter();
            }

        }

        #endregion


        #region start or stop draw
        void StartDraw()
        {
            sizeChange?.resize();
            sizeChange?.Start();
            using (var g = CreateGraphics())
                if (MapImage != null)
                {
                    g.FillRectangle(Brushes.Snow, pictureBox1.Location.X, pictureBox1.Location.Y, pictureBox1.Size.Width, pictureBox1.Size.Height);
                    g.DrawImage(MapImage, pictureBox1.Location.X, pictureBox1.Location.Y, pictureBox1.Size.Width, pictureBox1.Size.Height);
                }
            drawTimer.Interval = configWindow.interval;
            drawTimer?.Start();
            SetKeyboardEnable(Playing, true);
            ShowWindow(Handle, SW_SHOWNA);

        }

        void StopDraw()
        {
            drawTimer?.Stop();
            SetKeyboardEnable(Playing, false);
            sizeChange?.StopResize();
            ShowWindow(Handle, SW_HIDE);
        }
        #endregion

        #region Draw


        private void DrawBackground(object sender, System.Windows.Forms.PaintEventArgs paint)
        {
            if (!Playing)
                paint.Graphics.DrawImage(MapImage, pictureBox1.Location.X, pictureBox1.Location.Y, pictureBox1.Size.Width, pictureBox1.Size.Height);
        }


        private void DrawBar(object sender, System.Windows.Forms.PaintEventArgs paint)
        {
            if (logReader?.reader == null) return;
            Graphics g = paint.Graphics;
            float wPerFrame = (float)pictureBox2.ClientSize.Width / logReader.maxMoveNum;

            g.FillRectangle(Brushes.LightGray, 0, 0, pictureBox2.ClientSize.Width, pictureBox2.ClientSize.Height);
            foreach (var ij in discFrames)
            {
                g.FillRectangle(Brushes.Gray, wPerFrame * ij[0], 0, wPerFrame * (ij[1] - ij[0]), pictureBox2.ClientSize.Height);
            }

            float circleSize = pictureBox1.Height / 39.0f;
            float dsize = Math.Max(1.0f, circleSize / 5.0f);
            foreach (var ij in deadList)
            {
                using (var brush = new SolidBrush(moveArg.PlayerColors[ij[1]]))
                    g.FillRectangle(brush, wPerFrame * ij[0] - dsize, 0, dsize * 2, pictureBox2.ClientSize.Height);
            }
        }


        public void MapChange(int newMapId)
        {
            if (mapId == newMapId) return;
            Invoke(new void_intDelegate(delegateMapChange), newMapId);
        }

        private void delegateMapChange(int newMapId)
        {
            if (mapId == newMapId) return;
            mapId = newMapId;
            MapImage.Dispose();
            setMapImage();
            sizeChange.resize();
            Invalidate();
        }
        private void Draw(object sender, PaintEventArgs paint)
        {
            lock (lockObject)
            {
                if (drawIcon && configWindow?.iconDict != null) 
                {
                    DrawMove_Icon(paint, moveArg, deadOrderList, Map.Maps[mapId], configWindow.iconDict, pictureBox1.Width, pictureBox1.Height);

                }
                else
                    DrawMove(paint, moveArg, deadOrderList, Map.Maps[mapId], pictureBox1.Width, pictureBox1.Height);
            }

        }


        public class IconDict
        {
            public Dictionary<Color, Image> icons = null;
            public Image impostor = null;
            public Image vent = null;
            public IconDict()
            {
                try
                {
                    using (FileStream stream = File.OpenRead(Program.exeFolder + "\\icon\\impostor.png"))
                    {
                        impostor = Image.FromStream(stream, false, false);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message + "\n" + e.StackTrace + "\n");
                    impostor = Properties.Resources.impostor;
                }

                try
                {
                    using (FileStream stream = File.OpenRead(Program.exeFolder + "\\icon\\vent.png"))
                    {
                        vent = Image.FromStream(stream, false, false);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message + "\n" + e.StackTrace + "\n");
                    vent = Properties.Resources.vent;
                }

                icons = new Dictionary<Color, Image>();
                {

                    int colorNum = GameMemReader.ColorList.Length;
                    for (int i = 0; i < colorNum; i++)
                    {
                        Color color = Color.FromArgb(GameMemReader.ColorList[i].ToArgb());
                        string iconName = Program.exeFolder + "\\icon\\" + ((PlayerColor)i).ToString() + ".png";
                        try
                        {
                            using (FileStream stream = File.OpenRead(iconName))
                            {
                                icons.Add(color, Image.FromStream(stream, false, false));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Write(e.Message + "\n" + e.StackTrace + "\n");
                        }
                    }
                }
            }
            ~IconDict()
            {
                Dispose();
            }
            public void Dispose()
            {
                if (impostor != null)
                {
                    impostor.Dispose();
                    impostor = null;
                }
                if (vent != null)
                {
                    vent.Dispose();
                    vent = null;
                }
                if (icons != null)
                {
                    foreach(var icon in icons.Values)
                    {
                        icon?.Dispose();
                    }
                    icons.Clear();
                    icons = null;
                }
            }
        }

        static public void DrawMove_Icon(PaintEventArgs paint, PlayerMoveArgs move, List<int> deadOrderList, Map.MapScale map, IconDict icons, int width, int height)
        {
            string fontName = "Times New Roman";
            if (move == null) return;
            int AllImpostorNum = 0;

            float circleSize = height / 39.0f;
            int dsize = Math.Max(1, (int)circleSize / 5);
            using (var fnt = new Font(fontName, circleSize))
            {
                int minutes = move.time / 60000;
                int seconds = move.time / 1000 - minutes * 60;

                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 4.5f, circleSize * 18, circleSize * 1.2f);
                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 5.7f, circleSize * 8, circleSize * 1.2f);
                if (move.Sabotage.TaskType != TaskTypes.SubmitScan && move.state == GameState.TASKS)
                    using (var fnt2 = new Font(fontName, circleSize, FontStyle.Bold))
                        paint.Graphics.DrawString(move.Sabotage.TaskType.ToString(), fnt2, Brushes.Red, 0, circleSize * 4.5f);
                else
                    paint.Graphics.DrawString(move.state.ToString(), fnt, Brushes.Black, 0, circleSize * 4.5f);
                paint.Graphics.DrawString($"{minutes:00}:{seconds:00}", fnt, Brushes.Black, 0, circleSize * 5.7f);

                paint.Graphics.FillRectangle(Brushes.LightGray, 0, 0, circleSize * 25, circleSize * 4.5f);
                paint.Graphics.DrawString("KILLED\t          :", fnt, Brushes.Black, 0, circleSize * 0.0f);
                paint.Graphics.DrawString("EJECTED        :", fnt, Brushes.Black, 0, circleSize * 1.5f);
                paint.Graphics.DrawString("DISCONNECT :", fnt, Brushes.Black, 0, circleSize * 3.0f);
            }
            int killed = 0, ejjected = 0, disconnected = 0;



            using (var fnt = new Font(fontName, circleSize * 0.8f))
            {
                //set dead order
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    if (move.PlayerIsDead[i] != 0 && !deadOrderList.Contains(i)) deadOrderList.Add(i);
                    if (move.IsImpostor[i]) AllImpostorNum++;
                }

             
                //draw living crew
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    if (move.IsImpostor[i] || move.PlayerColors == null || move.PlayerIsDead[i] != 0) continue;

                    using (var brush = new SolidBrush(move.PlayerColors[i]))
                    {

                        int pointX = (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * width);
                        int pointY = (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * height);
                        
                        Image icon = icons.icons.GetValueOrDefault(move.PlayerColors[i]);
                        if (icon != null)
                        {
                            float icon_w = circleSize * icon.Width / icon.Height;
                            paint.Graphics.DrawImage(icon, pointX - icon_w, pointY - circleSize, icon_w * 2, circleSize * 2);
                            paint.Graphics.DrawString(move.PlayerNames[i], fnt, Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 2.0f);
                        }
                        else
                        {
                            paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                            paint.Graphics.DrawString(move.PlayerNames[i], fnt, Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);

                        }

                        paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                        paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                    }
                }


                //DrawImp
                for (int i = 0; i < AllImpostorNum; i++)
                {
                    int id = move.ImpostorId[i];
                    if (move.PlayerColors == null || move.PlayerIsDead[id] != 0) continue;


                    using (var brush = new SolidBrush(move.PlayerColors[id]))
                    using (Pen pen = new Pen(GetCColor(move.PlayerColors[id]), dsize))
                    {
                        int pointX = (int)(((move.PlayerPoses[id].X) * map.xs + map.xp) * width);
                        int pointY = (int)(((-move.PlayerPoses[id].Y) * map.ys + map.yp) * height);
                        Image icon = icons.icons.GetValueOrDefault(move.PlayerColors[id]);
                        if (move.InVent[i])
                        {
                            if (icon != null)
                            {
                                float icon_w = circleSize * icon.Width / icon.Height;
                                paint.Graphics.DrawImage(icons.vent, pointX - icon_w, pointY - circleSize, icon_w * 2, circleSize * 2);
                                paint.Graphics.DrawImage(icon, pointX - icon_w * 0.8f, pointY - circleSize * 0.6f, icon_w * 1.6f, circleSize * 1.6f);
                                paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 2.0f);
                            }
                            else
                            {
                                int s = (int)(circleSize * 0.8);
                                int cos = (int)(Math.Cos(Math.PI / 3) * s);
                                int sin = (int)(Math.Sin(Math.PI / 3) * s);
                                Point[] points = {
                                         new Point(pointX, pointY - s),
                                         new Point(pointX + sin, pointY + cos),
                                         new Point(pointX - sin, pointY + cos )
                                };
                                paint.Graphics.FillPolygon(brush, points);
                                paint.Graphics.DrawPolygon(pen, points);
                                paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            }
                        }
                        else
                        {
                            if (icon != null)
                            {
                                float icon_w = circleSize * icon.Width / icon.Height;
                                paint.Graphics.DrawImage(icons.impostor, pointX - icon_w, pointY - circleSize, icon_w * 2, circleSize * 2);
                                paint.Graphics.DrawImage(icon, pointX - icon_w, pointY - circleSize, icon_w * 2, circleSize * 2);
                                paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 2.0f);
                            }
                            else
                            {
                                paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                                paint.Graphics.DrawEllipse(pen, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                                paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            }
                        }
                        

                    }
                }

                //draw dead
                foreach (int i in deadOrderList)
                {
                    if (move.PlayerIsDead[i] != 0)
                    {
                        using (var brush = new SolidBrush(move.PlayerColors[i]))
                        {

                            int pointX = (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * width);
                            int pointY = (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * height);


                            if (move.PlayerIsDead[i] == -10)
                            {
                                pointX = (int)(circleSize * (2.0 * disconnected + 10.5));
                                pointY = (int)(circleSize * 3.7);
                                disconnected++;
                            }
                            else if (move.PlayerIsDead[i] == -11)
                            {
                                pointX = (int)(circleSize * (2.0 * ejjected + 10.5));
                                pointY = (int)(circleSize * 2.2);
                                ejjected++;
                            }
                            else if (move.PlayerIsDead[i] <= -20)
                            {
                                pointX = (int)(circleSize * (2.0 * killed + 10.5));
                                pointY = (int)(circleSize * 0.7);
                                killed++;
                            }


                            Point[] points = {  new Point(pointX - dsize * 2, pointY - dsize * 4),
                                            new Point(pointX, pointY - dsize * 2),
                                            new Point(pointX + dsize * 2, pointY - dsize * 4),
                                            new Point(pointX + dsize * 4, pointY - dsize * 2),
                                            new Point(pointX + dsize * 2, pointY),
                                            new Point(pointX + dsize * 4, pointY + dsize * 2),
                                            new Point(pointX + dsize * 2, pointY + dsize * 4),
                                            new Point(pointX, pointY + dsize * 2),
                                            new Point(pointX - dsize * 2, pointY + dsize * 4),
                                            new Point(pointX - dsize * 4, pointY + dsize * 2),
                                            new Point(pointX - dsize * 2, pointY),
                                            new Point(pointX - dsize * 4, pointY - dsize * 2),
                                            };
                            paint.Graphics.FillPolygon(brush, points);
                            if (Math.Abs(move.PlayerIsDead[i]) >= 20)
                            {
                                using (var pen = new Pen(move.PlayerColors[Math.Abs(move.PlayerIsDead[i]) - 20], dsize))
                                    paint.Graphics.DrawPolygon(pen, points);
                            }

                            if (move.PlayerIsDead[i] > 0)
                                paint.Graphics.DrawString(move.PlayerNames[i], fnt, move.IsImpostor[i] ? Brushes.Red : Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            else if (move.IsImpostor[i])
                                paint.Graphics.DrawString("imp", fnt, (move.PlayerColors[i].ToArgb() == Color.Red.ToArgb() || move.PlayerColors[i].ToArgb() == Color.HotPink.ToArgb()) ? Brushes.Black : Brushes.Red, pointX - circleSize * 1.0f, pointY - circleSize * 0.7f);
                            if (move.PlayerIsDead[i] == 11)
                                paint.Graphics.DrawString("ejected", fnt, move.PlayerColors[i].ToArgb() == Color.Black.ToArgb() ? Brushes.Red : Brushes.Black, pointX - circleSize * 2.0f, pointY - circleSize * 0.7f);

                            if (!move.IsImpostor[i])
                            {
                                paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                                paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                            }

                        }
                    }
                }

            }


        }

        static public void DrawMove(PaintEventArgs paint, PlayerMoveArgs move, List<int> deadOrderList, Map.MapScale map, int width, int height)
        {
            paint.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            string fontName = "Times New Roman";
            if (move == null) return;
            int AllImpostorNum = 0;

            float circleSize = height / 39.0f;
            int dsize = Math.Max(1, (int)circleSize / 5);
            using (var fnt = new Font(fontName, circleSize))
            {
                int minutes = move.time / 60000;
                int seconds = move.time / 1000 - minutes * 60;

                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 4.5f, circleSize * 18, circleSize * 1.2f);
                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 5.7f, circleSize * 8, circleSize * 1.2f);
                if (move.Sabotage.TaskType != TaskTypes.SubmitScan && move.state == GameState.TASKS)
                    using (var fnt2 = new Font(fontName, circleSize, FontStyle.Bold))
                        paint.Graphics.DrawString(move.Sabotage.TaskType.ToString(), fnt2, Brushes.Red, 0, circleSize * 4.5f);
                else
                    paint.Graphics.DrawString(move.state.ToString(), fnt, Brushes.Black, 0, circleSize * 4.5f);
                paint.Graphics.DrawString($"{minutes:00}:{seconds:00}", fnt, Brushes.Black, 0, circleSize * 5.7f);

                paint.Graphics.FillRectangle(Brushes.LightGray, 0, 0, circleSize * 25, circleSize * 4.5f);
                paint.Graphics.DrawString("KILLED\t          :", fnt, Brushes.Black, 0, circleSize * 0.0f);
                paint.Graphics.DrawString("EJECTED        :", fnt, Brushes.Black, 0, circleSize * 1.5f);
                paint.Graphics.DrawString("DISCONNECT :", fnt, Brushes.Black, 0, circleSize * 3.0f);
            }
            int killed = 0, ejjected = 0, disconnected = 0;



            using (var fnt = new Font(fontName, circleSize * 0.8f))
            {
                //set dead order
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    if (move.PlayerIsDead[i] != 0 && !deadOrderList.Contains(i)) deadOrderList.Add(i);
                }

                //draw dead
                foreach (int i in deadOrderList)
                {
                    if (move.PlayerIsDead[i] != 0)
                    {
                        using (var brush = new SolidBrush(move.PlayerColors[i]))
                        {

                            int pointX = (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * width);
                            int pointY = (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * height);


                            if (move.PlayerIsDead[i] == -10)
                            {
                                pointX = (int)(circleSize * (2.0 * disconnected + 10.5));
                                pointY = (int)(circleSize * 3.7);
                                disconnected++;
                            }
                            else if (move.PlayerIsDead[i] == -11)
                            {
                                pointX = (int)(circleSize * (2.0 * ejjected + 10.5));
                                pointY = (int)(circleSize * 2.2);
                                ejjected++;
                            }
                            else if (move.PlayerIsDead[i] <= -20)
                            {
                                pointX = (int)(circleSize * (2.0 * killed + 10.5));
                                pointY = (int)(circleSize * 0.7);
                                killed++;
                            }


                            Point[] points = {  new Point(pointX - dsize * 2, pointY - dsize * 4),
                                            new Point(pointX, pointY - dsize * 2),
                                            new Point(pointX + dsize * 2, pointY - dsize * 4),
                                            new Point(pointX + dsize * 4, pointY - dsize * 2),
                                            new Point(pointX + dsize * 2, pointY),
                                            new Point(pointX + dsize * 4, pointY + dsize * 2),
                                            new Point(pointX + dsize * 2, pointY + dsize * 4),
                                            new Point(pointX, pointY + dsize * 2),
                                            new Point(pointX - dsize * 2, pointY + dsize * 4),
                                            new Point(pointX - dsize * 4, pointY + dsize * 2),
                                            new Point(pointX - dsize * 2, pointY),
                                            new Point(pointX - dsize * 4, pointY - dsize * 2),
                                            };
                            paint.Graphics.FillPolygon(brush, points);
                            if (Math.Abs(move.PlayerIsDead[i]) >= 20)
                            {
                                using (var pen = new Pen(move.PlayerColors[Math.Abs(move.PlayerIsDead[i]) - 20], dsize))
                                    paint.Graphics.DrawPolygon(pen, points);
                            }

                            if (move.PlayerIsDead[i] > 0)
                                paint.Graphics.DrawString(move.PlayerNames[i], fnt, move.IsImpostor[i] ? Brushes.Red : Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            else if (move.IsImpostor[i])
                                paint.Graphics.DrawString("imp", fnt, (move.PlayerColors[i].ToArgb() == Color.Red.ToArgb() || move.PlayerColors[i].ToArgb() == Color.HotPink.ToArgb()) ? Brushes.Black : Brushes.Red, pointX - circleSize * 1.0f, pointY - circleSize * 0.7f);
                            if (move.PlayerIsDead[i] == 11)
                                paint.Graphics.DrawString("ejected", fnt, move.PlayerColors[i].ToArgb() == Color.Black.ToArgb() ? Brushes.Red : Brushes.Black, pointX - circleSize * 2.0f, pointY - circleSize * 0.7f);

                            if (!move.IsImpostor[i])
                            {
                                paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                                paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                            }

                        }
                    }
                }

                //draw living crew
                for (int i = 0; i < move.PlayerNum; i++) {
                    if (move.IsImpostor[i])
                    {
                        AllImpostorNum++;
                        continue;
                    }
                    if (move.PlayerColors == null || move.PlayerIsDead[i] != 0) continue;

                    using (var brush = new SolidBrush(move.PlayerColors[i]))
                    {

                        int pointX = (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * width);
                        int pointY = (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * height);


                        paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                        paint.Graphics.DrawString(move.PlayerNames[i], fnt, Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);

                        paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                        paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                    }
                }
         


                //DrawImp
                for (int i = 0; i < AllImpostorNum; i++)
                {
                    int id = move.ImpostorId[i];
                    if (move.PlayerColors == null || move.PlayerIsDead[id] != 0) continue;


                    using (var brush = new SolidBrush(move.PlayerColors[id]))
                    using (Pen pen = new Pen(GetCColor(move.PlayerColors[id]), dsize))
                    {
                        int pointX = (int)(((move.PlayerPoses[id].X) * map.xs + map.xp) * width);
                        int pointY = (int)(((-move.PlayerPoses[id].Y) * map.ys + map.yp) * height);

                        if (move.InVent[i])
                        {
                            int s = (int)(circleSize * 0.8);
                            int cos = (int)(Math.Cos(Math.PI / 3) * s);
                            int sin = (int)(Math.Sin(Math.PI / 3) * s);
                            Point[] points = {
                                         new Point(pointX, pointY - s),
                                         new Point(pointX + sin, pointY + cos),
                                         new Point(pointX - sin, pointY + cos )
                                };
                            paint.Graphics.FillPolygon(brush, points);
                            paint.Graphics.DrawPolygon(pen, points);

                        }
                        else
                        {
                            paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                            paint.Graphics.DrawEllipse(pen, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);

                        }
                        paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);

                    }
                }
            }


        }
        #endregion

        #region transparent window

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, Byte bAlpha, int dwFlags);

        static private int ToCOLORREF(Color c)
        {
            return c.B * 0x10000 + c.G * 0x100 + c.R;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams cp = base.CreateParams;

                cp.ExStyle = cp.ExStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT;
                if (this.FormBorderStyle != FormBorderStyle.None)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                }

                return cp;
            }
        }

        #endregion

        static private Color GetCColor(Color color)
        {
            byte r = (byte)~color.R;
            byte g = (byte)~color.G;
            byte b = (byte)~color.B;

            return Color.FromArgb(r, g, b);
        }

        #region Keyboard hook
        [DllImport("KeyboardHook32.dll", EntryPoint = "SetKeyboardHook")]
        static extern int SetKeyboardHook32(int threadId, IntPtr winhandle, IntPtr trackhandle);

        [DllImport("KeyboardHook32.dll", EntryPoint = "ResetKeyboardHook")]
        static extern int ResetKeyboardHook32();
        [DllImport("KeyboardHook32.dll", EntryPoint = "SetKeyboardEnable")]
        static extern int SetKeyboardEnable32(bool gPlaying, bool gEnable);

        [DllImport("KeyboardHook64.dll", EntryPoint = "SetKeyboardHook")]
        static extern int SetKeyboardHook64(int threadId, IntPtr winhandle, IntPtr trackhandle);

        [DllImport("KeyboardHook64.dll", EntryPoint = "ResetKeyboardHook")]
        static extern int ResetKeyboardHook64();
        [DllImport("KeyboardHook64.dll", EntryPoint = "SetKeyboardEnable")]
        static extern int SetKeyboardEnable64(bool gPlaying, bool gEnable);


        static int SetKeyboardHook(int threadId, IntPtr winhandle, IntPtr trackhandle)
        {
            if (Environment.Is64BitProcess)
                return SetKeyboardHook64(threadId, winhandle, trackhandle);
            else
                return SetKeyboardHook32(threadId, winhandle, trackhandle);
        }
        static int ResetKeyboardHook()
        {
            if (Environment.Is64BitProcess)
                return ResetKeyboardHook64();
            else
                return ResetKeyboardHook32();
        }
        static int SetKeyboardEnable(bool gPlaying, bool gEnable)
        {
            if (Environment.Is64BitProcess)
                return SetKeyboardEnable64(gPlaying, gEnable);
            else
                return SetKeyboardEnable32(gPlaying, gEnable);
        }



        #endregion

        #region window Native
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOWNA = 8;
        #endregion


        class AdjustForm1ToOwnerWindow : AdjustToOwnerWindow
        {
            OverlayWindow overlayform;
            public AdjustForm1ToOwnerWindow(OverlayWindow form, IntPtr hOwnerWnd) : base(form, hOwnerWnd)
            {
                this.overlayform = form;
                return;
            }

            override public void resize(object sender = null, EventArgs e = null)
            {
                if (!NativeMethods.IsWindow(hOwnerWnd))
                {
                    if (sender != null)
                        ((System.Windows.Forms.Timer)sender).Enabled = false;
                    Stop();
                    overlayform.Close();
                    return;
                }
                POINT pos = new POINT();
                NativeMethods.ClientToScreen(hOwnerWnd, out pos);
                RECT rect = new RECT();
                NativeMethods.GetClientRect(hOwnerWnd, out rect);


                float w = rect.right;
                float h = rect.bottom;
                float hw = Map.Maps[overlayform.mapId].hw;
                if (h / w > hw)
                {
                    h = w * hw;
                    overlayform.pictureBox1.Location = new Point(0, (int)((rect.bottom - h) * 0.5));
                }
                else
                {
                    w = h / hw;
                    overlayform.pictureBox1.Location = new Point((int)((rect.right - w) * 0.5), 0);
                }
                overlayform.pictureBox1.Size = new Size((int)w, (int)h);

                overlayform.Location = new Point(pos.X, pos.Y);
                overlayform.Size = new Size(rect.right, rect.bottom);

                overlayform.trackwin.Location = new Point(pos.X, pos.Y + rect.bottom - 30);
                overlayform.trackwin.Size = new Size(rect.right, 30);
            }

        }

        #region ResizeTest
        uint SWP_NOZORDER = 0x0004;
        uint SWP_NOMOVE = 0x0002;
        public bool setOwnerSize(int w, int h)
        {
            if (ownerHandle == IntPtr.Zero) return false;
            return SetWindowPos(ownerHandle, IntPtr.Zero, 0, 0, w, h, SWP_NOMOVE | SWP_NOZORDER);
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        #endregion
    }
}
