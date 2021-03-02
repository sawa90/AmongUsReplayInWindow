
namespace AmongUsReplayInWindow
{
    partial class TrackBarWin
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
            this.trackBar0 = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar0)).BeginInit();
            this.SuspendLayout();
            // 
            // trackBar0
            // 
            this.trackBar0.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBar0.BackColor = System.Drawing.Color.Snow;
            this.trackBar0.Location = new System.Drawing.Point(12, 0);
            this.trackBar0.Name = "trackBar0";
            this.trackBar0.Size = new System.Drawing.Size(360, 45);
            this.trackBar0.SmallChange = 5;
            this.trackBar0.TabIndex = 0;
            this.trackBar0.TabStop = false;
            this.trackBar0.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // TrackBarWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Snow;
            this.ClientSize = new System.Drawing.Size(384, 24);
            this.Controls.Add(this.trackBar0);
            this.Name = "TrackBarWin";
            this.Text = "TrackBar1";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar0)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        System.Windows.Forms.TrackBar trackBar0;
    }
}