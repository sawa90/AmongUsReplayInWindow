using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AmongUsCapture;
using System.IO;

namespace AmongUsReplayInWindow
{
    public partial class fromFile : Form
    {
        #region variable
        public static List<fromFile> fromFileList = new List<fromFile>(); 
        public static bool open = false;

        public object lockObject = new object();

        public MoveLogFile.ReadMoveLogFile logReader = null;


        PaintEventHandler drawTrackBar = null;
        PaintEventHandler backgroundPaint = null;

        int mapId = 0;
        Image MapImage;
        float hw = 1;


        public PlayerMoveArgs e = null;
        string filename;

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
        ConfigWindow configWindow;
        internal int step = 1;
        internal bool drawIcon;

        #endregion

        #region Initialize

        public fromFile(ConfigWindow configWindow, string filename)
        {
            this.configWindow = configWindow;
            Init();
            this.filename = filename;
            if (!setReader(filename))
            {
                Close();
                return;
            }
            setTrackBarHandler();
            fromFileList.Add(this);
        }

        ~fromFile()
        {
            Form1_FormClosing(null, null);
        }


        private void Init()
        {
            open = true;
            InitializeComponent();
            pictureBox1.Width = ClientSize.Width;
            pictureBox1.Height = ClientSize.Height;
            pictureBox1.Paint += new PaintEventHandler(Draw);
            FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            backgroundPaint = new PaintEventHandler(DrawBackground);
            Paint += backgroundPaint;
            SizeChanged += new EventHandler(SizeChangedHandler);
            step = configWindow.step;
            drawIcon = configWindow.drawIcon;
        }
        #endregion


        private void Form1_FormClosing(Object sender, FormClosingEventArgs ev)
        {
            fromFileList.Remove(this);
            MapImage?.Dispose();
            logReader?.Close();
            deleteTrackBarHandler();
            MapImage = null;
            logReader = null;

            open = false;
        }

        private void SizeChangedHandler(Object sender, EventArgs ev)
        {
            float w = (float)ClientSize.Width;
            float h = (float)ClientSize.Height - 40;
            
            if (h / w > hw)
            {
                h = w * hw;
                pictureBox1.Location = new Point(0, (int)((ClientSize.Height - 40 - h) * 0.5));
            }
            else
            {
                w = h / hw;
                pictureBox1.Location = new Point((int)(((float)ClientSize.Width - w) * 0.5), 0);
            }
            pictureBox1.Size = new Size((int)w, (int)h);
            Invalidate();
            pictureBox2.Invalidate();
        }

        #region set reader
        public bool setReader(string filename)
        {
            lock (lockObject)
            {
                Console.WriteLine($"Read {filename}...");
                logReader = new MoveLogFile.ReadMoveLogFile(filename);
                if (logReader?.reader == null)
                {
                    Console.WriteLine($"Can not read {filename}");
                    logReader?.Close();
                    logReader = null;
                    return false;
                }
                mapId = (int)logReader.startArgs.PlayMap;
                MapImage = Map.setMapImage(mapId);
                hw = Map.Maps[mapId].hw;
                SizeChangedHandler(null, null);

                Invalidate();
                getFrameData();
            }
            drawTrackBar = new PaintEventHandler(DrawBar);
            pictureBox2.Paint += drawTrackBar;
            pictureBox2.Invalidate();
            using (var g = CreateGraphics())
                if (MapImage != null)
                {
                    g.FillRectangle(Brushes.Snow, pictureBox1.Location.X, pictureBox1.Location.Y, pictureBox1.Size.Width, pictureBox1.Size.Height);
                    g.DrawImage(MapImage, pictureBox1.Location.X, pictureBox1.Location.Y, pictureBox1.Size.Width, pictureBox1.Size.Height);
                }
            return true;
        }


