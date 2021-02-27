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
using AmongUsReplayInWindow.setOwnerWindow;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;

namespace AmongUsReplayInWindow
{
    public partial class StartWindow : Form
    {
        CancellationTokenSource tokenSource = null;
        Task createWindowTask = null;
        Task gameReaderTask = null;
        internal OverlayWindow overlayForm = null;
        delegate void void_stringDelegate(string str);
        delegate void void_ProcessDelegate(Process process);
        internal DrawMove.IconDict iconDict;
        internal Settings settings = null;
        string settingPath;
        bool closed = false;

        public StartWindow()
        {
            settingPath = Program.exeFolder + "\\setting.json";
            try
            {
                if (File.Exists(settingPath))
                    settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingPath));
            } catch(Exception e) { }
            if (settings == null) settings = new Settings();
            InitializeComponent();
            iconDict = new DrawMove.IconDict();
            applySettings();

        }

        ~StartWindow()
        {
            StartWindow_FormClosed(null, null);
        }

        private void StartWindow_FormClosed(object sender, FormClosedEventArgs ev)
        {
            closed = true;
            if (settings != null)
            {
                try
                {
                    using (StreamWriter sw = File.CreateText(settingPath))
                        sw.Write(JsonConvert.SerializeObject(settings));
                } catch(Exception e) { }
            }
            createWindowTask?.Wait();
            if (gameReaderTask != null && !gameReaderTask.IsCompleted)
            {
                try { tokenSource?.Cancel(); } catch (ObjectDisposedException e) { }
                gameReaderTask?.Wait(10000);
            }
            iconDict?.Dispose();
            try { tokenSource?.Dispose(); } catch (ObjectDisposedException e) { }

            createWindowTask = null;
            iconDict = null;
            tokenSource = null;
        }

        private void openFileDialogButton_Click(object sender, EventArgs ev)
        {
            if (filenameTextBox.Text != "")
            {
                openFileDialog1.InitialDirectory = filenameTextBox.Text;
                openFileDialog1.FileName = Path.GetFileName(filenameTextBox.Text);
            }
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

        void createOverlayWindow(Process process)
        {
            overlayForm = new OverlayWindow(this, tokenSource, process);
        }

        private void GetAmongUsWindow_Click(object sender, EventArgs ev)
        {
            if (!OverlayWindow.open)
            {
                OverlayWindow.open = true;
                try { tokenSource?.Cancel(); } catch (ObjectDisposedException e) { }
                try { gameReaderTask?.Wait(10000); }
                catch(TimeoutException time_e)
                {
                    Console.WriteLine(time_e.Message);
                    Console.WriteLine(time_e.StackTrace);
                    OverlayWindow.open = false;
                    try { tokenSource?.Dispose(); } catch (ObjectDisposedException e) { }
                    tokenSource = null;
                    return;
                }
                
                tokenSource = new CancellationTokenSource();
                var cancelToken = tokenSource.Token;
                GetAmongUsWindow.Text = "Looking for Among Us window";
                createWindowTask = Task.Factory.StartNew(() =>
                {
                    Process ownerProcess = getOwnerWindow.findWindow("Among Us");
                    if (ownerProcess == null)
                    {
                        OverlayWindow.open = false;
                        Invoke(new void_stringDelegate(ChangeGetAmongUsWindowButton), "Get Among Us Window");
                        return;
                    }
                    Invoke(new void_ProcessDelegate(createOverlayWindow), ownerProcess);
                    if (overlayForm.ownerHandle != IntPtr.Zero)
                    {
                        Invoke(new void_stringDelegate(ChangeGetAmongUsWindowButton), "Running...");
                        gameReaderTask = Task.Factory.StartNew(() =>
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
                            try { tokenSource?.Dispose(); } catch (ObjectDisposedException e) { }
                            tokenSource = null;
                            try
                            {
                                if (!closed)
                                    Invoke(new void_stringDelegate(ChangeGetAmongUsWindowButton), "Get Among Us Window");
                            }
                            catch (ObjectDisposedException e) { }
                        });
                    }
                });

            }
        }

        void ChangeGetAmongUsWindowButton(string text)
        {
            GetAmongUsWindow.Text = text;
        }

        #region setting
        internal bool drawIcon = true;
        internal int interval = 50;
        internal int step = 1;
        private void replaySpeedTrackBar_Scroll(object sender, EventArgs ev)
        {
            if (sender != null)
                settings.speed = replaySpeedTrackBar.Value;
            interval = 50 + Math.Max(0, -settings.speed) * 25;
            step = 1 + Math.Max(0, settings.speed);
            if (settings.speed == replaySpeedTrackBar.Minimum) step = 0;
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

      
       

        public enum PlayerIconRendering
        {
            Icon,
            Simple
        }
        

        private void mapAlphaUpdown_ValueChanged(object sender, EventArgs e)
        {
            if (sender != null)
                settings.mapAlpha = (int)mapAlphaUpdown.Value;
            overlayForm?.setAlpha(settings.mapAlpha);
        }

        [JsonObject]
        public class Settings
        {
            [DefaultValue(0)]
            public int speed = 0;

            [DefaultValue(230)]
            public int mapAlpha = 230;

            [DefaultValue(PlayerIconRendering.Icon)]
            public PlayerIconRendering playerIcon = PlayerIconRendering.Icon;

            [DefaultValue("color")]
            public string MapImageFolder = "color";

            [DefaultValue(true)]
            public bool PlayerNameVisible = true;

            [DefaultValue(true)]
            public bool TaskBarVisible = true;

            [DefaultValue(1.0f)]
            public float PlayerSize = 1.0f;
        }

        void applySettings()
        {
            replaySpeedTrackBar.Value = settings.speed;
            mapAlphaUpdown.Value = settings.mapAlpha;

            replaySpeedTrackBar_Scroll(null, null);
            mapAlphaUpdown_ValueChanged(null, null);

            drawIcon = settings.playerIcon == StartWindow.PlayerIconRendering.Icon;
            if (overlayForm != null)
            {
                overlayForm.drawIcon = drawIcon;
            }
            foreach (var formF in fromFile.fromFileList)
            {
                formF.drawIcon = drawIcon;
            }

            DrawMove.TaskBarVisible = settings.TaskBarVisible;
            DrawMove.PlayerNameVisible = settings.PlayerNameVisible;

            Map.mapFolder = settings.MapImageFolder;

            DrawMove.playerSize = settings.PlayerSize;

        }

        #endregion


        private void SettingButton_Click(object sender, EventArgs e)
        {
            SettingDialog settingDialog = new SettingDialog(this);
            settingDialog.ShowDialog();
        }
    }
}
