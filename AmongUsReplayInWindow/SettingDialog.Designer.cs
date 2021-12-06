
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
            this.HotKeyBox = new System.Windows.Forms.ComboBox();
            this.HotKeyLabel = new System.Windows.Forms.Label();
            this.VoteCheckBox = new System.Windows.Forms.CheckBox();
            this.textLogCheckBox = new System.Windows.Forms.CheckBox();
            this.textLogPopupCheckBox = new System.Windows.Forms.CheckBox();
            this.backgroundColorDialog = new System.Windows.Forms.ColorDialog();
            this.backgroundColorButton = new System.Windows.Forms.Button();
            this.AngelCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.PlayerSizeBar)).BeginInit();
            this.SuspendLayout();
            // 
            // RenderingLabel
            // 
            this.RenderingLabel.AutoSize = true;
            this.RenderingLabel.Location = new System.Drawing.Point(24, 130);
            this.RenderingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.RenderingLabel.Name = "RenderingLabel";
            this.RenderingLabel.Size = new System.Drawing.Size(59, 25);
            this.RenderingLabel.TabIndex = 9;
            this.RenderingLabel.Text = "Player";
            // 
            // PlayerIcon
            // 
            this.PlayerIcon.FormattingEnabled = true;
            this.PlayerIcon.Items.AddRange(new object[] {
            "Icon",
            "Simple"});
            this.PlayerIcon.Location = new System.Drawing.Point(24, 165);
            this.PlayerIcon.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PlayerIcon.Name = "PlayerIcon";
            this.PlayerIcon.Size = new System.Drawing.Size(171, 33);
            this.PlayerIcon.TabIndex = 8;
            this.PlayerIcon.SelectedIndexChanged += new System.EventHandler(this.PlayerIcon_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(259, 130);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 25);
            this.label1.TabIndex = 11;
            this.label1.Text = "Map";
            // 
            // MapImageBox
            // 
            this.MapImageBox.FormattingEnabled = true;
            this.MapImageBox.Location = new System.Drawing.Point(259, 165);
            this.MapImageBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MapImageBox.Name = "MapImageBox";
            this.MapImageBox.Size = new System.Drawing.Size(171, 33);
            this.MapImageBox.TabIndex = 10;
            this.MapImageBox.SelectedIndexChanged += new System.EventHandler(this.MapImageBox_SelectedIndexChanged);
            // 
            // PlayerSizeBar
            // 
            this.PlayerSizeBar.Location = new System.Drawing.Point(24, 280);
            this.PlayerSizeBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PlayerSizeBar.Maximum = 200;
            this.PlayerSizeBar.Minimum = 10;
            this.PlayerSizeBar.Name = "PlayerSizeBar";
            this.PlayerSizeBar.Size = new System.Drawing.Size(407, 69);
            this.PlayerSizeBar.SmallChange = 5;
            this.PlayerSizeBar.TabIndex = 12;
            this.PlayerSizeBar.TickFrequency = 10;
            this.PlayerSizeBar.Value = 100;
            this.PlayerSizeBar.Scroll += new System.EventHandler(this.PlayerSizeBar_Scroll);
            // 
            // PlayerSizeLabel
            // 
            this.PlayerSizeLabel.AutoSize = true;
            this.PlayerSizeLabel.Location = new System.Drawing.Point(24, 232);
            this.PlayerSizeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.PlayerSizeLabel.Name = "PlayerSizeLabel";
            this.PlayerSizeLabel.Size = new System.Drawing.Size(95, 25);
            this.PlayerSizeLabel.TabIndex = 13;
            this.PlayerSizeLabel.Text = "Player Size";
            // 
            // PlayerNameCheckBox
            // 
            this.PlayerNameCheckBox.AutoSize = true;
            this.PlayerNameCheckBox.Location = new System.Drawing.Point(40, 373);
            this.PlayerNameCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PlayerNameCheckBox.Name = "PlayerNameCheckBox";
            this.PlayerNameCheckBox.Size = new System.Drawing.Size(137, 29);
            this.PlayerNameCheckBox.TabIndex = 14;
            this.PlayerNameCheckBox.Text = "Player Name";
            this.PlayerNameCheckBox.UseVisualStyleBackColor = true;
            this.PlayerNameCheckBox.CheckedChanged += new System.EventHandler(this.PlayerNameCheckBox_CheckedChanged);
            // 
            // TaskBarCheckBox
            // 
            this.TaskBarCheckBox.AutoSize = true;
            this.TaskBarCheckBox.Location = new System.Drawing.Point(234, 373);
            this.TaskBarCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TaskBarCheckBox.Name = "TaskBarCheckBox";
            this.TaskBarCheckBox.Size = new System.Drawing.Size(101, 29);
            this.TaskBarCheckBox.TabIndex = 15;
            this.TaskBarCheckBox.Text = "Task Bar";
            this.TaskBarCheckBox.UseVisualStyleBackColor = true;
            this.TaskBarCheckBox.CheckedChanged += new System.EventHandler(this.TaskBarCheckBox_CheckedChanged);
            // 
            // HotKeyBox
            // 
            this.HotKeyBox.FormattingEnabled = true;
            this.HotKeyBox.Location = new System.Drawing.Point(24, 68);
            this.HotKeyBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.HotKeyBox.Name = "HotKeyBox";
            this.HotKeyBox.Size = new System.Drawing.Size(171, 33);
            this.HotKeyBox.TabIndex = 16;
            this.HotKeyBox.SelectedIndexChanged += new System.EventHandler(this.HotKeyBox_SelectedIndexChanged);
            // 
            // HotKeyLabel
            // 
            this.HotKeyLabel.AutoSize = true;
            this.HotKeyLabel.Location = new System.Drawing.Point(24, 38);
            this.HotKeyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.HotKeyLabel.Name = "HotKeyLabel";
            this.HotKeyLabel.Size = new System.Drawing.Size(75, 25);
            this.HotKeyLabel.TabIndex = 17;
            this.HotKeyLabel.Text = "Hot Key";
            // 
            // VoteCheckBox
            // 
            this.VoteCheckBox.AutoSize = true;
            this.VoteCheckBox.Location = new System.Drawing.Point(40, 415);
            this.VoteCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.VoteCheckBox.Name = "VoteCheckBox";
            this.VoteCheckBox.Size = new System.Drawing.Size(74, 29);
            this.VoteCheckBox.TabIndex = 18;
            this.VoteCheckBox.Text = "Vote";
            this.VoteCheckBox.UseVisualStyleBackColor = true;
            this.VoteCheckBox.CheckedChanged += new System.EventHandler(this.VoteCheckBox_CheckedChanged);
            // 
            // textLogCheckBox
            // 
            this.textLogCheckBox.AutoSize = true;
            this.textLogCheckBox.Location = new System.Drawing.Point(234, 415);
            this.textLogCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textLogCheckBox.Name = "textLogCheckBox";
            this.textLogCheckBox.Size = new System.Drawing.Size(160, 29);
            this.textLogCheckBox.TabIndex = 19;
            this.textLogCheckBox.Text = "Output text log";
            this.textLogCheckBox.UseVisualStyleBackColor = true;
            this.textLogCheckBox.CheckedChanged += new System.EventHandler(this.textLogCheckBox_CheckedChanged);
            // 
            // textLogPopupCheckBox
            // 
            this.textLogPopupCheckBox.AutoSize = true;
            this.textLogPopupCheckBox.Location = new System.Drawing.Point(259, 457);
            this.textLogPopupCheckBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textLogPopupCheckBox.Name = "textLogPopupCheckBox";
            this.textLogPopupCheckBox.Size = new System.Drawing.Size(161, 29);
            this.textLogPopupCheckBox.TabIndex = 20;
            this.textLogPopupCheckBox.Text = "Pop up text log";
            this.textLogPopupCheckBox.UseVisualStyleBackColor = true;
            this.textLogPopupCheckBox.CheckedChanged += new System.EventHandler(this.textLogPopupCheckBox_CheckedChanged);
            // 
            // backgroundColorButton
            // 
            this.backgroundColorButton.Location = new System.Drawing.Point(259, 67);
            this.backgroundColorButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.backgroundColorButton.Name = "backgroundColorButton";
            this.backgroundColorButton.Size = new System.Drawing.Size(173, 38);
            this.backgroundColorButton.TabIndex = 21;
            this.backgroundColorButton.Text = "Background Color";
            this.backgroundColorButton.UseVisualStyleBackColor = true;
            this.backgroundColorButton.Click += new System.EventHandler(this.backgroundColorButton_Click);
            // 
            // AngelCheckBox
            // 
            this.AngelCheckBox.AutoSize = true;
            this.AngelCheckBox.Location = new System.Drawing.Point(40, 457);
            this.AngelCheckBox.Name = "AngelCheckBox";
            this.AngelCheckBox.Size = new System.Drawing.Size(84, 29);
            this.AngelCheckBox.TabIndex = 22;
            this.AngelCheckBox.Text = "Angel";
            this.AngelCheckBox.UseVisualStyleBackColor = true;
            this.AngelCheckBox.CheckedChanged += new System.EventHandler(this.AngelCheckBox_CheckedChanged);
            // 
            // SettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 520);
            this.Controls.Add(this.AngelCheckBox);
            this.Controls.Add(this.backgroundColorButton);
            this.Controls.Add(this.textLogPopupCheckBox);
            this.Controls.Add(this.textLogCheckBox);
            this.Controls.Add(this.VoteCheckBox);
            this.Controls.Add(this.HotKeyLabel);
            this.Controls.Add(this.HotKeyBox);
            this.Controls.Add(this.TaskBarCheckBox);
            this.Controls.Add(this.PlayerNameCheckBox);
            this.Controls.Add(this.PlayerSizeLabel);
            this.Controls.Add(this.PlayerSizeBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MapImageBox);
            this.Controls.Add(this.RenderingLabel);
            this.Controls.Add(this.PlayerIcon);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
        private System.Windows.Forms.ComboBox HotKeyBox;
        private System.Windows.Forms.Label HotKeyLabel;
        private System.Windows.Forms.CheckBox VoteCheckBox;
        private System.Windows.Forms.CheckBox textLogCheckBox;
        private System.Windows.Forms.CheckBox textLogPopupCheckBox;
        private System.Windows.Forms.ColorDialog backgroundColorDialog;
        private System.Windows.Forms.Button backgroundColorButton;
        private System.Windows.Forms.CheckBox AngelCheckBox;
    }
}