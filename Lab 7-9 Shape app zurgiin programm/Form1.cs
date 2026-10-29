
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lab_7_9_Shape_app_zurgiin_programm
{
    public partial class Form1 : Form
    {
        enum Tool
        {
            Pencil, Line, Rectangle, Ellipse, Eraser,
            Square, RightTriangle, Pentagon, Circle,
            Hexagon, Rhombus, Trapezoid, Heart, Star,
            Arrow, IsoTriangle
        }

        Bitmap bmp;
        Tool tool = Tool.Pencil;
        Color penColor = Color.Black;
        int penSize = 3;
        bool drawing = false;
        Point startPt, lastPt, curPt;

        Stack<Bitmap> undoStack = new Stack<Bitmap>();
        Stack<Bitmap> redoStack = new Stack<Bitmap>();

        public Form1()
        {
            InitializeComponent();
            Text = "Mini Paint - Зураг зурах программ";

            BuildMenu();

            Load += (s, e) => NewCanvas();
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.Paint += PictureBox1_Paint;
        }

        // ================== ЦЭС ==================
        void BuildMenu()
        {
            menuStrip1.Items.Clear();

            var file = new ToolStripMenuItem("Файл");
            file.DropDownItems.Add(Item("Шинэ",
                Keys.Control | Keys.N, (s, e) => NewCanvas()));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(Item("Гарах",
                Keys.Alt | Keys.F4, (s, e) => Close()));

            var tools = new ToolStripMenuItem("Хэрэгсэл");
            AddTool(tools, "Харандаа", Tool.Pencil);
            AddTool(tools, "Шулуун зураас", Tool.Line);
            AddTool(tools, "Баллуур", Tool.Eraser);

            // ---- Дүрс ----
            var shape = new ToolStripMenuItem("Дүрс");
            AddTool(shape, "Дөрвөлжин", Tool.Square);
            AddTool(shape, "Гурвалжин", Tool.RightTriangle);
            AddTool(shape, "Тэгш өнцөгт", Tool.Rectangle);
            AddTool(shape, "5 өнцөгт", Tool.Pentagon);
            AddTool(shape, "Эллипс", Tool.Ellipse);
            AddTool(shape, "Тойрог", Tool.Circle);
            AddTool(shape, "6 өнцөгт", Tool.Hexagon);
            AddTool(shape, "Ромбо", Tool.Rhombus);
            AddTool(shape, "Трапец", Tool.Trapezoid);
            AddTool(shape, "Зүрх", Tool.Heart);
            AddTool(shape, "Од", Tool.Star);
            AddTool(shape, "Сум", Tool.Arrow);
            AddTool(shape, "Адил хажуут гурвалжин", Tool.IsoTriangle);

            // ---- Засварлах ----
            var edit = new ToolStripMenuItem("Засварлах");
            edit.DropDownItems.Add(Item("Буцаах",
                Keys.Control | Keys.Z, (s, e) => Undo()));
            edit.DropDownItems.Add(Item("Дахин хийх",
                Keys.Control | Keys.Y, (s, e) => Redo()));
            edit.DropDownItems.Add(Item("Цэвэрлэх",
                Keys.Control | Keys.Delete, (s, e) => ClearCanvas()));

            var color = new ToolStripMenuItem("Өнгө");
            AddColor(color, "Хар", Color.Black);
            AddColor(color, "Улаан", Color.Red);
            AddColor(color, "Улбар шар", Color.Orange);
            AddColor(color, "Шар", Color.Gold);
            AddColor(color, "Ногоон", Color.Green);
            AddColor(color, "Цэнхэр", Color.Blue);
            AddColor(color, "Нил ягаан", Color.Purple);
            AddColor(color, "Цагаан", Color.White);
            color.DropDownItems.Add(new ToolStripSeparator());
            color.DropDownItems.Add(Item("Бусад өнгө...",
                Keys.None, (s, e) =>
                {
                    if (colorDialog1.ShowDialog() == DialogResult.OK)
                        penColor = colorDialog1.Color;
                }));

            menuStrip1.Items.AddRange(
                new ToolStripItem[] { file, tools, shape, edit, color });
        }

        ToolStripMenuItem Item(
            string text, Keys shortcut, EventHandler click)
        {
            var item = new ToolStripMenuItem(text);
            if (shortcut != Keys.None)
                item.ShortcutKeys = shortcut;
            item.Click += click;
            return item;
        }

        void AddTool(ToolStripMenuItem parent, string name, Tool selectedTool)
        {
            var item = new ToolStripMenuItem(name);
            item.Click += (s, e) => tool = selectedTool;
            parent.DropDownItems.Add(item);
        }

        void AddColor(ToolStripMenuItem parent, string name, Color c)
        {
            var icon = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(icon))
            {
                g.Clear(c);
                g.DrawRectangle(Pens.Gray, 0, 0, 15, 15);
            }

            var item = new ToolStripMenuItem(name, icon);
            item.Click += (s, e) => penColor = c;
            parent.DropDownItems.Add(item);
        }

        // ================== ЗУРАГ ==================
        void SaveUndoState()
        {
            if (bmp == null) return;

            undoStack.Push((Bitmap)bmp.Clone());

            foreach (var old in redoStack)
                old.Dispose();
            redoStack.Clear();
        }

        void RestoreBitmap(Bitmap saved)
        {
            if (bmp != null)
                bmp.Dispose();

            bmp = (Bitmap)saved.Clone();
            pictureBox1.Image = bmp;
            pictureBox1.Invalidate();
        }

        void Undo()
        {
            if (undoStack.Count == 0) return;

            redoStack.Push((Bitmap)bmp.Clone());
            Bitmap previous = undoStack.Pop();
            RestoreBitmap(previous);
            previous.Dispose();
        }

        void Redo()
        {
            if (redoStack.Count == 0) return;

            undoStack.Push((Bitmap)bmp.Clone());
            Bitmap next = redoStack.Pop();
            RestoreBitmap(next);
            next.Dispose();
        }

        void ClearCanvas()
        {
            if (bmp == null) return;

            SaveUndoState();

            using (var g = Graphics.FromImage(bmp))
                g.Clear(Color.White);

            pictureBox1.Invalidate();
        }

        void NewCanvas()
        {
            int w = Math.Max(pictureBox1.Width, 100);
            int h = Math.Max(pictureBox1.Height, 100);

            if (bmp != null)
                bmp.Dispose();

            foreach (var old in undoStack) old.Dispose();
            foreach (var old in redoStack) old.Dispose();
            undoStack.Clear();
            redoStack.Clear();

            bmp = new Bitmap(w, h);

            using (var g = Graphics.FromImage(bmp))
                g.Clear(Color.White);

            pictureBox1.Image = bmp;
            pictureBox1.Invalidate();
        }

        Pen MakePen()
        {
            var pen = tool == Tool.Eraser
                ? new Pen(Color.White, penSize * 4)
                : new Pen(penColor, penSize);

            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            pen.LineJoin = LineJoin.Round;
            return pen;
        }

        void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || bmp == null)
                return;

            drawing = true;
            startPt = lastPt = curPt = e.Location;
            SaveUndoState();
        }

        void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!drawing || bmp == null)
                return;

            curPt = e.Location;

            if (tool == Tool.Pencil || tool == Tool.Eraser)
            {
                using (var g = Graphics.FromImage(bmp))
                using (var pen = MakePen())
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawLine(pen, lastPt, curPt);
                }

                lastPt = curPt;
            }

            pictureBox1.Invalidate();
        }

        void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!drawing || bmp == null)
                return;

            curPt = e.Location;

            if (tool != Tool.Pencil && tool != Tool.Eraser)
            {
                using (var g = Graphics.FromImage(bmp))
                using (var pen = MakePen())
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    DrawSelectedShape(g, pen, startPt, curPt);
                }
            }

            drawing = false;
            pictureBox1.Invalidate();
        }

        void DrawSelectedShape(Graphics g, Pen pen, Point p1, Point p2)
        {
            Rectangle r = GetRectangle(p1, p2);

            switch (tool)
            {
                case Tool.Line:
                    g.DrawLine(pen, p1, p2);
                    break;
                case Tool.Rectangle:
                    g.DrawRectangle(pen, r);
                    break;
                case Tool.Square:
                    r.Width = r.Height = Math.Min(r.Width, r.Height);
                    g.DrawRectangle(pen, r);
                    break;
                case Tool.Ellipse:
                    g.DrawEllipse(pen, r);
                    break;
                case Tool.Circle:
                    r.Width = r.Height = Math.Min(r.Width, r.Height);
                    g.DrawEllipse(pen, r);
                    break;
                case Tool.RightTriangle:
                    g.DrawPolygon(pen, new Point[] {
                        new Point(r.Left, r.Bottom),
                        new Point(r.Left, r.Top),
                        new Point(r.Right, r.Bottom)
                    });
                    break;
                case Tool.IsoTriangle:
                    g.DrawPolygon(pen, new Point[] {
                        new Point(r.Left + r.Width / 2, r.Top),
                        new Point(r.Left, r.Bottom),
                        new Point(r.Right, r.Bottom)
                    });
                    break;
                case Tool.Pentagon:
                    g.DrawPolygon(pen, RegularPolygon(r, 5));
                    break;
                case Tool.Hexagon:
                    g.DrawPolygon(pen, RegularPolygon(r, 6));
                    break;
                case Tool.Rhombus:
                    g.DrawPolygon(pen, new Point[] {
                        new Point(r.Left + r.Width / 2, r.Top),
                        new Point(r.Right, r.Top + r.Height / 2),
                        new Point(r.Left + r.Width / 2, r.Bottom),
                        new Point(r.Left, r.Top + r.Height / 2)
                    });
                    break;
                case Tool.Trapezoid:
                    g.DrawPolygon(pen, new Point[] {
                        new Point(r.Left + r.Width / 4, r.Top),
                        new Point(r.Right - r.Width / 4, r.Top),
                        new Point(r.Right, r.Bottom),
                        new Point(r.Left, r.Bottom)
                    });
                    break;
                case Tool.Heart:
                    g.DrawPolygon(pen, new Point[] {
                        new Point(r.Left + r.Width / 2, r.Bottom),
                        new Point(r.Left, r.Top + r.Height / 3),
                        new Point(r.Left + r.Width / 8, r.Top + r.Height / 8),
                        new Point(r.Left + r.Width / 3, r.Top),
                        new Point(r.Left + r.Width / 2, r.Top + r.Height / 4),
                        new Point(r.Left + r.Width * 2 / 3, r.Top),
                        new Point(r.Right - r.Width / 8, r.Top + r.Height / 8),
                        new Point(r.Right, r.Top + r.Height / 3)
                    });
                    break;
                case Tool.Star:
                    g.DrawPolygon(pen, StarPoints(r));
                    break;
                case Tool.Arrow:
                    g.DrawPolygon(pen, new Point[] {
                        new Point(r.Left, r.Top + r.Height / 3),
                        new Point(r.Left + r.Width * 2 / 3, r.Top + r.Height / 3),
                        new Point(r.Left + r.Width * 2 / 3, r.Top),
                        new Point(r.Right, r.Top + r.Height / 2),
                        new Point(r.Left + r.Width * 2 / 3, r.Bottom),
                        new Point(r.Left + r.Width * 2 / 3, r.Top + r.Height * 2 / 3),
                        new Point(r.Left, r.Top + r.Height * 2 / 3)
                    });
                    break;
            }
        }

        Point[] RegularPolygon(Rectangle r, int sides)
        {
            Point[] points = new Point[sides];
            double cx = r.Left + r.Width / 2.0;
            double cy = r.Top + r.Height / 2.0;

            for (int i = 0; i < sides; i++)
            {
                double angle = -Math.PI / 2 + 2 * Math.PI * i / sides;
                points[i] = new Point(
                    (int)(cx + r.Width / 2.0 * Math.Cos(angle)),
                    (int)(cy + r.Height / 2.0 * Math.Sin(angle)));
            }

            return points;
        }

        Point[] StarPoints(Rectangle r)
        {
            Point[] points = new Point[10];
            double cx = r.Left + r.Width / 2.0;
            double cy = r.Top + r.Height / 2.0;

            for (int i = 0; i < 10; i++)
            {
                double angle = -Math.PI / 2 + i * Math.PI / 5;
                double scale = i % 2 == 0 ? 1.0 : 0.42;

                points[i] = new Point(
                    (int)(cx + r.Width / 2.0 * scale * Math.Cos(angle)),
                    (int)(cy + r.Height / 2.0 * scale * Math.Sin(angle)));
            }

            return points;
        }

        Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Max(1, Math.Abs(p2.X - p1.X)),
                Math.Max(1, Math.Abs(p2.Y - p1.Y)));
        }

        void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (!drawing || bmp == null ||
                tool == Tool.Pencil || tool == Tool.Eraser)
                return;

            using (var pen = MakePen())
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawSelectedShape(e.Graphics, pen, startPt, curPt);
            }
        }
    }
}
