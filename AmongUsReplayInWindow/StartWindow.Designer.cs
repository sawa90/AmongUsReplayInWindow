﻿
namespace AmongUsReplayInWindow
{
    partial class StartWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.GetAmongUsWindow = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogButton = new System.Windows.Forms.Button();
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.replaySpeedTrackBar = new System.Windows.Forms.TrackBar();
            this.speedLabel = new System.Windows.Forms.Label();
            this.mapAlphaLabel = new System.Windows.Forms.Label();
            this.mapAlphaUpdown = new System.Windows.Forms.NumericUpDown();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SettingButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.replaySpeedTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mapAlphaUpdown)).BeginInit();
            this.SuspendLayout();
            // 
            // GetAmongUsWindow
            // 
            this.GetAmongUsWindow.Location = new System.Drawing.Point(67, 23);
            this.GetAmongUsWindow.Name = "GetAmongUsWindow";
            this.GetAmongUsWindow.Size = new System.Drawing.Size(193, 23);
            this.GetAmongUsWindow.TabIndex = 0;
            this.GetAmongUsWindow.Text = "Get Among Us Window";
            this.GetAmongUsWindow.UseVisualStyleBackColor = true;
            this.GetAmongUsWindow.Click += new System.EventHandler(this.GetAmongUsWindow_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // openFileDialogButton
            // 
            this.openFileDialogButton.Location = new System.Drawing.Point(247, 68);
            this.openFileDialogButton.Name = "openFileDialogButton";
            this.openFileDialogButton.Size = new System.Drawing.Size(75, 23);
            this.openFileDialogButton.TabIndex = 1;
            this.openFileDialogButton.Text = "Open";
            this.openFileDialogButton.UseVisualStyleBackColor = true;
            this.openFileDialogButton.Click += new System.EventHandler(this.openFileDialogButton_Click);
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(9, 69);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.Size = new System.Drawing.Size(232, 23);
            this.filenameTextBox.TabIndex = 3;
            // 
            // replaySpeedTrackBar
            // 
            this.replaySpeedTrackBar.Location = new System.Drawing.Point(12, 123);
            this.replaySpeedTrackBar.Minimum = -10;
            this.replaySpeedTrackBar.Name = "replaySpeedTrackBar";
            this.replaySpeedTrackBar.Size = new System.Drawing.Size(290, 45);
            this.replaySpeedTrackBar.TabIndex = 4;
            this.replaySpeedTrackBar.TickFrequency = 100;
            this.replaySpeedTrackBar.Scroll += new System.EventHandler(this.replaySpeedTrackBar_Scroll);
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Location = new System.Drawing.Point(12, 105);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(76, 15);
            this.speedLabel.TabIndex = 5;
            this.speedLabel.Text = "Replay speed";
            // 
            // mapAlphaLabel
            // 
            this.mapAlphaLabel.AutoSize = true;
            this.mapAlphaLabel.Location = new System.Drawing.Point(14, 153);
            this.mapAlphaLabel.Name = "mapAlphaLabel";
            this.mapAlphaLabel.Size = new System.Drawing.Size(74, 15);
            this.mapAlphaLabel.TabIndex = 8;
            this.mapAlphaLabel.Text = "Replay alpha";
            // 
            // mapAlphaUpdown
            // 
            this.mapAlphaUpdown.Location = new System.Drawing.Point(14, 172);
            this.mapAlphaUpdown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.mapAlphaUpdown.Name = "mapAlphaUpdown";
            this.mapAlphaUpdown.Size = new System.Drawing.Size(120, 23);
            this.mapAlphaUpdown.TabIndex = 9;
            this.mapAlphaUpdown.Value = new decimal(new int[] {
            230,
            0,
            0,
            0});
            this.mapAlphaUpdown.ValueChanged += new System.EventHandler(this.mapAlphaUpdown_ValueChanged);
            // 
            // SettingButton
            // 
            this.SettingButton.Location = new System.Drawing.Point(185, 170);
            this.SettingButton.Name = "SettingButton";
            this.SettingButton.Size = new System.Drawing.Size(117, 23);
            this.SettingButton.TabIndex = 10;
            this.SettingButton.Text = "Setting...";
            this.SettingButton.UseVisualStyleBackColor = true;
            this.SettingButton.Click += new System.EventHandler(this.SettingButton_Click);
            // 
            // StartWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 207);
            this.Controls.Add(this.SettingButton);
            this.Controls.Add(this.mapAlphaUpdown);
            this.Controls.Add(this.mapAlphaLabel);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.replaySpeedTrackBar);
            this.Controls.Add(this.filenameTextBox);
            this.Controls.Add(this.openFileDialogButton);
            this.Controls.Add(this.GetAmongUsWindow);
            this.Name = "StartWindow";
            this.Text = "AmongUsReplayInWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartWindow_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.replaySpeedTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mapAlphaUpdown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GetAmongUsWindow;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button openFileDialogButton;
        private System.Windows.Forms.TextBox filenameTextBox;
        private System.Windows.Forms.TrackBar replaySpeedTrackBar;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.Label mapAlphaLabel;
        private System.Windows.Forms.NumericUpDown mapAlphaUpdown;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button SettingButton;
    }
}