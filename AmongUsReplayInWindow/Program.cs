using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AmongUsCapture;

namespace AmongUsReplayInWindow
{
    static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")] // この行を追加
        private static extern bool AllocConsole();

        static public string exePath;
        static public string exeFolder;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (exePath != null) exeFolder = System.IO.Path.GetDirectoryName(exePath);
            if (exeFolder == null || exeFolder == string.Empty) exeFolder = "";
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //AllocConsole();
            try
            {
                var form = new ConfigWindow();
                Application.Run(form);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            //Console.WriteLine("\n\nPress Key to exit");
            //Console.ReadKey();

        }



    }
}
