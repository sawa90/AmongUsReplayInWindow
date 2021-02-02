using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace AmongUsReplayInWindow.setOwnerWindow
{
    class getOwnerWindow
    {
        public static Process findWindow(string processName = "Among Us")
        {
            for (int i = 0; i < 100; i++)
            {
                foreach (System.Diagnostics.Process p in
                    System.Diagnostics.Process.GetProcesses())
                {
                    if (p.ProcessName.Equals(processName) && p.MainWindowHandle!=IntPtr.Zero) return p;

                }
                System.Threading.Thread.Sleep(1000);
            }
            return null;
        }
    }



    public class AdjustToOwnerWindow
    {
        public IntPtr hOwnerWnd;
        Form form;
        Timer timer = null;
        public AdjustToOwnerWindow(Form form, IntPtr hOwnerWnd)
        {
            this.form = form;
            this.hOwnerWnd = hOwnerWnd;
            var owner = new AdjustToOwnerWindow.OwnerWindow(hOwnerWnd);
            form.Show(owner);

        }
        ~AdjustToOwnerWindow()
        {
            timer?.Stop();
            timer?.Dispose();
        }

        public void Start(int interval = 1000)
        {
            timer?.Stop();
            timer?.Dispose();
            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += new EventHandler(resize);
            timer.Enabled = true;
        }
        public void Stop()
        {
            if (timer != null)
            {
                timer?.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        public void StopResize(int interval = 1000)
        {
            timer?.Stop();
            timer?.Dispose();
            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += new EventHandler(CheckOwnerClose);
            timer.Enabled = true;
        }

        virtual public void CheckOwnerClose(object sender = null, EventArgs e = null)
        {
            if (!NativeMethods.IsWindow(hOwnerWnd))
            {
                ((Timer)sender).Enabled = false;
                Stop();
                form.Close();
            }
        }

        virtual public void resize(object sender = null, EventArgs e = null)
        {

            if (!NativeMethods.IsWindow(hOwnerWnd))
            {
                ((Timer)sender).Enabled = false;
                Stop();
                form.Close();
                return;
            }
            POINT pos = new POINT();
            NativeMethods.ClientToScreen(hOwnerWnd, out pos);
            form.Location = new Point(pos.X, pos.Y);
            RECT rect = new RECT();
            NativeMethods.GetClientRect(hOwnerWnd, out rect);
            form.Size = new Size(rect.right, rect.bottom);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X, Y;
        }
        public static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool ClientToScreen(IntPtr hwnd, out POINT lpPoint);

            [DllImport("user32.dll")]
            public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            public static extern bool IsWindow(IntPtr hWnd);
        }

        internal class OwnerWindow : IWin32Window
        {
            IntPtr handle;

            public OwnerWindow(IntPtr hwnd)
            {
                handle = hwnd;
            }
            public IntPtr Handle
            {
                get
                {
                    return handle;
                }
            }
        }

    }
    
}
