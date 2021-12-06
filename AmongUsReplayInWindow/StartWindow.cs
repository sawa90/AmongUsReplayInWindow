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
using System.Runtime.InteropServices;

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
            StartWindow_FormClosing(null, null);
        }

        private void StartWindow_FormClosing(object sender, FormClosingEventArgs ev)
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
            if(OverlayWindow.open) overlayForm?.Close();
            try { tokenSource?.Cancel(); } catch (ObjectDisposedException e) { }
            try { createWindowTask?.Wait(); }
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
            if (gameReaderTask != null && !gameReaderTask.IsCompleted)
            {
                gameReaderTask?.Wait(10000);
            }
            iconDict?.Dispose();
            try { tokenSource?.Dispose(); } catch (ObjectDisposedException e) { }

            createWindowTask = null;
            iconDict = null;
            tokenSource = null;

            for(int i = MoveLogFile.writeMoves.Count - 1; i >= 0;i--)
            {
                try
                {
                    MoveLogFile.writeMoves[i]?.UnexpectedClose();
                }catch(ObjectDisposedException e) { }
            }
        }

        private void openFileDialogButton_Click(object sender, EventArgs ev)
        {
            if (filenameTextBox.Text != "")
            {
                openFileDialog1.InitialDirectory = filenameTextBox.Text;
                openFileDialog1.FileName = Path.GetFileName(filenameTextBox.Text);
            }
            else
            {
                openFileDialog1.InitialDirectory = Program.exeFolder + "\\replay";
                openFileDialog1.FileName = "";
            }
            openFileDialog1.Filter = "Data Files (*.dat)|*.dat";
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                filenameTextBox.Text = openFileDialog1.FileName;
                try
                {
                    var form2 = new fromFile(this, filenameTextBox.Text);
                    form2.Show();
                    var textfile = Path.ChangeExtension(filenameTextBox.Text, "txt");
                    if (File.Exists(textfile))
                    {
                        var popup = new PopupTextLogWindow(textfile);
                        popup.Show();
                    }
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
                    Process ownerProcess = getOwnerWindow.findWindow(cancelToken, "Among Us");
                    if (ownerProcess == null)
                    {
                        OverlayWindow.open = false;
                        if (!closed)
                            Invoke(new void_stringDelegate(ChangeGetAmongUsWindowButton), "Get Among Us Window");
                        return;
                    }
                    if (!closed)
                        Invoke(new void_ProcessDelegate(createOverlayWindow), ownerProcess);
                    if (overlayForm.ownerHandle != IntPtr.Zero)
                    {
                        if (!closed)
                            Invoke(new void_stringDelegate(ChangeGetAmongUsWindowButton), "Running...");
                        gameReaderTask = Task.Factory.StartNew(() =>
                        {
                            var gameReader = Task.Factory.StartNew(() =>
                            {
                                GameMemReader.getInstance().PlayerMove += overlayForm.PlayerPosHandler;
                                GameMemReader.getInstance().GameStateChanged += overlayForm.GameStateChangedEventHandler;
                                GameMemReader.getInstance().GameStart += overlayForm.GameStartHandler;
                                GameMemReader.getInstance().ChatMessageAdded += overlayForm.TextLogHander;
                                GameMemReader.getInstance().TextLogEvent += overlayForm.TextLogHander;
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
                }, cancelToken);

            }
        }

        void ChangeGetAmongUsWindowButton(string text)
        {
            GetAmongUsWindow.Text = text;
        }

        #region setting
        internal int interval = 50;
        internal int step = 1;
        internal uint hotkey = 0x11;
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

            [DefaultValue(true)]
            public bool VoteVisible = true;

            [DefaultValue(1.0f)]
            public float PlayerSize = 1.0f;

            [DefaultValue("Control")]
            public string hotkey = "Control";

            [DefaultValue(true)]
            public bool OutputTextLog = true;

            [DefaultValue(false)]
            public bool PopupTextLog = false;

            [DefaultValue("Snow")]
            public Color backgroundColor = Color.Snow;

            [DefaultValue(false)]
            public bool AngelVisible = false;
        }

        void applySettings()
        {
            replaySpeedTrackBar.Value = settings.speed;
            mapAlphaUpdown.Value = settings.mapAlpha;

            replaySpeedTrackBar_Scroll(null, null);
            mapAlphaUpdown_ValueChanged(null, null);

            DrawMove.drawIcon = settings.playerIcon == StartWindow.PlayerIconRendering.Icon;


            DrawMove.TaskBarVisible = settings.TaskBarVisible;
            DrawMove.PlayerNameVisible = settings.PlayerNameVisible;
            DrawMove.VoteVisible = settings.VoteVisible;
            DrawMove.AngelVisible = settings.AngelVisible;
            DrawMove.backgroundColor = settings.backgroundColor;

            Map.mapFolder = settings.MapImageFolder;

            DrawMove.playerSize = settings.PlayerSize;

            if (!StartWindow.hotKeyDict.TryGetValue(settings.hotkey, out hotkey))
            {
                settings.hotkey = "Control";
                hotkey = 0x11;
            }
            StartWindow.SetHotKey(hotkey);
            OverlayWindow.OutputTextLog = settings.OutputTextLog;
            OverlayWindow.PopupTextLog = settings.PopupTextLog;
        }

        public static Dictionary<string, UInt32> hotKeyDict = new Dictionary<string, uint>()
        {
            {"Control", (UInt32)0x11 },
            {"Tab",  (UInt32)0x09 },
            {"Shift", (UInt32)0x10 }
        };

        [DllImport("KeyboardHook.dll")]
        public static extern void SetHotKey(UInt32 key);
        #endregion


        private void SettingButton_Click(object sender, EventArgs e)
        {
            SettingDialog settingDialog = new SettingDialog(this);
            settingDialog.ShowDialog();
        }

    }
}