        private void removeReader()
        {
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
                e = logReader.ReadFrombFileMove();
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
                        deadOrderList.Add(j);
                    }
                    oldPlayerIsDead[j] = e.PlayerIsDead[j];
                }
            }
            if (e.state == GameState.DISCUSSION)
                discFrames.Add(new int[2] { discFrame, (int)logReader.maxMoveNum });
            logReader.reader.BaseStream.Position = readerPos;
        }

        #endregion

        System.EventHandler scrollEventHandler = null;
        KeyEventHandler KeyEventHandler = null;
        internal Timer timer;

        public void setTrackBarHandler()
        {
            lock (lockObject)
            {
                trackBar1.Value = 0;
                trackBar1.Maximum = (int)logReader.maxMoveNum;

                if (scrollEventHandler != null) trackBar1.Scroll -= scrollEventHandler;
                scrollEventHandler = new EventHandler(trackBar_Scroll);
                trackBar1.Scroll += scrollEventHandler;

                if (KeyEventHandler != null) KeyDown -= KeyEventHandler;
                KeyEventHandler = new KeyEventHandler(trackBar_KeyDown);
                trackBar1.KeyDown += KeyEventHandler;
                timer = new System.Windows.Forms.Timer();
                timer.Interval = configWindow.interval;
                timer.Tick += new EventHandler(Update);
                timer.Start();
            }
        }


        public void deleteTrackBarHandler()
        {
            lock (lockObject)
            {
                if (scrollEventHandler != null) trackBar1.Scroll -= scrollEventHandler;
                scrollEventHandler = null;
                if (KeyEventHandler != null)
                {
                    trackBar1.KeyDown -= KeyEventHandler;
                }
                KeyEventHandler = null;
                timer?.Stop();
                timer?.Dispose();
                timer = null;
            }
        }




        private void trackBar_Scroll(object senderl, EventArgs ev)
        {
            lock (lockObject)
            {
                if (logReader?.reader == null) return;
                logReader?.seek(trackBar1.Value);
                e = logReader.ReadFrombFileMove();
            }
            pictureBox1.Invalidate();

        }

        private void trackBar_KeyDown(object sender, KeyEventArgs e)
        {
            bool skipflag = false;
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                if (trackBar1.Value == trackBar1.Maximum && e.KeyCode == Keys.Down) trackBar1.Value = 0;
                else {
                    int[] oldij = new int[2] { 0, 0 };
                    foreach (int[] ij in discFrames)
                    {
                        if (trackBar1.Value <= ij[0] + 3)
                        {
                            if (e.KeyCode == Keys.Down) trackBar1.Value = ij[0];
                            else trackBar1.Value = oldij[1];
                            skipflag = true;
                            break;
                        }
                        else if (trackBar1.Value <= ij[1] + 3)
                        {
                            if (e.KeyCode == Keys.Up) trackBar1.Value = ij[0];
                            else trackBar1.Value = ij[1];
                            skipflag = true;
                            break;
                        }
                        oldij = ij;
                    }
                    if (!skipflag)
                    {
                        if (e.KeyCode == Keys.Up) trackBar1.Value = oldij[1];
                        else trackBar1.Value = trackBar1.Maximum;
                    }
                }

                trackBar_Scroll(null, null);
                e.Handled = true;
            }

        }


        private void Update(object sender, EventArgs ev)
        {
            if (logReader?.reader == null) return;
            if (((Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left) || !trackBar1.Focused)
            {
                int value = trackBar1.Value + step;
                if (value < trackBar1.Maximum)
                    trackBar1.Value = value;
                else trackBar1.Value = trackBar1.Maximum;
            }
            trackBar_Scroll(sender, ev);
        }


        #region Draw
        private void DrawBackground(object sender, System.Windows.Forms.PaintEventArgs paint)
        {
            if (MapImage != null)
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
            float dsize = Math.Max(1.0f, circleSize / 5.0f) ;
            foreach (var ij in deadList)
            {
                using (var brush = new SolidBrush(e.PlayerColors[ij[1]]))
                    g.FillRectangle(brush, wPerFrame * ij[0] - dsize, 0, dsize * 2, pictureBox2.ClientSize.Height);
            }
        }

        private void Draw(object sender, PaintEventArgs paint)
        {
            lock (lockObject)
            {
                if (drawIcon && configWindow?.iconDict != null)
                    DrawMove.DrawMove_Icon(paint, e, deadOrderList, Map.Maps[mapId], configWindow.iconDict, pictureBox1.Width, pictureBox1.Height);
                else
                    DrawMove.DrawMove_Simple(paint, e, deadOrderList, Map.Maps[mapId], pictureBox1.Width, pictureBox1.Height);
            }

        }

        #endregion
       
    }
}
