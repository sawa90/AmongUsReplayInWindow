
namespace AmongUsReplayInWindow
{
    partial class PopupTextLogWindow
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
            this.TextLogBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // TextLogBox
            // 
            this.TextLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextLogBox.Location = new System.Drawing.Point(0, -3);
            this.TextLogBox.Name = "TextLogBox";
            this.TextLogBox.ReadOnly = true;
            this.TextLogBox.Size = new System.Drawing.Size(537, 400);
            this.TextLogBox.TabIndex = 0;
            this.TextLogBox.Text = "";
            // 
            // PopupTextLogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 396);
            this.Controls.Add(this.TextLogBox);
            this.Name = "PopupTextLogWindow";
            this.Text = "PopupTextLogWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox TextLogBox;
    }
}