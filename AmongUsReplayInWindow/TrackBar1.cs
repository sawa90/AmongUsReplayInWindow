using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AmongUsReplayInWindow.setOwnerWindow;

namespace AmongUsReplayInWindow
{
    public partial class TrackBarWin : Form
    {

        public TrackBarWin(OverlayWindow OwnerForm)
        {
            ShowInTaskbar = false;
            InitializeComponent();
            NativeMethods.SetLayeredWindowAttributes(this.Handle, ToCOLORREF(Color.Snow), 0, ULW_COLORKEY);
            this.OwnerForm = OwnerForm;
            this.Show();
            this.Size = new Size(0, 0);
            step = OwnerForm.startWindow.step;
            KeyEventHandler = new KeyEventHandler(trackBar_KeyDown);
            trackBar0.KeyDown += KeyEventHandler;
            Visible = false;
        }

        ~TrackBarWin()
        {
            timer?.Stop();
            timer?.Dispose();
        }

        OverlayWindow OwnerForm;

        #region Handler
        System.EventHandler scrollEventHandler = null;
        KeyEventHandler KeyEventHandler = null;
        internal Timer timer;

        public void setHandler()
        {
            lock (OwnerForm.lockObject)
            {
                NativeMethods.ShowWindow(Handle, NativeMethods.SW_SHOWNA);
                trackBar0.Value = 0;
                trackBar0.Maximum = (int)OwnerForm.logReader.maxMoveNum;

                if (scrollEventHandler != null) trackBar0.Scroll -= scrollEventHandler;
                scrollEventHandler = new EventHandler(trackBar_Scroll);
                trackBar0.Scroll += scrollEventHandler;

                timer = new System.Windows.Forms.Timer();
                timer.Interval = OwnerForm.startWindow.interval;
                timer.Tick += new EventHandler(Update);
                timer.Start();
                setFocus();
            }
        }


        public void deleteHandler()
        {
            lock (OwnerForm.lockObject)
            {
                if (scrollEventHandler != null) trackBar0.Scroll -= scrollEventHandler;
                scrollEventHandler = null;

                timer?.Stop();
                timer?.Dispose();
                timer = null;
                giveFocus();
                Visible = false;
            }
        }




        private void trackBar_Scroll(object senderl, EventArgs ev)
        {
            lock (OwnerForm.lockObject)
            {
                if (OwnerForm.logReader?.reader == null) return;
                OwnerForm.logReader?.seek(trackBar0.Value);
                OwnerForm.moveArg = OwnerForm.logReader.ReadFrombFileMove();
            }

        }

        private void trackBar_KeyDown(object sender, KeyEventArgs e)
        {
            bool skipflag = false;
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                int[] oldij = new int[2] { 0, 0 };
                if (trackBar0.Value == trackBar0.Maximum && e.KeyCode == Keys.Down) trackBar0.Value = 0;
                else {
                    foreach (int[] ij in OwnerForm.discFrames)
                    {
                        if (trackBar0.Value <= ij[0] + 3)
                        {
                            if (e.KeyCode == Keys.Down) trackBar0.Value = ij[0];
                            else trackBar0.Value = oldij[1];
                            skipflag = true;
                            break;
                        }
                        else if (trackBar0.Value <= ij[1] + 3)
                        {
                            if (e.KeyCode == Keys.Up) trackBar0.Value = ij[0];
                            else trackBar0.Value = ij[1];
                            skipflag = true;
                            break;
                        }
                        oldij = ij;
                    }
                    if (!skipflag)
                    {
                        if (e.KeyCode == Keys.Up) trackBar0.Value = oldij[1];
                        else trackBar0.Value = trackBar0.Maximum;
                    }
                }
                
                trackBar_Scroll(null, null);
                e.Handled = true;
            } else if(e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey)
            {
                giveFocus();
                OwnerForm.Visible = false;
                Visible = false;
            }

        }

        internal int step = 1;
        private void Update(object sender, EventArgs ev)
        {
            if (OwnerForm.logReader?.reader == null) return;
            if ((Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left || !trackBar0.Focused)
            {
                int value = trackBar0.Value + step;
                if (value < trackBar0.Maximum)
                    trackBar0.Value = value;
                else trackBar0.Value = trackBar0.Maximum;
            }
            trackBar_Scroll(sender, ev);
        }

        #endregion

        #region Focus
        public void setFocus()
        {
            if (Visible && NativeMethods.GetForegroundWindow() == OwnerForm.ownerHandle) trackBar0.Focus();
        }

        public void giveFocus()
        {
            IntPtr hfgwnd = NativeMethods.GetForegroundWindow();
            if (hfgwnd == Handle || hfgwnd == trackBar0.Handle)
                if (NativeMethods.IsWindow(OwnerForm.ownerHandle))
                    Microsoft.VisualBasic.Interaction.AppActivate(OwnerForm.ownerProcessId);
        }
        #endregion

        #region Transparent Window 

        static private int ToCOLORREF(Color c)
        {
            return c.B * 0x10000 + c.G * 0x100 + c.R;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams cp = base.CreateParams;

                cp.ExStyle = cp.ExStyle | WS_EX_LAYERED ;
                if (this.FormBorderStyle != FormBorderStyle.None)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                }

                return cp;
            }
        }

        public const int ULW_COLORKEY = 1;
        public const int ULW_ALPHA = 2;
        public const int WS_EX_LAYERED = 0x00080000;
        #endregion

        class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, Byte bAlpha, int dwFlags);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern bool IsWindow(IntPtr hWnd);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

            public const int SW_HIDE = 0;
            public const int SW_SHOWNA = 8;
        }


    }
}
