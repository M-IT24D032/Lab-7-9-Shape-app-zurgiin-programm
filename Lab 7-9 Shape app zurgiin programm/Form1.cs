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
            Load += (s, e) => NewCanvas();
        }

        void NewCanvas()
        {
            int w = Math.Max(pictureBox1.Width, 100);
            int h = Math.Max(pictureBox1.Height, 100);
            bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp)) g.Clear(Color.White);
            pictureBox1.Image = bmp;
        }
    }
}