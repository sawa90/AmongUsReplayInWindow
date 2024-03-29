﻿using System;
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

        int mapId = 0;
        Map.backgroundMap backgroundMap;
        float hw = 1;
        public Point mapLocation;
        public Size mapSize;


        public PlayerMoveArgs e = null;
        string filename;

        public List<int[]> discFrames = new List<int[]>();
        List<int[]> deadList = new List<int[]>();
        List<DrawMove.DeadPos> deadOrderList = new List<DrawMove.DeadPos>();


        public const int ULW_COLORKEY = 1;
        public const int ULW_ALPHA = 2;
        public const int WS_THICKFRAME = 0x00040000;
        public const int WS_BORDER = 0x00800000;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TOPMOST = 0x00000008;
        StartWindow startWindow;
        internal int step = 1;
        int version = 0;
        #endregion

        #region Initialize

        public fromFile(StartWindow startWindow, string filename)
        {
            this.startWindow = startWindow;
            Init();
            this.filename = filename;
            if (!setReader(filename))
            {
                Close();
                return;
            }
            setTrackBarHandler();
            trackBar1.BackColor = DrawMove.backgroundColor;
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
            mapSize = ClientSize;
            mapLocation = Point.Empty;
            Paint += new PaintEventHandler(Draw);
            FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            SizeChanged += new EventHandler(SizeChangedHandler);
            step = startWindow.step;
        }
        #endregion


        private void Form1_FormClosing(Object sender, FormClosingEventArgs ev)
        {
            fromFileList.Remove(this);
            backgroundMap?.Dispose();
            logReader?.Close();
            deleteTrackBarHandler();
            backgroundMap = null;
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
                mapLocation = new Point(0, (int)((ClientSize.Height - 40 - h) * 0.5));
            }
            else
            {
                w = h / hw;
                mapLocation = new Point((int)(((float)ClientSize.Width - w) * 0.5), 0);
            }
            mapSize = new Size((int)w, (int)h);
            backgroundMap?.ChangeSize(ClientSize, mapLocation, mapSize);
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
                version = logReader.version;
                mapId = (int)logReader.startArgs.PlayMap;
                hw = Map.Maps[mapId].hw;
                SizeChangedHandler(null, null);
                backgroundMap = new Map.backgroundMap(ClientSize, mapLocation, mapSize, mapId,false);
                Invalidate();
                discFrames = logReader.discFrames;
                deadList = logReader.deadList;
                deadOrderList = logReader.deadOrderList;
                e = logReader.e;
            }
            drawTrackBar = new PaintEventHandler(DrawBar);
            pictureBox2.Paint += drawTrackBar;
            pictureBox2.Invalidate();
            if (backgroundMap != null)
            {
                using (var g = CreateGraphics())
                    backgroundMap.Draw(g);
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
                timer.Interval = startWindow.interval;
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
            Invalidate();

        }
        bool pauseflag = false;
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
            if (e.KeyCode == Keys.Space) pauseflag = !pauseflag;
        }


        private void Update(object sender, EventArgs ev)
        {
            if (logReader?.reader == null) return;
            if ((((Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left) || !trackBar1.Focused) && !pauseflag)
            {
                int value = trackBar1.Value + step;
                if (value < trackBar1.Maximum)
                    trackBar1.Value = value;
                else trackBar1.Value = trackBar1.Maximum;
            }
            trackBar_Scroll(sender, ev);
        }


        #region Draw
        public void changeColor()
        {
            backgroundMap.RedrawMap();
            trackBar1.BackColor = DrawMove.backgroundColor;
        }
        private void DrawBar(object sender, System.Windows.Forms.PaintEventArgs paint)
        {
            if (logReader?.reader == null) return;
            Graphics g = paint.Graphics;
            float wPerFrame = (float)pictureBox2.ClientSize.Width / logReader.maxMoveNum;

            g.FillRectangle(Brushes.LightGray, 0, 0, pictureBox2.ClientSize.Width, pictureBox2.ClientSize.Height);
            using (var brush = new SolidBrush(DrawMove.DiscussionColor))
                foreach (var ij in discFrames)
                {
                    g.FillRectangle(brush, wPerFrame * ij[0], 0, wPerFrame * (ij[1] - ij[0]), pictureBox2.ClientSize.Height);
                }

            float circleSize = mapSize.Height / 39.0f;
            float dsize = Math.Max(1.0f, circleSize / 5.0f) ;
            foreach (var ij in deadList)
            {
                using (var brush = new SolidBrush(e.PlayerColors[ij[1]]))
                    g.FillRectangle(brush, wPerFrame * ij[0] - dsize, 0, dsize * 2, pictureBox2.ClientSize.Height);
            }
        }

        private void Draw(object sender, PaintEventArgs paint)
        {
            if (Width <= 0 || Height <= 0) return;
            backgroundMap?.Draw(paint.Graphics);
            lock (lockObject)
            {
                DrawMove.DrawMove_Icon(paint, e, deadOrderList, Map.Maps[mapId], startWindow.iconDict, mapLocation, mapSize, version);

            }
            
        }

        #endregion
       
    }
}
