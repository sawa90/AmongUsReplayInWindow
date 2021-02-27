﻿
namespace AmongUsReplayInWindow
{
    partial class SettingDialog
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
            this.RenderingLabel = new System.Windows.Forms.Label();
            this.PlayerIcon = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.MapImageBox = new System.Windows.Forms.ComboBox();
            this.PlayerSizeBar = new System.Windows.Forms.TrackBar();
            this.PlayerSizeLabel = new System.Windows.Forms.Label();
            this.PlayerNameCheckBox = new System.Windows.Forms.CheckBox();
            this.TaskBarCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PlayerSizeBar)).BeginInit();
            this.SuspendLayout();
            // 
            // RenderingLabel
            // 
            this.RenderingLabel.AutoSize = true;
            this.RenderingLabel.Location = new System.Drawing.Point(12, 9);
            this.RenderingLabel.Name = "RenderingLabel";
            this.RenderingLabel.Size = new System.Drawing.Size(39, 15);
            this.RenderingLabel.TabIndex = 9;
            this.RenderingLabel.Text = "Player";
            // 
            // PlayerIcon
            // 
            this.PlayerIcon.FormattingEnabled = true;
            this.PlayerIcon.Items.AddRange(new object[] {
            "Icon",
            "Simple"});
            this.PlayerIcon.Location = new System.Drawing.Point(12, 30);
            this.PlayerIcon.Name = "PlayerIcon";
            this.PlayerIcon.Size = new System.Drawing.Size(121, 23);
            this.PlayerIcon.TabIndex = 8;
            this.PlayerIcon.SelectedIndexChanged += new System.EventHandler(this.PlayerIcon_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(176, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Map";
            // 
            // MapImageBox
            // 
            this.MapImageBox.FormattingEnabled = true;
            this.MapImageBox.Location = new System.Drawing.Point(176, 30);
            this.MapImageBox.Name = "MapImageBox";
            this.MapImageBox.Size = new System.Drawing.Size(121, 23);
            this.MapImageBox.TabIndex = 10;
            this.MapImageBox.SelectedIndexChanged += new System.EventHandler(this.MapImageBox_SelectedIndexChanged);
            // 
            // PlayerSizeBar
            // 
            this.PlayerSizeBar.Location = new System.Drawing.Point(12, 99);
            this.PlayerSizeBar.Maximum = 200;
            this.PlayerSizeBar.Minimum = 10;
            this.PlayerSizeBar.Name = "PlayerSizeBar";
            this.PlayerSizeBar.Size = new System.Drawing.Size(285, 45);
            this.PlayerSizeBar.SmallChange = 5;
            this.PlayerSizeBar.TabIndex = 12;
            this.PlayerSizeBar.TickFrequency = 10;
            this.PlayerSizeBar.Value = 100;
            this.PlayerSizeBar.Scroll += new System.EventHandler(this.PlayerSizeBar_Scroll);
            // 
            // PlayerSizeLabel
            // 
            this.PlayerSizeLabel.AutoSize = true;
            this.PlayerSizeLabel.Location = new System.Drawing.Point(12, 70);
            this.PlayerSizeLabel.Name = "PlayerSizeLabel";
            this.PlayerSizeLabel.Size = new System.Drawing.Size(62, 15);
            this.PlayerSizeLabel.TabIndex = 13;
            this.PlayerSizeLabel.Text = "Player Size";
            // 
            // PlayerNameCheckBox
            // 
            this.PlayerNameCheckBox.AutoSize = true;
            this.PlayerNameCheckBox.Location = new System.Drawing.Point(23, 155);
            this.PlayerNameCheckBox.Name = "PlayerNameCheckBox";
            this.PlayerNameCheckBox.Size = new System.Drawing.Size(92, 19);
            this.PlayerNameCheckBox.TabIndex = 14;
            this.PlayerNameCheckBox.Text = "Player Name";
            this.PlayerNameCheckBox.UseVisualStyleBackColor = true;
            this.PlayerNameCheckBox.CheckedChanged += new System.EventHandler(this.PlayerNameCheckBox_CheckedChanged);
            // 
            // TaskBarCheckBox
            // 
            this.TaskBarCheckBox.AutoSize = true;
            this.TaskBarCheckBox.Location = new System.Drawing.Point(159, 155);
            this.TaskBarCheckBox.Name = "TaskBarCheckBox";
            this.TaskBarCheckBox.Size = new System.Drawing.Size(68, 19);
            this.TaskBarCheckBox.TabIndex = 15;
            this.TaskBarCheckBox.Text = "Task Bar";
            this.TaskBarCheckBox.UseVisualStyleBackColor = true;
            this.TaskBarCheckBox.CheckedChanged += new System.EventHandler(this.TaskBarCheckBox_CheckedChanged);
            // 
            // SettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 189);
            this.Controls.Add(this.TaskBarCheckBox);
            this.Controls.Add(this.PlayerNameCheckBox);
            this.Controls.Add(this.PlayerSizeLabel);
            this.Controls.Add(this.PlayerSizeBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MapImageBox);
            this.Controls.Add(this.RenderingLabel);
            this.Controls.Add(this.PlayerIcon);
            this.Name = "SettingDialog";
            this.Text = "SettingDialog";
            ((System.ComponentModel.ISupportInitialize)(this.PlayerSizeBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label RenderingLabel;
        private System.Windows.Forms.ComboBox PlayerIcon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox MapImageBox;
        private System.Windows.Forms.TrackBar PlayerSizeBar;
        private System.Windows.Forms.Label PlayerSizeLabel;
        private System.Windows.Forms.CheckBox PlayerNameCheckBox;
        private System.Windows.Forms.CheckBox TaskBarCheckBox;
    }
}