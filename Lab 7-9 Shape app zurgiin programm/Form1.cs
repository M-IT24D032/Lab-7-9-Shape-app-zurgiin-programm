using System.Drawing.Drawing2D;

namespace Lab_7_9_Shape_app_zurgiin_programm
{
    public partial class Form1 : Form
    {
        enum Tool { Pencil, Line, Rectangle, Ellipse, Eraser }

        Bitmap bmp;
        Tool tool = Tool.Pencil;
        Color penColor = Color.Black;
        int penSize = 3;
        bool drawing = false;
        Point startPt, lastPt, curPt;

        public Form1()
        {
            InitializeComponent();
            Text = "Mini Paint - Зураг зурах программ";
            BuildMenu();
            Load += (s, e) => NewCanvas();
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
        }

        // ================== ЦЭС ==================
        void BuildMenu()
        {
            menuStrip1.Items.Clear();

            var file = new ToolStripMenuItem("Файл");
            file.DropDownItems.Add(Item("Шинэ", Keys.Control | Keys.N, (s, e) => NewCanvas()));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(Item("Гарах", Keys.Alt | Keys.F4, (s, e) => Close()));

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
                    penColor = colorDialog1.Color;
            }));

            menuStrip1.Items.AddRange(new ToolStripItem[] { file, color });
        }

        ToolStripMenuItem Item(string text, Keys shortcut, EventHandler click)
        {
            var it = new ToolStripMenuItem(text);
            if (shortcut != Keys.None) it.ShortcutKeys = shortcut;
            it.Click += click;
            return it;
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
            it.Click += (s, e) => penColor = c;
            parent.DropDownItems.Add(it);
        }

        // ================== ЗУРАГ ==================
        void NewCanvas()
        {
            int w = Math.Max(pictureBox1.Width, 100);
            int h = Math.Max(pictureBox1.Height, 100);
            bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp)) g.Clear(Color.White);
            pictureBox1.Image = bmp;
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

        void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
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
            drawing = false;
        }
    }
}