using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using AmongUsCapture;

namespace AmongUsReplayInWindow
{
    public partial class ConfigWindow : Form
    {
        CancellationTokenSource tokenSource = null;
        Task gameReaderTask = null;
        OverlayWindow overlayForm = null;
        delegate void void_stringDelegate(string str);
        internal OverlayWindow.IconDict iconDict;

        public ConfigWindow()
        {
            InitializeComponent();
            iconDict = new OverlayWindow.IconDict();
            RenderingBox.SelectedIndex = 0;
        }


        ~ConfigWindow()
        {
            if (!gameReaderTask.IsCompleted)
            {
                tokenSource?.Cancel();
                gameReaderTask?.Wait(10000);
            }
            tokenSource?.Dispose();
            iconDict?.Dispose();
        }

        private void openFileDialogButton_Click(object sender, EventArgs ev)
        {
            if (filenameTextBox.Text != "")
                openFileDialog1.InitialDirectory = filenameTextBox.Text;
            else
                openFileDialog1.InitialDirectory = Program.exeFolder + "\\replay";
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                filenameTextBox.Text = openFileDialog1.FileName;
                try
                {
                    var form2 = new fromFile(this, filenameTextBox.Text);
                    form2.Show();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private void GetAmongUsWindow_Click(object sender, EventArgs ev)
        {
            if (!OverlayWindow.open)
            {
                OverlayWindow.open = true;
                tokenSource?.Cancel();  
                try { gameReaderTask?.Wait(10000); }
                catch(TimeoutException time_e)
                {
                    Console.WriteLine(time_e.Message);
                    OverlayWindow.open = false;
                    tokenSource?.Dispose();
                    tokenSource = null;
                    return;
                }
                
                tokenSource = new CancellationTokenSource();
                var cancelToken = tokenSource.Token;
                overlayForm = new OverlayWindow(this, tokenSource);
                if (overlayForm.ownerHandle != IntPtr.Zero)
                {
                    GetAmongUsWindow.Text = "Running...";
                    var gameReaderTask = Task.Factory.StartNew(() =>
                    {
                        var gameReader = Task.Factory.StartNew(() =>
                        {
                            GameMemReader.getInstance().PlayerMove += overlayForm.PlayerPosHandler;
                            GameMemReader.getInstance().GameStateChanged += overlayForm.GameStateChangedEventHandler;
                            GameMemReader.getInstance().GameStart += overlayForm.GameStartHandler;
                            GameMemReader.getInstance().RunLoop(cancelToken);
                        }, cancelToken); // run loop in background
                            
                        try { gameReader.Wait(); }
                        catch (System.AggregateException exc)
                        {
                            exc.Handle((e) =>
                            {
                                if (e.GetType() != typeof(TaskCanceledException))
                                {
                                    Console.WriteLine(e.Message);
                                    return false;
                                }
                                return true;
                            });
                        }
                    }).ContinueWith(t =>
                    {
                        tokenSource?.Dispose();
                        tokenSource = null;
                        Invoke(new void_stringDelegate(ChangeGetAmongUsWindowButton), "Get Among Us Window"); 
                    });
                }

            }
        }

        void ChangeGetAmongUsWindowButton(string text)
        {
            GetAmongUsWindow.Text = text;
        }

        internal int speed = 0;
        internal int interval = 50;
        internal int step = 1;
        private void replaySpeedTrackBar_Scroll(object sender, EventArgs ev)
        {
            speed = replaySpeedTrackBar.Value;
            interval = 50 + Math.Max(0, -speed) * 25;
            step = 1 + Math.Max(0, speed);
            if (speed == replaySpeedTrackBar.Minimum) step = 0;
            if (overlayForm?.drawTimer != null)
            {
                overlayForm.drawTimer.Interval = interval;
                if (overlayForm?.trackwin?.timer?.Interval != null)
                {
                    overlayForm.trackwin.timer.Interval = interval;
                    overlayForm.trackwin.step = step;
                }
                
            }

            foreach (var formF in fromFile.fromFileList)
            {
                if (formF.timer?.Interval != null)
                    formF.timer.Interval = interval;
                formF.step = step;
            }
        }

        #region Rendering
        internal Rendering rendering = Rendering.Icon;
        internal bool drawIcon = true;
        private void RenderingBox_SelectedIndexChanged(object sender, EventArgs ev)
        {
            rendering = (Rendering)RenderingBox.SelectedIndex;
            drawIcon = rendering == Rendering.Icon;
            if (overlayForm != null)
            {
                overlayForm.drawIcon = drawIcon;
            }

            foreach (var formF in fromFile.fromFileList)
            {
                formF.drawIcon = drawIcon;
            }
        }

        public enum Rendering
        {
            Icon,
            Simple
        }
        #endregion

    }
}
