using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AmongUsCapture;
using System.Windows.Forms;

namespace AmongUsReplayInWindow
{
    class DrawMove
    {

        public class IconDict
        {
            public Dictionary<Color, Image> icons = null;
            public Image impostor = null;
            public Image vent = null;
            public IconDict()
            {
                try
                {
                    using (FileStream stream = File.OpenRead(Program.exeFolder + "\\icon\\impostor.png"))
                    {
                        impostor = Image.FromStream(stream, false, false);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message + "\n" + e.StackTrace + "\n");
                    impostor = Properties.Resources.impostor;
                }

                try
                {
                    using (FileStream stream = File.OpenRead(Program.exeFolder + "\\icon\\vent.png"))
                    {
                        vent = Image.FromStream(stream, false, false);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message + "\n" + e.StackTrace + "\n");
                    vent = Properties.Resources.vent;
                }

                icons = new Dictionary<Color, Image>();
                {

                    int colorNum = GameMemReader.ColorList.Length;
                    for (int i = 0; i < colorNum; i++)
                    {
                        Color color = Color.FromArgb(GameMemReader.ColorList[i].ToArgb());
                        string iconName = Program.exeFolder + "\\icon\\" + ((PlayerColor)i).ToString() + ".png";
                        try
                        {
                            using (FileStream stream = File.OpenRead(iconName))
                            {
                                icons.Add(color, Image.FromStream(stream, false, false));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Write(e.Message + "\n" + e.StackTrace + "\n");
                        }
                    }
                }
            }
            ~IconDict()
            {
                Dispose();
            }
            public void Dispose()
            {
                if (impostor != null)
                {
                    impostor.Dispose();
                    impostor = null;
                }
                if (vent != null)
                {
                    vent.Dispose();
                    vent = null;
                }
                if (icons != null)
                {
                    foreach (var icon in icons.Values)
                    {
                        icon?.Dispose();
                    }
                    icons.Clear();
                    icons = null;
                }
            }
        }

        static public void DrawMove_Icon(PaintEventArgs paint, PlayerMoveArgs move, List<int> deadOrderList, Map.MapScale map, IconDict icons, Point mapLocation, Size mapSize)
        {
            paint.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            string fontName = "Times New Roman";
            if (move == null) return;
            int AllImpostorNum = 0;

            float circleSize = mapSize.Height / 39.0f;
            float iconSize = mapSize.Height / 45.0f;
            float dsize = Math.Max(1, circleSize / 5.0f);
            using (var fnt = new Font(fontName, circleSize))
            {
                int minutes = move.time / 60000;
                int seconds = move.time / 1000 - minutes * 60;

                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 4.5f, circleSize * 18, circleSize * 1.2f);
                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 5.7f, circleSize * 8, circleSize * 1.2f);
                if (move.Sabotage.TaskType != TaskTypes.SubmitScan && move.state == GameState.TASKS)
                    using (var fnt2 = new Font(fontName, circleSize, FontStyle.Bold))
                        paint.Graphics.DrawString(move.Sabotage.TaskType.ToString(), fnt2, Brushes.Red, 0, circleSize * 4.5f);
                else
                    paint.Graphics.DrawString(move.state.ToString(), fnt, Brushes.Black, 0, circleSize * 4.5f);
                paint.Graphics.DrawString($"{minutes:00}:{seconds:00}", fnt, Brushes.Black, 0, circleSize * 5.7f);

                paint.Graphics.FillRectangle(Brushes.LightGray, 0, 0, circleSize * 25, circleSize * 4.5f);
                paint.Graphics.DrawString("KILLED\t          :", fnt, Brushes.Black, 0, circleSize * 0.0f);
                paint.Graphics.DrawString("EJECTED        :", fnt, Brushes.Black, 0, circleSize * 1.5f);
                paint.Graphics.DrawString("DISCONNECT :", fnt, Brushes.Black, 0, circleSize * 3.0f);
            }
            int killed = 0, ejjected = 0, disconnected = 0;



            using (var fnt = new Font(fontName, circleSize * 0.8f))
            {
                //set dead order
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    if (move.PlayerIsDead[i] != 0 && !deadOrderList.Contains(i)) deadOrderList.Add(i);
                    if (move.IsImpostor[i]) AllImpostorNum++;
                }


                //draw living crew
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    if (move.IsImpostor[i] || move.PlayerColors == null || move.PlayerIsDead[i] != 0) continue;

