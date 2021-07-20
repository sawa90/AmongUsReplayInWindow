using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AmongUsCapture;
using System.Windows.Forms;
using System.Numerics;

namespace AmongUsReplayInWindow
{
    static class DrawMove
    {
        public static bool PlayerNameVisible = true;
        public static bool TaskBarVisible = true;
        public static bool VoteVisible = true;
        public static float playerSize = 1.0f;
        public static bool drawIcon = true;
        public static Color backgroundColor = Color.Snow;
        public static Color DiscussionColor = Color.FromArgb(132, 172, 171);

        public class IconDict
        {
            public Dictionary<Color, Image> icons = null;
            public Image impostor = null;
            public Image vent = null;
            public Image dead = null;
            public Image megaphone = null;
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
                    Console.WriteLine(e);
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
                    Console.WriteLine(e);
                    vent = Properties.Resources.vent;
                }
                try
                {
                    using (FileStream stream = File.OpenRead(Program.exeFolder + "\\icon\\dead.png"))
                    {
                        dead = Image.FromStream(stream, false, false);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    dead = Properties.Resources.dead;
                }
                try
                {
                    using (FileStream stream = File.OpenRead(Program.exeFolder + "\\icon\\megaphone.png"))
                    {
                        megaphone = Image.FromStream(stream, false, false);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    megaphone = Properties.Resources.megaphone;
                }

                icons = new Dictionary<Color, Image>();
                {

                    int colorNum = PlayerData.ColorList.Length;
                    for (int i = 0; i < colorNum; i++)
                    {
                        Color color = Color.FromArgb(PlayerData.ColorList[i].ToArgb());
                        string iconName = Program.exeFolder + "\\icon\\" + ((PlayerData.PlayerColor)i).ToString() + ".png";
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
                    try
                    {
                        Color color = Color.FromArgb(Color.Empty.ToArgb());
                        string iconName = Program.exeFolder + "\\icon\\Empty.png";
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
            ~IconDict()
            {
                Dispose();
            }
            public void Dispose()
            {

                impostor?.Dispose();
                impostor = null;

                vent?.Dispose();
                vent = null;

                dead.Dispose();
                dead = null;

                megaphone?.Dispose();
                megaphone = null;

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

        static public void DrawVoting(Graphics g, PlayerMoveArgs move, List<int> deadOrderList, Map.MapScale map, IconDict icons, Point mapLocation, Size mapSize)
        {
            if (mapSize.Width == 0 || mapSize.Height == 0 || move == null || move.voteList == null) return;
            if (move.state == GameState.DISCUSSION)
            {
                bool voted = false;
                for (int i=0;i<move.PlayerNum;i++)
                {
                    sbyte vote = move.voteList[i];
                    if (move.PlayerIsDead[i]==0 && vote >= -1 && (vote < PlayerData.MaxPlayerNum || vote >= 31)) voted = true;
                }
                if (!voted) return;
            }

            string fontName = "Times New Roman";

            
            float centerX = mapLocation.X + mapSize.Width / 2;
            float centerY = mapLocation.Y + mapSize.Height / 2;
            float voteWidth = mapLocation.X + mapSize.Width / 2;
            float voteAreaX_ori = centerX - voteWidth;
            float voteAreaY_ori = mapSize.Height * 0.18f * playerSize;
            float dvoteWidth = voteWidth * 2 / 3;
            float voteHeight = Math.Min(mapSize.Height / 3, (mapSize.Height + mapLocation.Y * 2 - voteAreaY_ori) / 2);
            float dvoteHeight = voteHeight * 2 / 5.7f;
            float mainIconSize = dvoteHeight * 0.9f;
            float iconSize = mainIconSize * 0.5f;
            float dvoteX;
            float fontsize = Math.Min(mainIconSize * 0.5f, (dvoteWidth * 0.95f - mainIconSize) / 10);

            Color backcolor;
            if (move.state != GameState.DISCUSSION) backcolor = Color.FromArgb(180, 100, 100, 150);
            else backcolor = Color.FromArgb(180, 100, 100, 100);
            using (var brush = new SolidBrush(backcolor))
                g.FillRectangle(brush, voteAreaX_ori, voteAreaY_ori, voteWidth * 2, voteHeight * 2);
            Color frontcolor = Color.FromArgb(230, 255, 255, 255);
            using (var brush = new SolidBrush(frontcolor))
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    bool dead = false;
                    if (!(move.PlayerIsDead[i] == 0 || (move.PlayerIsDead[i] == 11 && move.state != GameState.DISCUSSION)))
                    {
                        if (move.voteList[i] > 20) dead = true;
                        else continue;
                    }
                    float x = voteAreaX_ori + dvoteWidth * (( i % 3) + 0.025f);
                    float y = voteAreaY_ori + dvoteHeight * ((int)(i / 3) + 0.05f);
                    if (!dead)
                        g.FillRectangle(brush, x, y, dvoteWidth * 0.95f, mainIconSize);
                    if (move.voteList[i] > 20 && icons?.megaphone != null)
                    {
                        float megaphone_w = icons.megaphone.Width / icons.megaphone.Height * mainIconSize;
                        g.DrawImage(icons.megaphone, x + dvoteWidth * 0.95f - megaphone_w, y, megaphone_w, mainIconSize);
                    }
                }

            int[] voteNum = new int[PlayerData.MaxPlayerNum + 1];
            int MaxVoteNum = 0;
            for (int i = 0; i < move.PlayerNum; i++)
            {
                if (move.PlayerIsDead[i] == 0 || (move.PlayerIsDead[i] == 11 && move.state != GameState.DISCUSSION))
                {
                    int voteId = move.voteList[i];
                    if (voteId > 20) voteId -= 32;
                    if (voteId < PlayerData.MaxPlayerNum && voteId >= 0) voteNum[voteId]++;
                }
            }
            for (int i = 0; i < move.PlayerNum; i++)
            {
                if (voteNum[i] > MaxVoteNum) MaxVoteNum = voteNum[i];
                voteNum[i] = 0;
            }
            dvoteX = Math.Min((dvoteWidth * 0.95f - mainIconSize) / MaxVoteNum, iconSize);

            using (var fnt = new Font(fontName, fontsize,GraphicsUnit.Pixel))
                for (int i = 0; i < move.PlayerNum; i++)
                {
                    bool dead = false;
                    if (!(move.PlayerIsDead[i] == 0 || (move.PlayerIsDead[i] == 11 && move.state != GameState.DISCUSSION)))
                    {
                        if (move.voteList[i] > 20) dead = true;
                        else continue;
                    }
                    using (var pColorBrush = new SolidBrush(move.PlayerColors[i]))
                    {
                        Image icon = null;
                        if (drawIcon) icon = icons?.icons?.GetValueOrDefault(move.PlayerColors[i]);


                        float icon_w = 1;
                        float icon_h = 1;
                        if (icon != null)
                        {
                            icon_w = icon.Width;
                            icon_h = icon.Height;
                            if (icon_w > icon_h)
                            {
                                icon_h = icon_h / icon_w;
                                icon_w = 1;
                            }
                            else
                            {
                                icon_w = icon_w / icon_h;
                                icon_h = 1;
                            }
                        }
                        float x = voteAreaX_ori + dvoteWidth * ((i % 3) + 0.025f);
                        float y = voteAreaY_ori + dvoteHeight * ((int)(i / 3) + 0.05f);
                        int voteId = move.voteList[i];
                        if (voteId > 20) voteId -= 32;

                        if (move.IsImpostor[i])
                        {
                            if (icon != null)
                            {
                                g.DrawImage(icons.impostor, x, y, icon_w * mainIconSize, icon_h * mainIconSize);
                                g.DrawImage(icon, x, y, icon_w * mainIconSize, icon_h * mainIconSize);
                            }
                            else
                                g.FillRectangle(pColorBrush, x, y, icon_w * mainIconSize, icon_h * mainIconSize);
                            g.DrawString(move.PlayerNames[i], fnt, Brushes.Red, x + mainIconSize, y);
                        }
                        else
                        {
                            if (icon != null)
                                g.DrawImage(icon, x, y, icon_w * mainIconSize, icon_h * mainIconSize);
                            else
                                g.FillRectangle(pColorBrush, x, y, icon_w * mainIconSize, icon_h * mainIconSize);
                            g.DrawString(move.PlayerNames[i], fnt, Brushes.Black, x + mainIconSize, y);
                        }
                        if (dead) continue;
                        if (move.PlayerIsDead[i] == 11 && icons?.dead != null) g.DrawImage(icons.dead, x, y, icon_w * mainIconSize, icon_h * mainIconSize);
                        if (voteId >= PlayerData.MaxPlayerNum)
                        {
                            if (voteId != PlayerData.notVote)
                                Console.WriteLine($"{move.PlayerNames[i]}/{move.PlayerColors[i]}->Error ID:{voteId}");
                        }
                        else if (voteId >= 0)
                        {
                            float voteX = voteAreaX_ori + dvoteWidth * ((voteId % 3) + 0.025f) + dvoteX * voteNum[voteId] + mainIconSize;
                            float voteY = voteAreaY_ori + dvoteHeight * ((int)(voteId / 3))+ mainIconSize - iconSize;
                            if (icon != null)
                            {
                                if (move.IsImpostor[i])
                                    g.DrawImage(icons.impostor, voteX, voteY, icon_w * iconSize, icon_h * iconSize);
                                g.DrawImage(icon, voteX, voteY, icon_w * iconSize, icon_h * iconSize);
                            }
                            else
                                g.FillRectangle(pColorBrush, voteX, voteY, icon_w * iconSize, icon_h * iconSize);

                            voteNum[voteId]++;
                        }
                        else if (voteId == -3)
                        {
                            float voteX = voteAreaX_ori + dvoteX * voteNum[PlayerData.MaxPlayerNum];
                            float voteY = voteAreaY_ori + dvoteHeight * 5.05f;
                            if (icon != null)
                            {
                                if (move.IsImpostor[i])
                                    g.DrawImage(icons.impostor, voteX, voteY, icon_w * iconSize, icon_h * iconSize);
                                g.DrawImage(icon, voteX, voteY, icon_w * iconSize, icon_h * iconSize);
                            }
                            else
                                g.FillRectangle(pColorBrush, voteX, voteY, icon_w * iconSize, icon_h * iconSize);
                            voteNum[PlayerData.MaxPlayerNum]++;
                        }
                    }
                }
        }


        static public void DrawMove_Icon(PaintEventArgs paint, PlayerMoveArgs move, List<int> deadOrderList, Map.MapScale map, IconDict icons, Point mapLocation, Size mapSize)
        {
            if (mapSize.Width == 0 || mapSize.Height == 0) return;
            paint.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            string fontName = "Times New Roman";
            if (move == null) return;
            int AllImpostorNum = 0;

            float circleSize = mapSize.Height / 39.0f * playerSize;
            float iconSize = mapSize.Height / 45.0f * playerSize;
            float dsize = Math.Max(1, circleSize / 5.0f);
            using (var fnt = new Font(fontName, circleSize*1.2f, GraphicsUnit.Pixel))
            {
                int minutes = move.time / 60000;
                int seconds = move.time / 1000 - minutes * 60;

                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 4.5f, circleSize * 18, circleSize * 1.2f);
                paint.Graphics.FillRectangle(Brushes.White, 0, circleSize * 5.7f, circleSize * 8, circleSize * 1.2f);
                if (move.Sabotage.TaskType != TaskTypes.SubmitScan && move.state == GameState.TASKS)
                    using (var fnt2 = new Font(fontName, circleSize*1.1f, FontStyle.Bold, GraphicsUnit.Pixel))
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

            if (map.Id != 1)
            {
                using (Pen pen = new Pen(Color.Black, dsize * 3))
                using (Pen pen2 = new Pen(Color.White, dsize * 2))
                {
                    float[][] doors;
                    switch (map.Id)
                    {
                        case 0:
                        case 3:
                            doors = Doors.skeld.Doors;
                            break;
                        case 2:
                            doors = Doors.polus.Doors;
                            break;
                        case 4:
                        default:
                            doors = Doors.airship.Doors;
                            break;
                    }
                    uint doorsUint = move.doorsUint;
                    int doorNum = doors.Length;
                    for (int i = 0; i < doorNum; i++)
                    {
                        if ((doorsUint >> i & 1) != 0)
                        {
                            float[] point = doors[i];

                            paint.Graphics.DrawLine(pen, mapLocation.X + (point[0] * map.xs + map.xp) * mapSize.Width, mapLocation.Y + (-point[1] * map.ys + map.yp) * mapSize.Height, mapLocation.X + (point[2] * map.xs + map.xp) * mapSize.Width, mapLocation.Y + (-point[3] * map.ys + map.yp) * mapSize.Height);
                            paint.Graphics.DrawLine(pen2, mapLocation.X + (point[0] * map.xs + map.xp) * mapSize.Width, mapLocation.Y + (-point[1] * map.ys + map.yp) * mapSize.Height, mapLocation.X + (point[2] * map.xs + map.xp) * mapSize.Width, mapLocation.Y + (-point[3] * map.ys + map.yp) * mapSize.Height);

                        }
                    }
                    if (map.Id == 4)
                    {
                        using (Pen pen3 = new Pen(Color.Black, dsize * 5))
                        using (Pen pen4 = new Pen(Color.LightGray, dsize * 4))
                            for (int i = doorNum - 2; i < doorNum; i++)
                        {
                            if ((doorsUint >> i & 1) != 0)
                            {
                                float[] point = doors[i];

                                paint.Graphics.DrawLine(pen3, mapLocation.X + (point[0] * map.xs + map.xp) * mapSize.Width, mapLocation.Y + (-point[1] * map.ys + map.yp) * mapSize.Height, mapLocation.X + (point[2] * map.xs + map.xp) * mapSize.Width, mapLocation.Y + (-point[3] * map.ys + map.yp) * mapSize.Height);
                                paint.Graphics.DrawLine(pen4, mapLocation.X + (point[0] * map.xs + map.xp) * mapSize.Width, mapLocation.Y + (-point[1] * map.ys + map.yp) * mapSize.Height, mapLocation.X + (point[2] * map.xs + map.xp) * mapSize.Width, mapLocation.Y + (-point[3] * map.ys + map.yp) * mapSize.Height);

                            }
                        }
                    }
                }
            }



            using (var fnt = new Font(fontName, circleSize * 0.9f, FontStyle.Bold, GraphicsUnit.Pixel))
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
                    if (Program.testflag) move.PlayerColors[i] = Color.FromArgb(move.PlayerColors[i].ToArgb());
                    if (move.IsImpostor[i] || move.PlayerColors == null || move.PlayerIsDead[i] != 0) continue;

                    using (var brush = new SolidBrush(move.PlayerColors[i]))
                    {

                        int pointX = mapLocation.X + (int)(((move.PlayerPoses[i].X) * map.xs + map.xp) * mapSize.Width);
                        int pointY = mapLocation.Y + (int)(((-move.PlayerPoses[i].Y) * map.ys + map.yp) * mapSize.Height);

                        Image icon = null;
                        if(drawIcon)icon = icons?.icons?.GetValueOrDefault(move.PlayerColors[i]);
                        if (icon != null)
                        {
                            float icon_w = iconSize * icon.Width / icon.Height;
                            paint.Graphics.DrawImage(icon, pointX - icon_w, pointY - iconSize, icon_w * 2, iconSize * 2);
                            if (PlayerNameVisible)
                                paint.Graphics.DrawString(move.PlayerNames[i], fnt, Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.8f);
                        }
                        else
                        {
                            paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                            if (PlayerNameVisible)
                                paint.Graphics.DrawString(move.PlayerNames[i], fnt, Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);

                        }
                        if (TaskBarVisible)
                        {
                            paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                            paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                        }
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
                        Image icon = null;
                        if (drawIcon) icon = icons?.icons?.GetValueOrDefault(move.PlayerColors[id]);
                        if (move.InVent[i])
                        {
                            if (icon != null)
                            {
                                float icon_w = iconSize * icon.Width / icon.Height;
                                paint.Graphics.DrawImage(icons.vent, pointX - icon_w, pointY - iconSize, icon_w * 2, iconSize * 2);
                                paint.Graphics.DrawImage(icon, pointX - icon_w * 0.8f, pointY - iconSize * 0.6f, icon_w * 1.6f, iconSize * 1.6f);
                                if (PlayerNameVisible)
                                    paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 2.0f);
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
                                if (PlayerNameVisible)
                                    paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
                            }
                        }
                        else
                        {
                            if (icon != null)
                            {
                                float icon_w = iconSize * icon.Width / icon.Height;
                                paint.Graphics.DrawImage(icons.impostor, pointX - icon_w, pointY - iconSize, icon_w * 2, iconSize * 2);
                                paint.Graphics.DrawImage(icon, pointX - icon_w, pointY - iconSize, icon_w * 2, iconSize * 2);
                                if (PlayerNameVisible)
                                    paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 1.8f);
                            }
                            else
                            {
                                paint.Graphics.FillEllipse(brush, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                                paint.Graphics.DrawEllipse(pen, pointX - circleSize / 2, pointY - circleSize / 2, circleSize, circleSize);
                                if (PlayerNameVisible)
                                    paint.Graphics.DrawString(move.PlayerNames[id], fnt, Brushes.Red, pointX - circleSize * 1.5f, pointY - circleSize * 1.5f);
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

                            if (move.PlayerIsDead[i] == 11)
                            {
                                pointX = mapLocation.X + (int)(((map.centerOfTable.X) * map.xs + map.xp) * mapSize.Width);
                                pointY = mapLocation.Y + (int)(((-map.centerOfTable.Y) * map.ys + map.yp) * mapSize.Height);
                            }
                            else if (move.PlayerIsDead[i] == -10)
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

                            if (move.PlayerIsDead[i] > 0 && PlayerNameVisible)
                                paint.Graphics.DrawString(move.PlayerNames[i], fnt, move.IsImpostor[i] ? Brushes.Red : Brushes.Black, pointX - circleSize * 1.5f, pointY - circleSize * 1.8f);
                            else if (move.IsImpostor[i])
                                paint.Graphics.DrawString("imp", fnt, (move.PlayerColors[i].ToArgb() == Color.Red.ToArgb() || move.PlayerColors[i].ToArgb() == Color.HotPink.ToArgb()) ? Brushes.Black : Brushes.Red, pointX - circleSize * 1.0f, pointY - circleSize * 0.7f);
                            if (move.PlayerIsDead[i] == 11)
                                paint.Graphics.DrawString("ejected", fnt, move.PlayerColors[i].ToArgb() == Color.Black.ToArgb() ? Brushes.Red : Brushes.Black, pointX - circleSize * 2.0f, pointY - circleSize * 0.7f);

                            if (!move.IsImpostor[i] && TaskBarVisible)
                            {
                                paint.Graphics.FillRectangle(Brushes.Gray, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2, circleSize * 0.3f);
                                paint.Graphics.FillRectangle(Brushes.Lime, pointX - circleSize, pointY + circleSize * 0.6f, circleSize * 2 * move.TaskProgress[i], circleSize * 0.3f);
                            }

                        }
                    }
                }

            }
            if (VoteVisible &&(move.state == GameState.DISCUSSION || move.state == GameState.VotingResult || move.state == GameState.HumansWinByVote || move.state == GameState.ImpostorWinByVote))
                DrawVoting(paint.Graphics, move, deadOrderList, map, icons, mapLocation, mapSize);
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
