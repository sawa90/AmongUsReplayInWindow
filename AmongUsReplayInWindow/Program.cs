using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

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
        static bool ConsoleIsOpen = false;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (exePath != null) exeFolder = System.IO.Path.GetDirectoryName(exePath);
            if (exeFolder == null || exeFolder == string.Empty) exeFolder = "";
            MoveLogFile.ClearTemp();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            StartWindow.LoadSettings();
            if (StartWindow.settings?.Console == true) ConsoleIsOpen = AllocConsole();
            //AllocConsole();
            if (testflag)
            {
                AmongUsCapture.GameMemReader.testflag = true;
            }
            if (args.Length > 0)
            {
                int ptr = int.Parse(args[0]);
                Console.WriteLine($"ReadSpace:{ptr:X}");
                if (ptr > 0)
                {
                    AmongUsCapture.ReadSpace.readSpacePtr = (IntPtr)ptr;
                    AmongUsCapture.ReadSpace.ExistReadSpace = true;
                }
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
            if (ConsoleIsOpen)
            {
                Console.WriteLine("\n\nPress Key to exit");
                Console.ReadKey();
                FreeConsole();
            }

        }



    }
}
