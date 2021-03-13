using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace AmongUsReplayInWindow
{
    public partial class SettingDialog : Form
    {
        StartWindow startWindow;
        string[] mapFolders;
        public SettingDialog(StartWindow startWindow)
        {
            InitializeComponent();
            this.startWindow = startWindow;

            StartWindow.Settings setting = startWindow.settings;

            PlayerIcon.SelectedIndex = (int)setting.playerIcon;

            var mapFoldersAbs = Directory.GetDirectories(Program.exeFolder + @"\map", "*", SearchOption.TopDirectoryOnly);
            mapFolders = new string[mapFoldersAbs.Length];
            for(int i = 0;i< mapFoldersAbs.Length; i++)
            {
                mapFolders[i] = Path.GetFileName(mapFoldersAbs[i]);
            }
            MapImageBox.Items.AddRange(mapFolders);
            if (!Directory.Exists(Program.exeFolder + @"\map\" + setting.MapImageFolder))
                startWindow.settings.MapImageFolder = "color";
            MapImageBox.SelectedIndex = MapImageBox.Items.IndexOf(setting.MapImageFolder);

            PlayerNameCheckBox.Checked = setting.PlayerNameVisible;
            TaskBarCheckBox.Checked = setting.TaskBarVisible;
            VoteCheckBox.Checked = setting.VoteVisible;

            foreach (var key in StartWindow.hotKeyDict.Keys)
                HotKeyBox.Items.Add(key);
            HotKeyBox.SelectedItem = setting.hotkey;

            textLogCheckBox.Checked = setting.OutputTextLog;
            textLogPopupCheckBox.Checked = setting.PopupTextLog;
            textLogPopupCheckBox.Enabled = setting.OutputTextLog;
        }

        private void PlayerIcon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender != null)
                startWindow.settings.playerIcon = (StartWindow.PlayerIconRendering)PlayerIcon.SelectedIndex;
            DrawMove.drawIcon = startWindow.settings.playerIcon == StartWindow.PlayerIconRendering.Icon;

        }

        private void MapImageBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mapfolder = (string)MapImageBox.SelectedItem;
            if (mapfolder != startWindow.settings.MapImageFolder)
            {
                startWindow.settings.MapImageFolder = mapfolder;
                Map.mapFolder = mapfolder;
                Map.backgroundMap.resetImage();
            }
        }

        private void PlayerSizeBar_Scroll(object sender, EventArgs e)
        {
            float size = PlayerSizeBar.Value * 0.01f;
            startWindow.settings.PlayerSize = size;
            DrawMove.playerSize = size;
        }

        private void PlayerNameCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            startWindow.settings.PlayerNameVisible = PlayerNameCheckBox.Checked;
            DrawMove.PlayerNameVisible = PlayerNameCheckBox.Checked;
        }

        private void TaskBarCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            startWindow.settings.TaskBarVisible = TaskBarCheckBox.Checked;
            DrawMove.TaskBarVisible = TaskBarCheckBox.Checked;
        }

        private void VoteCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            startWindow.settings.VoteVisible = VoteCheckBox.Checked;
            DrawMove.VoteVisible = VoteCheckBox.Checked;
        }

        private void HotKeyBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UInt32 key;
            if(StartWindow.hotKeyDict.TryGetValue((string)HotKeyBox.SelectedItem,out key))
            {
                startWindow.settings.hotkey = (string)HotKeyBox.SelectedItem;
                startWindow.hotkey = key;
                StartWindow.SetHotKey(key);
            }
        }

        private void textLogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            startWindow.settings.OutputTextLog = textLogCheckBox.Checked;
            OverlayWindow.OutputTextLog = textLogCheckBox.Checked;
            textLogPopupCheckBox.Enabled = textLogCheckBox.Checked;
        }

        private void textLogPopupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            startWindow.settings.PopupTextLog = textLogPopupCheckBox.Checked;
            OverlayWindow.PopupTextLog = textLogPopupCheckBox.Checked;
        }
    }
}
