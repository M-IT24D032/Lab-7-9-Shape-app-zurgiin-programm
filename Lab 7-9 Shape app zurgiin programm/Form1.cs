using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Lab_7_9_Shape_app_zurgiin_programm
{
    public partial class Form1 : Form
    {
        enum Tool
        {
            Pencil, Line, Eraser,
            Square, RightTriangle, Rectangle, Pentagon, Ellipse, Circle,
            Hexagon, Rhombus, Trapezoid, Heart, Star, Arrow, IsoTriangle
        }

        Bitmap bmp;
        Tool tool = Tool.Pencil;
        Color penColor = Color.Black;
        int penSize = 3;
        bool drawing = false;
        Point startPt, lastPt, curPt;

        readonly Stack<Bitmap> undoStack = new();
        readonly Stack<Bitmap> redoStack = new();

        ToolStripStatusLabel lblStatus, lblSize, lblColor;
        string colorName = "Хар";

        public Form1()
        {
            InitializeComponent();
            Text = "Зураг зурах программ";
            BuildMenu();
            BuildStatusBar();
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

            // ---- Файл ----
            var file = new ToolStripMenuItem("Файл");
            file.DropDownItems.Add(Item("Шинэ", Keys.Control | Keys.N, (s, e) => NewCanvas()));
            file.DropDownItems.Add(Item("Хадгалах...", Keys.Control | Keys.S, (s, e) => SaveImage()));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(Item("Гарах", Keys.Alt | Keys.F4, (s, e) => Close()));

            // ---- Хэрэгсэл ----
            var tools = new ToolStripMenuItem("Хэрэгсэл");
            AddTool(tools, "Харандаа", Tool.Pencil);
            AddTool(tools, "Шулуун", Tool.Line);
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
            edit.DropDownItems.Add(Item("Буцаах", Keys.Control | Keys.Z, (s, e) => Undo()));
            edit.DropDownItems.Add(Item("Дахин хийх", Keys.Control | Keys.Y, (s, e) => Redo()));
            edit.DropDownItems.Add(Item("Цэвэрлэх", Keys.Control | Keys.Delete, (s, e) => ClearCanvas()));

            // ---- Хэмжээ (зураасны зузаан 1-12) ----
            var size = new ToolStripMenuItem("Хэмжээ");
            for (int n = 1; n <= 12; n++)
            {
                int v = n;
                size.DropDownItems.Add(Item($"{v} px", Keys.None, (s, e) => SetSize(v)));
            }

            // ---- Өнгө ----
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
            color.DropDownItems.Add(Item("Бусад өнгө...", Keys.None, (s, e) =>
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                    SetColor(colorDialog1.Color, "Сонгосон");
            }));

            menuStrip1.Items.AddRange(new ToolStripItem[] { file, tools, shape, edit, size, color });
        }

        ToolStripMenuItem Item(string text, Keys shortcut, EventHandler click)
        {
            var it = new ToolStripMenuItem(text);
            if (shortcut != Keys.None) it.ShortcutKeys = shortcut;
            it.Click += click;
            return it;
        }

        void AddTool(ToolStripMenuItem parent, string name, Tool t)
        {
            parent.DropDownItems.Add(Item(name, Keys.None, (s, e) => SetTool(t, name)));
        }

        void AddColor(ToolStripMenuItem parent, string name, Color c)
        {
            var icon = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(icon))
            {
                g.Clear(c);
                g.DrawRectangle(Pens.Gray, 0, 0, 15, 15);
            }
            var it = new ToolStripMenuItem(name, icon);
            it.Click += (s, e) => SetColor(c, name);
            parent.DropDownItems.Add(it);
        }

        // ================== СТАТУС МӨР ==================
        void BuildStatusBar()
        {
            statusStrip1.Items.Clear();
            lblStatus = new ToolStripStatusLabel("Бэлэн") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            lblSize = new ToolStripStatusLabel();
            lblColor = new ToolStripStatusLabel();
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, lblSize, lblColor });
            UpdateStatus();
        }

        void UpdateStatus()
        {
            lblSize.Text = $"Зураасны хэмжээ: {penSize} px";
            lblColor.Text = $"Өнгө: {colorName}";
        }

        void SetTool(Tool t, string name)
        {
            tool = t;
            lblStatus.Text = "Хэрэгсэл: " + name;
        }

        void SetSize(int n) { penSize = n; UpdateStatus(); }

        void SetColor(Color c, string name)
        {
            penColor = c;
            colorName = name;
            UpdateStatus();
        }

        // ================== ЗУРАГ ==================
        void NewCanvas()
        {
            int w = Math.Max(pictureBox1.Width, 100);
            int h = Math.Max(pictureBox1.Height, 100);
            bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp)) g.Clear(Color.White);
            pictureBox1.Image = bmp;
            undoStack.Clear();
            redoStack.Clear();
        }

        void PushUndo()
        {
            undoStack.Push((Bitmap)bmp.Clone());
            redoStack.Clear();
        }

        void Undo()
        {
            if (undoStack.Count == 0) return;
            redoStack.Push((Bitmap)bmp.Clone());
            bmp = undoStack.Pop();
            pictureBox1.Image = bmp;
        }

        void Redo()
        {
            if (redoStack.Count == 0) return;
            undoStack.Push((Bitmap)bmp.Clone());
            bmp = redoStack.Pop();
            pictureBox1.Image = bmp;
        }

        void ClearCanvas()
        {
            PushUndo();
            using (var g = Graphics.FromImage(bmp)) g.Clear(Color.White);
            pictureBox1.Invalidate();
        }

        void SaveImage()
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp",
                FileName = "zurag"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            var fmt = sfd.FilterIndex switch
            {
                2 => ImageFormat.Jpeg,
                3 => ImageFormat.Bmp,
                _ => ImageFormat.Png
            };
            bmp.Save(sfd.FileName, fmt);
            lblStatus.Text = "Хадгаллаа: " + sfd.FileName;
        }

        Pen MakePen()
        {
            var p = tool == Tool.Eraser
                ? new Pen(Color.White, penSize * 4)
                : new Pen(penColor, penSize);
            p.StartCap = p.EndCap = LineCap.Round;
            p.LineJoin = LineJoin.Round;
            return p;
        }

        // ================== ДҮРСҮҮД ==================
        static Rectangle MakeRect(Point a, Point b) =>
            new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

        // Тал тэнцүү дөрвөлжин (дөрвөлжин, тойрогт хэрэглэнэ)
        static Rectangle SquareRect(Point a, Point b)
        {
            int d = Math.Max(Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
            int x = b.X >= a.X ? a.X : a.X - d;
            int y = b.Y >= a.Y ? a.Y : a.Y - d;
            return new Rectangle(x, y, d, d);
        }

        bool IsShapeTool() => tool != Tool.Pencil && tool != Tool.Eraser;

        // Тэнцүү талт олон өнцөгт (5, 6 өнцөгт)
        static PointF[] RegularPolygon(Rectangle r, int n)
        {
            float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f;
            float rx = r.Width / 2f, ry = r.Height / 2f;
            var pts = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                double ang = -Math.PI / 2 + 2 * Math.PI * i / n;
                pts[i] = new PointF(cx + rx * (float)Math.Cos(ang), cy + ry * (float)Math.Sin(ang));
            }
            return pts;
        }

        static PointF[] StarPoints(Rectangle r)
        {
            float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f;
            float rx = r.Width / 2f, ry = r.Height / 2f;
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double ang = -Math.PI / 2 + Math.PI * i / 5;
                float k = i % 2 == 0 ? 1f : 0.4f;
                pts[i] = new PointF(cx + k * rx * (float)Math.Cos(ang), cy + k * ry * (float)Math.Sin(ang));
            }
            return pts;
        }

        static PointF[] HeartPoints(Rectangle r)
        {
            const int N = 100;
            var raw = new PointF[N];
            for (int i = 0; i < N; i++)
            {
                double t = 2 * Math.PI * i / N;
                double x = 16 * Math.Pow(Math.Sin(t), 3);
                double y = 13 * Math.Cos(t) - 5 * Math.Cos(2 * t) - 2 * Math.Cos(3 * t) - Math.Cos(4 * t);
                raw[i] = new PointF((float)x, (float)-y); // y-г эргүүлнэ
            }
            float minX = raw.Min(p => p.X), maxX = raw.Max(p => p.X);
            float minY = raw.Min(p => p.Y), maxY = raw.Max(p => p.Y);
            var pts = new PointF[N];
            for (int i = 0; i < N; i++)
            {
                pts[i] = new PointF(
                    r.X + (raw[i].X - minX) / (maxX - minX) * r.Width,
                    r.Y + (raw[i].Y - minY) / (maxY - minY) * r.Height);
            }
            return pts;
        }

        static PointF[] ArrowPoints(Rectangle r)
        {
            float l = r.Left, t = r.Top, w = r.Width, h = r.Height;
            float cy = t + h / 2f, neck = l + w * 0.6f;
            return new[]
            {
                new PointF(l, t + h / 4f),
                new PointF(neck, t + h / 4f),
                new PointF(neck, t),
                new PointF(l + w, cy),
                new PointF(neck, t + h),
                new PointF(neck, t + 3 * h / 4f),
                new PointF(l, t + 3 * h / 4f)
            };
        }

        void DrawShape(Graphics g, Point a, Point b)
        {
            using var pen = MakePen();
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle r = MakeRect(a, b);
            float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f;

            switch (tool)
            {
                case Tool.Line:
                    g.DrawLine(pen, a, b);
                    break;
                case Tool.Rectangle:
                    g.DrawRectangle(pen, r);
                    break;
                case Tool.Square:
                    g.DrawRectangle(pen, SquareRect(a, b));
                    break;
                case Tool.Ellipse:
                    g.DrawEllipse(pen, r);
                    break;
                case Tool.Circle:
                    g.DrawEllipse(pen, SquareRect(a, b));
                    break;
                case Tool.RightTriangle:
                    g.DrawPolygon(pen, new[]
                    {
                        new PointF(r.Left, r.Top),
                        new PointF(r.Left, r.Bottom),
                        new PointF(r.Right, r.Bottom)
                    });
                    break;
                case Tool.IsoTriangle:
                    g.DrawPolygon(pen, new[]
                    {
                        new PointF(cx, r.Top),
                        new PointF(r.Right, r.Bottom),
                        new PointF(r.Left, r.Bottom)
                    });
                    break;
                case Tool.Pentagon:
                    g.DrawPolygon(pen, RegularPolygon(r, 5));
                    break;
                case Tool.Hexagon:
                    g.DrawPolygon(pen, RegularPolygon(r, 6));
                    break;
                case Tool.Rhombus:
                    g.DrawPolygon(pen, new[]
                    {
                        new PointF(cx, r.Top),
                        new PointF(r.Right, cy),
                        new PointF(cx, r.Bottom),
                        new PointF(r.Left, cy)
                    });
                    break;
                case Tool.Trapezoid:
                    g.DrawPolygon(pen, new[]
                    {
                        new PointF(r.Left + r.Width / 4f, r.Top),
                        new PointF(r.Right - r.Width / 4f, r.Top),
                        new PointF(r.Right, r.Bottom),
                        new PointF(r.Left, r.Bottom)
                    });
                    break;
                case Tool.Star:
                    g.DrawPolygon(pen, StarPoints(r));
                    break;
                case Tool.Heart:
                    if (r.Width > 1 && r.Height > 1) g.DrawPolygon(pen, HeartPoints(r));
                    break;
                case Tool.Arrow:
                    g.DrawPolygon(pen, ArrowPoints(r));
                    break;
            }
        }

        // ================== ХУЛГАНА ==================
        void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            PushUndo();
            drawing = true;
            startPt = lastPt = curPt = e.Location;
        }

        void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!drawing) return;
            curPt = e.Location;
            if (tool == Tool.Pencil || tool == Tool.Eraser)
            {
                using var g = Graphics.FromImage(bmp);
                using var pen = MakePen();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLine(pen, lastPt, curPt);
                lastPt = curPt;
            }
            pictureBox1.Invalidate();
        }

        void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!drawing) return;
            drawing = false;
            curPt = e.Location;
            if (IsShapeTool())
            {
                using var g = Graphics.FromImage(bmp);
                DrawShape(g, startPt, curPt);
            }
            pictureBox1.Invalidate();
        }

        // Дүрсийг чирч байх үед урьдчилан харуулна
        void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (drawing && IsShapeTool())
                DrawShape(e.Graphics, startPt, curPt);
        }
    }
}