                    using (var brush = new SolidBrush(move.PlayerColors[i]))
                    {

                        int pointX = mapLocation.X + (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * mapSize.Width);
                        int pointY = mapLocation.Y + (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * mapSize.Height);

                        Image icon = icons.icons.GetValueOrDefault(move.PlayerColors[i]);
                        if (icon != null)
                        {
                            float icon_w = iconSize * icon.Width / icon.Height;
                            paint.Graphics.DrawImage(icon, pointX - icon_w, pointY - iconSize, icon_w * 2, iconSize * 2);
                            paint.Graphics.DrawString(move.PlayerNames[i], fnt, Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 2.0f);
                        }
                        else
                        {
                            paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                            paint.Graphics.DrawString(move.PlayerNames[i], fnt, Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);

                        }

                        paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                        paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                    }
                }


                //DrawImp
                for (int i = 0; i < AllImpostorNum; i++)
                {
                    int id = move.ImpostorId[i];
                    if (move.PlayerColors == null || move.PlayerIsDead[id] != 0) continue;


                    using (var brush = new SolidBrush(move.PlayerColors[id]))
                    using (Pen pen = new Pen(GetCColor(move.PlayerColors[id]), dsize))
                    {
                        int pointX = mapLocation.X + (int)(((move.PlayerPoses[id].X) * map.xs + map.xp) * mapSize.Width);
                        int pointY = mapLocation.Y + (int)(((-move.PlayerPoses[id].Y) * map.ys + map.yp) * mapSize.Height);
                        Image icon = icons.icons.GetValueOrDefault(move.PlayerColors[id]);
                        if (move.InVent[i])
                        {
                            if (icon != null)
                            {
                                float icon_w = iconSize * icon.Width / icon.Height;
                                paint.Graphics.DrawImage(icons.vent, pointX - icon_w, pointY - iconSize, icon_w * 2, iconSize * 2);
                                paint.Graphics.DrawImage(icon, pointX - icon_w * 0.8f, pointY - iconSize * 0.6f, icon_w * 1.6f, iconSize * 1.6f);
                                paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.DarkRed, pointX - circleSize * 1.5f, pointY - circleSize * 2.0f);
                            }
                            else
                            {
                                int s = (int)(circleSize * 0.8);
                                int cos = (int)(Math.Cos(Math.PI / 3) * s);
                                int sin = (int)(Math.Sin(Math.PI / 3) * s);
                                Point[] points = {
                                         new Point(pointX, pointY - s),
                                         new Point(pointX + sin, pointY + cos),
                                         new Point(pointX - sin, pointY + cos )
                                };
                                paint.Graphics.FillPolygon(brush, points);
                                paint.Graphics.DrawPolygon(pen, points);
                                paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.DarkRed, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            }
                        }
                        else
                        {
                            if (icon != null)
                            {
                                float icon_w = iconSize * icon.Width / icon.Height;
                                paint.Graphics.DrawImage(icons.impostor, pointX - icon_w, pointY - iconSize, icon_w * 2, iconSize * 2);
                                paint.Graphics.DrawImage(icon, pointX - icon_w, pointY - iconSize, icon_w * 2, iconSize * 2);
                                paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.DarkRed, pointX - circleSize * 1.5f, pointY - circleSize * 2.0f);
                            }
                            else
                            {
                                paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                                paint.Graphics.DrawEllipse(pen, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                                paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.DarkRed, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            }
                        }


                    }
                }

                //draw dead
                foreach (int i in deadOrderList)
                {
                    if (move.PlayerIsDead[i] != 0)
                    {
                        using (var brush = new SolidBrush(move.PlayerColors[i]))
                        {

                            int pointX = mapLocation.X + (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * mapSize.Width);
                            int pointY = mapLocation.Y + (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * mapSize.Height);


                            if (move.PlayerIsDead[i] == -10)
                            {
                                pointX = (int)(circleSize * (2.0 * disconnected + 10.5));
                                pointY = (int)(circleSize * 3.7);
                                disconnected++;
                            }
                            else if (move.PlayerIsDead[i] == -11)
                            {
                                pointX = (int)(circleSize * (2.0 * ejjected + 10.5));
                                pointY = (int)(circleSize * 2.2);
                                ejjected++;
                            }
                            else if (move.PlayerIsDead[i] <= -20)
                            {
                                pointX = (int)(circleSize * (2.0 * killed + 10.5));
                                pointY = (int)(circleSize * 0.7);
                                killed++;
                            }

                            float d = dsize * 1.8f;
                            PointF[] points = {  new PointF(pointX - d, pointY - d * 2),
                                            new PointF(pointX, pointY - d),
                                            new PointF(pointX + d, pointY - d * 2),
                                            new PointF(pointX + d * 2, pointY - d),
                                            new PointF(pointX + d, pointY),
                                            new PointF(pointX + d * 2, pointY + d),
                                            new PointF(pointX + d, pointY + d * 2),
                                            new PointF(pointX, pointY + d),
                                            new PointF(pointX - d, pointY + d * 2),
                                            new PointF(pointX - d * 2, pointY + d),
                                            new PointF(pointX - d, pointY),
                                            new PointF(pointX - d * 2, pointY - d),
                                            };
                            paint.Graphics.FillPolygon(brush, points);
                            if (Math.Abs(move.PlayerIsDead[i]) >= 20)
                            {
                                using (var pen = new Pen(move.PlayerColors[Math.Abs(move.PlayerIsDead[i]) - 20], dsize))
                                    paint.Graphics.DrawPolygon(pen, points);
                            }

                            if (move.PlayerIsDead[i] > 0)
                                paint.Graphics.DrawString(move.PlayerNames[i], fnt, move.IsImpostor[i] ? Brushes.Red : Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            else if (move.IsImpostor[i])
                                paint.Graphics.DrawString("imp", fnt, (move.PlayerColors[i].ToArgb() == Color.Red.ToArgb() || move.PlayerColors[i].ToArgb() == Color.HotPink.ToArgb()) ? Brushes.Black : Brushes.Red, pointX - circleSize * 1.0f, pointY - circleSize * 0.7f);
                            if (move.PlayerIsDead[i] == 11)
                                paint.Graphics.DrawString("ejected", fnt, move.PlayerColors[i].ToArgb() == Color.Black.ToArgb() ? Brushes.Red : Brushes.Black, pointX - circleSize * 2.0f, pointY - circleSize * 0.7f);

                            if (!move.IsImpostor[i])
                            {
                                paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                                paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                            }

                        }
                    }
                }

            }


        }

        static public void DrawMove_Simple(PaintEventArgs paint, PlayerMoveArgs move, List<int> deadOrderList, Map.MapScale map, Point mapLocation, Size mapSize)
        {
            paint.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            string fontName = "Times New Roman";
            if (move == null) return;
            int AllImpostorNum = 0;

            float circleSize = mapSize.Height / 39.0f;
            float dsize = Math.Max(1, circleSize / 5.0f);
            using (var fnt = new Font(fontName, circleSize))
            {
                int minutes = move.time / 60000;
                int seconds = move.time / 1000 - minutes * 60;

                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 4.5f, circleSize * 18, circleSize * 1.2f);
                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 5.7f, circleSize * 8, circleSize * 1.2f);
                if (move.Sabotage.TaskType != TaskTypes.SubmitScan && move.state == GameState.TASKS)
                    using (var fnt2 = new Font(fontName, circleSize, FontStyle.Bold))
                        paint.Graphics.DrawString(move.Sabotage.TaskType.ToString(), fnt2, Brushes.Red, 0, circleSize * 4.5f);
                else
                    paint.Graphics.DrawString(move.state.ToString(), fnt, Brushes.Black, 0, circleSize * 4.5f);
                paint.Graphics.DrawString($"{minutes:00}:{seconds:00}", fnt, Brushes.Black, 0, circleSize * 5.7f);

                paint.Graphics.FillRectangle(Brushes.LightGray, 0, 0, circleSize * 25, circleSize * 4.5f);
                paint.Graphics.DrawString("KILLED\t          :", fnt, Brushes.Black, 0, circleSize * 0.0f);
                paint.Graphics.DrawString("EJECTED        :", fnt, Brushes.Black, 0, circleSize * 1.5f);
                paint.Graphics.DrawString("DISCONNECT :", fnt, Brushes.Black, 0, circleSize * 3.0f);
            }
            int killed = 0, ejjected = 0, disconnected = 0;



            using (var fnt = new Font(fontName, circleSize * 0.8f))
            {
                //set dead order
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    if (move.PlayerIsDead[i] != 0 && !deadOrderList.Contains(i)) deadOrderList.Add(i);
                }

                //draw dead
                foreach (int i in deadOrderList)
                {
                    if (move.PlayerIsDead[i] != 0)
                    {
                        using (var brush = new SolidBrush(move.PlayerColors[i]))
                        {

                            int pointX = mapLocation.X + (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * mapSize.Width);
                            int pointY = mapLocation.Y + (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * mapSize.Height);


                            if (move.PlayerIsDead[i] == -10)
                            {
                                pointX = (int)(circleSize * (2.0 * disconnected + 10.5));
                                pointY = (int)(circleSize * 3.7);
                                disconnected++;
                            }
                            else if (move.PlayerIsDead[i] == -11)
                            {
                                pointX = (int)(circleSize * (2.0 * ejjected + 10.5));
                                pointY = (int)(circleSize * 2.2);
                                ejjected++;
                            }
                            else if (move.PlayerIsDead[i] <= -20)
                            {
                                pointX = (int)(circleSize * (2.0 * killed + 10.5));
                                pointY = (int)(circleSize * 0.7);
                                killed++;
                            }

                            float d = dsize * 1.8f;
                            PointF[] points = {  new PointF(pointX - d, pointY - d * 2),
                                            new PointF(pointX, pointY - d),
                                            new PointF(pointX + d, pointY - d * 2),
                                            new PointF(pointX + d * 2, pointY - d),
                                            new PointF(pointX + d, pointY),
                                            new PointF(pointX + d * 2, pointY + d),
                                            new PointF(pointX + d, pointY + d * 2),
                                            new PointF(pointX, pointY + d),
                                            new PointF(pointX - d, pointY + d * 2),
                                            new PointF(pointX - d * 2, pointY + d),
                                            new PointF(pointX - d, pointY),
                                            new PointF(pointX - d * 2, pointY - d),
                                            };
                            paint.Graphics.FillPolygon(brush, points);
                            if (Math.Abs(move.PlayerIsDead[i]) >= 20)
                            {
                                using (var pen = new Pen(move.PlayerColors[Math.Abs(move.PlayerIsDead[i]) - 20], dsize))
                                    paint.Graphics.DrawPolygon(pen, points);
                            }

                            if (move.PlayerIsDead[i] > 0)
                                paint.Graphics.DrawString(move.PlayerNames[i], fnt, move.IsImpostor[i] ? Brushes.Red : Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            else if (move.IsImpostor[i])
                                paint.Graphics.DrawString("imp", fnt, (move.PlayerColors[i].ToArgb() == Color.Red.ToArgb() || move.PlayerColors[i].ToArgb() == Color.HotPink.ToArgb()) ? Brushes.Black : Brushes.Red, pointX - circleSize * 1.0f, pointY - circleSize * 0.7f);
                            if (move.PlayerIsDead[i] == 11)
                                paint.Graphics.DrawString("ejected", fnt, move.PlayerColors[i].ToArgb() == Color.Black.ToArgb() ? Brushes.Red : Brushes.Black, pointX - circleSize * 2.0f, pointY - circleSize * 0.7f);

                            if (!move.IsImpostor[i])
                            {
                                paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                                paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                            }

                        }
                    }
                }

                //draw living crew
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    if (move.IsImpostor[i])
                    {
                        AllImpostorNum++;
                        continue;
                    }
                    if (move.PlayerColors == null || move.PlayerIsDead[i] != 0) continue;

                    using (var brush = new SolidBrush(move.PlayerColors[i]))
                    {

                        int pointX = mapLocation.X + (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * mapSize.Width);
                        int pointY = mapLocation.Y + (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * mapSize.Height);


                        paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                        paint.Graphics.DrawString(move.PlayerNames[i], fnt, Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);

                        paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                        paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                    }
                }



                //DrawImp
                for (int i = 0; i < AllImpostorNum; i++)
                {
                    int id = move.ImpostorId[i];
                    if (move.PlayerColors == null || move.PlayerIsDead[id] != 0) continue;


                    using (var brush = new SolidBrush(move.PlayerColors[id]))
                    using (Pen pen = new Pen(GetCColor(move.PlayerColors[id]), dsize))
                    {
                        int pointX = mapLocation.X + (int)(((move.PlayerPoses[id].X) * map.xs + map.xp) * mapSize.Width);
                        int pointY = mapLocation.Y + (int)(((-move.PlayerPoses[id].Y) * map.ys + map.yp) * mapSize.Height);

                        if (move.InVent[i])
                        {
                            int s = (int)(circleSize * 0.8);
                            int cos = (int)(Math.Cos(Math.PI / 3) * s);
                            int sin = (int)(Math.Sin(Math.PI / 3) * s);
                            Point[] points = {
                                         new Point(pointX, pointY - s),
                                         new Point(pointX + sin, pointY + cos),
                                         new Point(pointX - sin, pointY + cos )
                                };
                            paint.Graphics.FillPolygon(brush, points);
                            paint.Graphics.DrawPolygon(pen, points);

                        }
                        else
                        {
                            paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                            paint.Graphics.DrawEllipse(pen, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);

                        }
                        paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.DarkRed, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);

                    }
                }
            }


        }
        static private Color GetCColor(Color color)
        {
            byte r = (byte)~color.R;
            byte g = (byte)~color.G;
            byte b = (byte)~color.B;

            return Color.FromArgb(r, g, b);
        }

    }
}
