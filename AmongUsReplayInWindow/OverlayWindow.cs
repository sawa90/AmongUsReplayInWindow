using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AmongUsCapture;
using System.Runtime.InteropServices;
using AmongUsReplayInWindow.setOwnerWindow;

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

        internal System.Windows.Forms.Timer drawTimer = new System.Windows.Forms.Timer();

        Map.backgroundMap backgroundMap;
        public int mapId = 0;

        public Point mapLocation;
        public Size mapSize;

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
                SetLayeredWindowAttributes(this.Handle, ToCOLORREF(Color.Snow), 230, ULW_COLORKEY | ULW_ALPHA);

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
            mapSize = ClientSize;
            mapLocation = Point.Empty;
            Paint += new PaintEventHandler(Draw);
            FormClosing += new FormClosingEventHandler(overlay_FormClosing);
            trackwin = new TrackBarWin(this);
            backgroundMap = new Map.backgroundMap(ClientSize, mapLocation, mapSize, mapId);
            drawTimer = new System.Windows.Forms.Timer();
            drawTimer.Interval = configWindow.interval;
            drawTimer.Tick += new EventHandler(DrawTimerHandler);
            Visible = false;
            drawIcon = configWindow.drawIcon;
        }

  
        #endregion

        private void overlay_FormClosing(Object sender, FormClosingEventArgs ev)
        {
            ResetKeyboardHook();
            cancelTokenSource?.Cancel();
            removeReader();
            drawTimer?.Dispose();
            writer?.Close();
            backgroundMap?.Dispose();
            sizeChange?.Stop();
            trackwin?.Close();

            drawTimer = null;
            writer = null;
            backgroundMap = null;
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
                logReader.seek(0);
                moveArg = logReader.ReadFrombFileMove();
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
            moveArg = logReader.ReadFrombFileMove();

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
                g.FillRectangle(Brushes.Snow, 0, 0, Width, Height);

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
            Invalidate();
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
                backgroundMap?.Draw(g);
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

            float circleSize = mapSize.Height / 39.0f;
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
            sizeChange.resize();
            backgroundMap.ChangeMapId(mapId, ClientSize, mapLocation, mapSize);
            Invalidate();
        }
        private void Draw(object sender, PaintEventArgs paint)
        {
            if (!Playing)
                backgroundMap?.Draw(paint.Graphics);
            lock (lockObject)
            {
                if (drawIcon && configWindow?.iconDict != null) 
                {
                    DrawMove.DrawMove_Icon(paint, moveArg, deadOrderList, Map.Maps[mapId], configWindow.iconDict, mapLocation, mapSize);

                }
                else
                    DrawMove.DrawMove_Simple(paint, moveArg, deadOrderList, Map.Maps[mapId], mapLocation, mapSize);
            }

        }

        #endregion

        #region transparent window

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, Byte bAlpha, int dwFlags);

        internal void setAlpha(int a)
        {
            Invoke(new void_intDelegate(setAlphaDelegate), a);
        }

        void setAlphaDelegate(int a)
        {
            SetLayeredWindowAttributes(this.Handle, ToCOLORREF(Color.Snow), (byte)a, ULW_COLORKEY | ULW_ALPHA);
        }

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

        #region Keyboard hook
        [DllImport("KeyboardHook32.dll", EntryPoint = "SetKeyboardHook")]
        static extern bool SetKeyboardHook32(int threadId, IntPtr winhandle, IntPtr trackhandle);

        [DllImport("KeyboardHook32.dll", EntryPoint = "ResetKeyboardHook")]
        static extern bool ResetKeyboardHook32();
        [DllImport("KeyboardHook32.dll", EntryPoint = "SetKeyboardEnable")]
        static extern void SetKeyboardEnable32(bool gPlaying, bool gEnable);

        [DllImport("KeyboardHook64.dll", EntryPoint = "SetKeyboardHook")]
        static extern bool SetKeyboardHook64(int threadId, IntPtr winhandle, IntPtr trackhandle);

        [DllImport("KeyboardHook64.dll", EntryPoint = "ResetKeyboardHook")]
        static extern bool ResetKeyboardHook64();
        [DllImport("KeyboardHook64.dll", EntryPoint = "SetKeyboardEnable")]
        static extern void SetKeyboardEnable64(bool gPlaying, bool gEnable);


        static bool SetKeyboardHook(int threadId, IntPtr winhandle, IntPtr trackhandle)
        {
            if (Environment.Is64BitProcess)
                return SetKeyboardHook64(threadId, winhandle, trackhandle);
            else
                return SetKeyboardHook32(threadId, winhandle, trackhandle);
        }
        static bool ResetKeyboardHook()
        {
            if (Environment.Is64BitProcess)
                return ResetKeyboardHook64();
            else
                return ResetKeyboardHook32();
        }
        static void SetKeyboardEnable(bool gPlaying, bool gEnable)
        {
            if (Environment.Is64BitProcess)
                SetKeyboardEnable64(gPlaying, gEnable);
            else
                SetKeyboardEnable32(gPlaying, gEnable);
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

                Point formLocation = overlayform.Location;
                if (formLocation.X != pos.X || formLocation.Y != pos.Y)
                {
                    overlayform.Location = new Point(pos.X, pos.Y);
                    overlayform.trackwin.Location = new Point(pos.X, pos.Y + rect.bottom - 30);
                }

                Size formSize = overlayform.Size;
                if (rect.right != formSize.Width || rect.bottom != formSize.Height)
                {
                    float w = rect.right;
                    float h = rect.bottom;
                    float hw = Map.Maps[overlayform.mapId].hw;
                    if (h / w > hw)
                    {
                        h = w * hw;
                        overlayform.mapLocation = new Point(0, (int)((rect.bottom - h) * 0.5));
                    }
                    else
                    {
                        w = h / hw;
                        overlayform.mapLocation = new Point((int)((rect.right - w) * 0.5), 0);
                    }
                    overlayform.mapSize = new Size((int)w, (int)h);
                    overlayform.Size = new Size(rect.right, rect.bottom);
                    overlayform.backgroundMap.ChangeSize(overlayform.Size, overlayform.mapLocation, overlayform.mapSize);
                    overlayform.trackwin.Size = new Size(rect.right, 30);
                }
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
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        #endregion
    }
}
