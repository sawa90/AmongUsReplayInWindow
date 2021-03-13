using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace AmongUsReplayInWindow
{
    public partial class PopupTextLogWindow : Form
    {
        public PopupTextLogWindow(string filename)
        {
            InitializeComponent();
            
            if (File.Exists(filename))
            {
                try
                {
                    string filetext = File.ReadAllText(filename);
                    TextLogBox.Text = filetext;
                    Text = Path.GetFileName(filename);
                    return;
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
            Close();
        }
    }
}
