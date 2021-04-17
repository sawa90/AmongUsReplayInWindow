using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Numerics;

namespace AmongUsReplayInWindow
{
    public static class Map
    {
        static string[] mapFilename = new string[5] { "skeld.png", "mira.png", "polus.png", "skeld.png", "airship.png" };
        static public string mapFolder = "color";
        public struct MapScale
        {
            public int Id;
            public float hw, xs, ys, xp, yp, xpad, ypad;
            public Vector2 centerOfTable;
        }

        static public MapScale[] Maps = new MapScale[5]
        { new MapScale
            {
                Id = 0,
                hw = 0.58f,
                xs = 0.022f,
                ys = 0.038f,
                xp = 0.554f,
                yp = 0.292f,
                xpad = 0,
                ypad = 0,
                centerOfTable = new Vector2(-1.0f, 1.1f)
            },
            new MapScale
            {
                Id = 1,
                hw = 0.73f,
                xs = 0.0223f,
                ys = 0.0305f,
                xp = 0.30f,
                yp = 0.83f,
                xpad = 0,
                ypad = 0,
                centerOfTable = new Vector2(24.03f,2.625f)
            },
            new MapScale
            {
                Id = 2,
                hw = 0.69f,
                xs = 0.0232f,
                ys = 0.0335f,
                xp = 0.021f,
                yp = 0.093f,
                xpad = 0,
                ypad = 0,
                centerOfTable = new Vector2(19.5f,-16.876f)
            },
            new MapScale
            {
                Id = 3,
                hw = 0.58f,
                xs = 0.022f,
                ys = 0.038f,
                xp = 0.554f,
                yp = 0.292f,
                xpad = 0,
                ypad = 0,
                centerOfTable = new Vector2(-1.0f, 1.1f)
            },
            new MapScale
            {
                Id = 4,
                hw = 0.53f,
                xs =  0.0154f,
                ys = 0.0288f,
                xp = 0.390f,
                yp = 0.5025f,
                xpad = 0.05f,
                ypad = 0.05f,//0.29f,
                centerOfTable = new Vector2(11, 15)
            },
        };

        static public Image setMapImage(int mapId)
        {
            Image MapImage = null;
            if (File.Exists(Program.exeFolder + "\\map\\" + mapFolder + "\\" + mapFilename[mapId]))
            {
                try
                {
                    using (FileStream stream = File.OpenRead(Program.exeFolder + "\\map\\"+mapFolder +"\\" + mapFilename[mapId]))
                        MapImage = Image.FromStream(stream, false, false);
                    return MapImage;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            switch (mapId)
            {
                case 0:
                    MapImage = Properties.Resources.skeld;
                    break;
                case 1:
                    MapImage = Properties.Resources.mira;
                    break;
                case 2:
                    MapImage = Properties.Resources.polus;
                    break;
                case 3:
                    MapImage = Properties.Resources.skeld;
                    break;
                case 4:
                    MapImage = Properties.Resources.airship;
                    break;
                default:
                    Console.WriteLine($"Not found map image ID={mapId}");
                    throw new FileNotFoundException();
            }
            return MapImage;
        }

        public class backgroundMap
        {
            static List<backgroundMap> instanceList = new List<backgroundMap>();

            int mapId;
            Image MapImage;
            Bitmap bitmap;
            IntPtr mapGDI;
            int w, h;
            Size formClientSize;
            Point location;
            Size size;

            const int SRCCOPY = 0x00CC0020;

            public backgroundMap(Size formClientSize, Point location, Size size, int mapId)
            {
                instanceList.Add(this);
                this.mapId = mapId;
                MapImage = setMapImage(mapId);
                ChangeSize(formClientSize, location, size);
            }

            ~backgroundMap()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (instanceList.Contains(this))
                    instanceList.Remove(this);
                DisposeBitmap();
                MapImage?.Dispose();
            }
            
            public void ChangeMapId(int mapId, Size formClientSize, Point location, Size size)
            {
                if (this.mapId != mapId)
                {
                    this.mapId = mapId;
                    MapImage?.Dispose();
                    MapImage = setMapImage(mapId);
                    ChangeSize(formClientSize, location, size);
                }
            }

            public void DisposeBitmap()
            {
                if (mapGDI != IntPtr.Zero)
                {
                    DeleteObject(mapGDI);
                    mapGDI = IntPtr.Zero;
                    bitmap?.Dispose();
                    bitmap = null;
                }
            }

            public bool Draw(Graphics g)
            {
                var backHdc = g.GetHdc();
                var backBuffer = CreateCompatibleDC(backHdc);
                IntPtr l = SelectObject(backBuffer, mapGDI);

                bool result = BitBlt(backHdc, 0, 0, w, h, backBuffer, 0, 0, SRCCOPY);

                SelectObject(backBuffer, l);
                DeleteDC(backBuffer);
                g.ReleaseHdc(backHdc);
                return result;
            }

            public void ChangeSize(Size formClientSize, Point location, Size size)
            {
                if (size.Width <= 0 || size.Width <= 0) return;
                this.formClientSize = formClientSize;
                this.location = location;
                this.size = size;
                DisposeBitmap();
                w = formClientSize.Width;
                h = formClientSize.Height;
                bitmap = new Bitmap(w, h);
                Graphics mapGraphics = Graphics.FromImage(bitmap);
                var smoothing = mapGraphics.SmoothingMode;
                var interpolation = mapGraphics.InterpolationMode;
                mapGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                mapGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                mapGraphics.FillRectangle(Brushes.Snow, 0, 0, w, h);
                mapGraphics.DrawImage(MapImage, location.X, location.Y, size.Width, size.Height);
                mapGraphics.SmoothingMode = smoothing;
                mapGraphics.InterpolationMode = interpolation;
                mapGraphics.Dispose();

                mapGDI = bitmap.GetHbitmap();
            }

            public static void resetImage()
            {
                foreach(var bgmap in instanceList)
                {
                    bgmap.MapImage?.Dispose();
                    bgmap.MapImage = setMapImage(bgmap.mapId);
                    bgmap.ChangeSize(bgmap.formClientSize, bgmap.location, bgmap.size);
                }
            }

            [DllImport("gdi32.dll", SetLastError = true)]
            private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSource, int nXSource, int nYSource, uint dwRaster);
            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            static extern IntPtr DeleteObject(IntPtr hgdiobj);

        }
    }
}
