using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AmongUsReplayInWindow
{
    static class Program
    {
        [DllImport("kernel32.dll")] 
        private static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        static public string exePath;
        static public string exeFolder;

        public const bool testflag = false;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (exePath != null) exeFolder = System.IO.Path.GetDirectoryName(exePath);
            if (exeFolder == null || exeFolder == string.Empty) exeFolder = "";
            MoveLogFile.ClearTemp();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //AllocConsole();
            if (testflag)
            {
                AmongUsCapture.GameMemReader.testflag = true;
            }
            try
            {
                var form = new StartWindow();
                Application.Run(form);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            //Console.WriteLine("\n\nPress Key to exit");
            //Console.ReadKey();
            //FreeConsole();

        }



    }
}
