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
        static Image CloseDoor_h;
        static Image CloseDoor_v;
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

        static public List<Vector2>[] Cameras = new List<Vector2>[5]{
            new List<Vector2>(){new Vector2(-7.493f, 2.205f), new Vector2(-18.017f, -4.281f), new Vector2(0.232f, -6.070f), new Vector2(12.958f, -3.647f) },
            new List<Vector2>(),
            new List<Vector2>(){ new Vector2(-0.657f,15.179f), new Vector2(-7.693f,-0.498f), new Vector2(5.180f,1.753f), new Vector2(3.479f,7.115f), new Vector2(12.757f,14.892f), new Vector2(17.473f,7.115f) },
            new List<Vector2>(){ new Vector2(-7.493f, 2.205f), new Vector2(-18.017f, -4.281f), new Vector2(0.232f, -6.070f), new Vector2(12.958f, -3.647f) },
            new List<Vector2>(){ new Vector2(3.030f,16.686f), new Vector2(-4.004f,9.448f), new Vector2(16.504f,10.155f), new Vector2(23.647f,10.155f), new Vector2(-8.496f,-0.185f), new Vector2(29.978f,-0.512f), new Vector2(4.654f,-11.397f)},
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

        static public float[] electricalDoorsPos_xy = new float[2] { 1200, 638 };
        static public Vector2[] electricalDoorsPos = new Vector2[12] { new Vector2(811, 497), new Vector2(756, 497), new Vector2(811, 453), new Vector2(756, 453),
                                                                       new Vector2(700, 453), new Vector2(734, 467), new Vector2(791, 467), new Vector2(791, 420),
                                                                       new Vector2(734, 420), new Vector2(791, 511), new Vector2(678, 420), new Vector2(679, 467), };
        static public Image[] setDoorImages(int mapId)
        {
            if (mapId != 4) return null;
            string hPath = Program.exeFolder + "\\map\\" + mapFolder + "\\CloseDoor_h.png";
            string vPath = Program.exeFolder + "\\map\\" + mapFolder + "\\CloseDoor_v.png";
            if (File.Exists(hPath) && File.Exists(vPath))
            {
                Image[] MapImage = new Image[2];
                try
                {
                    using (FileStream stream = File.OpenRead(hPath))
                        MapImage[0] = Image.FromStream(stream, false, false);
                    using (FileStream stream = File.OpenRead(vPath))
                        MapImage[1] = Image.FromStream(stream, false, false);
                    return MapImage;
                }
                catch (Exception e)
                {
                    MapImage[0]?.Dispose();
                    MapImage[1]?.Dispose();
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            return null;
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
            bool IsOverlay;
            uint electricalDoors;
            Image[] DoorImages = null;

            const int SRCCOPY = 0x00CC0020;

            public backgroundMap(Size formClientSize, Point location, Size size, int mapId, bool IsOverlay, uint electricalDoors)
            {
                this.IsOverlay = IsOverlay;
                instanceList.Add(this);
                this.mapId = mapId;
                this.formClientSize = formClientSize;
                this.location = location;
                this.size = size;
                this.electricalDoors = electricalDoors;
                MapImage = setMapImage(mapId);
                DoorImages = setDoorImages(mapId);
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
                if (DoorImages != null)
                {
                    DoorImages[0]?.Dispose();
                    DoorImages[1]?.Dispose();
                }
            }

            public bool ChangeMapId(int mapId, Size formClientSize, Point location, Size size, uint electricalDoors)
            {
                if (this.mapId == mapId && (mapId != 4 || this.electricalDoors == electricalDoors)) return false;
                this.electricalDoors = electricalDoors;
                this.mapId = mapId;
                MapImage?.Dispose();
                MapImage = setMapImage(mapId);
                if (DoorImages != null)
                {
                    DoorImages[0]?.Dispose();
                    DoorImages[1]?.Dispose();
                    DoorImages = null;
                }
                DoorImages = setDoorImages(mapId);
                ChangeSize(formClientSize, location, size);
                return true;
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

            public void RedrawMap()
            {
                ChangeSize(formClientSize, location, size);
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
                if (IsOverlay)
                {
                    mapGraphics.FillRectangle(Brushes.Snow, 0, 0, w, h);
                    var oldInterpolation = mapGraphics.InterpolationMode;
                    mapGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    mapGraphics.DrawImage(Properties.Resources.stamp, w * 0.95f, h * 0.12f, w * 0.05f, w * 0.05f);
                    mapGraphics.InterpolationMode = oldInterpolation;
                }
                else
                {
                    using (Brush brush = new SolidBrush(DrawMove.backgroundColor))
                        mapGraphics.FillRectangle(brush, 0, 0, w, h);
                }
                mapGraphics.DrawImage(MapImage, location.X, location.Y, size.Width, size.Height);
                if (mapId == 4 && electricalDoors != 0 && DoorImages != null) 
                {
                    for(int i = 0; i < 5; i++)
                    {
                        if (((electricalDoors >> i) & 1) != 0)
                        {
                            var xpos = location.X + Map.electricalDoorsPos[i].X * size.Width / Map.electricalDoorsPos_xy[0];
                            var ypos = location.Y + Map.electricalDoorsPos[i].Y * size.Height / Map.electricalDoorsPos_xy[1];
                            var xsize = DoorImages[0].Width * size.Width / Map.electricalDoorsPos_xy[0];
                            var ysize = DoorImages[0].Height * size.Height / Map.electricalDoorsPos_xy[1];
                            mapGraphics.DrawImage(DoorImages[0], xpos, ypos, xsize, ysize);
                        }
                    }
                    for (int i = 5; i < 12; i++)
                    {
                        if (((electricalDoors >> i) & 1) != 0) 
                        {
                            var xpos = location.X + Map.electricalDoorsPos[i].X * size.Width / Map.electricalDoorsPos_xy[0];
                            var ypos = location.Y + Map.electricalDoorsPos[i].Y * size.Height / Map.electricalDoorsPos_xy[1];
                            var xsize = DoorImages[1].Width * size.Width / Map.electricalDoorsPos_xy[0];
                            var ysize = DoorImages[1].Height * size.Height / Map.electricalDoorsPos_xy[1];
                            mapGraphics.DrawImage(DoorImages[1], xpos, ypos, xsize, ysize);
                        }
                    }
                }
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
                    if (bgmap.DoorImages != null)
                    {
                        bgmap.DoorImages[0]?.Dispose();
                        bgmap.DoorImages[1]?.Dispose();
                        bgmap.DoorImages = null;
                    }
                    bgmap.DoorImages = setDoorImages(bgmap.mapId);
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
