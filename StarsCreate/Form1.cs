using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace StarsCreate
{
    public partial class Form1 : Form
    {
        private int[,] PixValue;
        private int h, w;
        private Random rd;
        private Bitmap bm;
        private Bitmap Imbm;

        /// <summary>
        /// Функция распределения Гаусса
        /// </summary>
        /// <param name="x">Текущая координата</param>
        /// <param name="d">Ширина расплёска</param>
        /// <param name="u">Положение максимума энергии</param>
        /// <returns></returns>
        private static double Gauss(double x, double d, double u)
        {
            double a = 1 / (d * Math.Sqrt(2 * Math.PI));
            double e = Math.Exp(-(Math.Pow(x - u, 2) / (2 * d * d)));
            return a * e;
        }

        public Form1()
        {
            InitializeComponent();
            textBox3.Text = trackBar1.Value.ToString();
            rd = new Random();
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            textBox3.Text = trackBar1.Value.ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            h = Convert.ToInt32(textBox1.Text);
            w = Convert.ToInt32(textBox2.Text);
            PixValue = new int[w, h];
            bm = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
            Imbm = new Bitmap(w, h);
            ValToBitMap();

            //pictureBox1.Image = Image.FromHbitmap(bm.GetHbitmap());
            pictureBox1.Image = Imbm;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            int x, y;
            x = rd.Next(Convert.ToInt32(bm.Width / 10), bm.Width);
            y = rd.Next(Convert.ToInt32(bm.Height / 10), bm.Height);
            int r = 10;

            //for (int i = 1; i <= 10; i++)
            //   Imbm.V_MIcirc(x, y, i, 255 - (i));
            //Imbm.V_Brezenh(x, y, 10, 255);

            using (Graphics gr = Graphics.FromImage(Imbm))
            {
                Pen WPwn = new Pen(Color.White, 1);
                gr.DrawEllipse(WPwn, new Rectangle(x - r, y - r, 2 * r, 2 * r));
            }

            pictureBox1.Image = Imbm;

            //bm.Save16bitBitmapToPng("D:\\1.png");
        }

        private void ValToBitMap()
        {
            for (int i = 0; i < Imbm.Height; i++)
            {
                for (int j = 0; j < Imbm.Width; j++)
                {
                    Imbm.SetPixel(j, i, Color.Black);
                }
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox1.Image = Imbm;
            bm.Save16bitBitmapToPng("D:\\1.png");
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            int x, y;
            x = rd.Next(1, bm.Width);
            y = rd.Next(1, bm.Height);
            int r = 10;

            for (int i = 1; i <= r; i++)
            {
                Imbm.V_MIcirc(x, y, i, 255);
                bm.V_MIcirc16(x, y, i, 65000);
            }
            //for (int i = 1; i <= r; i++)
            //    using (Graphics gr = Graphics.FromImage(Imbm))
            //    {
            //        Pen WPwn = new Pen(Color.White, 1);
            //        gr.DrawEllipse(WPwn, new Rectangle(x - i, y - i, 2 * i, 2 * i));
            //    }

            pictureBox1.Image = Imbm;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                // выключение таймера
                button4.Text = "Начать генерацию";
            }
            else
            {
                // включение таймера
                button4.Text = "Остановить генерацию";
            }

            timer1.Enabled = !timer1.Enabled;
        }

        private void ValToBitMap16()
        {
            for (int i = 0; i < bm.Height; i++)
            {
                for (int j = 0; j < bm.Width; j++)
                {
                    bm.SetPixelFor16bit(j, i, (ushort)(i * j * 65556 / bm.Width / bm.Height));
                }
            }
            //Imbm = new Bitmap(bm);
        }
    }

    public static class Ext
    {
        /// <summary>
        /// Задать значение яркости пикселя( градация серого)
        /// </summary>
        /// <param name="bmp">палитра рисования</param>
        /// <param name="x">по горизонтали</param>
        /// <param name="y">по вертикали</param>
        /// <param name="pixel">значение в диапазоне от 0 до 2^16</param>
        public static void SetPixelFor16bit(this Bitmap bmp, int x, int y, ushort pixel)
        {
            var bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            if (bd.Stride / bmp.Width != 2) throw new Exception();
            int i = bd.Stride * y + 2 * x;
            unsafe
            {
                byte* p1 = (byte*)&pixel;
                var ptr = (byte*)bd.Scan0 + i;
                *ptr = *p1;
                *++ptr = *++p1;
            }
            bmp.UnlockBits(bd);
        }

        /// <summary>
        /// Сохранить изображение в файл
        /// </summary>
        /// <param name="bmp">Сохраняемая палитра</param>
        /// <param name="file">Пусть сохранения</param>
        public static void Save16bitBitmapToPng(this Bitmap bmp, string file)
        {
            var bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            if (bd.Stride / bmp.Width != 2) throw new Exception();
            var source = BitmapSource.Create(
                bd.Width, bd.Height, bmp.HorizontalResolution, bmp.VerticalResolution,
                System.Windows.Media.PixelFormats.Gray16, null, bd.Scan0, bd.Stride * bd.Height, bd.Stride);
            bmp.UnlockBits(bd);
            using (var stream = new FileStream(file, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
            }
        }

        public static void Pixel_circle16(this Bitmap bmp, int xc, int yc, int x, int y, int pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            x1 = xc + x < bmp.Width ? xc + x : bmp.Width - 1;
            x1 = x1 >= 0 ? x1 : 0;

            x2 = xc + y < bmp.Width ? xc + y : bmp.Width - 1;
            x2 = x2 >= 0 ? x2 : 0;

            x3 = xc - x < bmp.Width ? xc - x : bmp.Width - 1;
            x3 = x3 >= 0 ? x3 : 0;

            x4 = xc - y < bmp.Width ? xc - y : bmp.Width - 1;
            x4 = x4 >= 0 ? x4 : 0;

            y1 = yc + y < bmp.Height ? yc + y : bmp.Height - 1;
            y1 = y1 >= 0 ? y1 : 0;

            y2 = yc + x < bmp.Height ? yc + x : bmp.Height - 1;
            y2 = y2 >= 0 ? y2 : 0;

            y3 = yc - x < bmp.Height ? yc - x : bmp.Height - 1;
            y3 = y3 >= 0 ? y3 : 0;

            y4 = yc - y < bmp.Height ? yc - y : bmp.Height - 1;
            y4 = y4 >= 0 ? y4 : 0;

            ushort ttt = Convert.ToUInt16(pixel);

            bmp.SetPixelFor16bit(x1, y1, ttt);
            bmp.SetPixelFor16bit(x2, y2, ttt);
            bmp.SetPixelFor16bit(x2, y3, ttt);
            bmp.SetPixelFor16bit(x1, y4, ttt);
            bmp.SetPixelFor16bit(x3, y4, ttt);
            bmp.SetPixelFor16bit(x4, y3, ttt);
            bmp.SetPixelFor16bit(x4, y2, ttt);
            bmp.SetPixelFor16bit(x3, y1, ttt);

            //bmp.SetPixel(x1, y1, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x2, y2, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x2, y3, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x1, y4, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x3, y4, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x4, y3, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x4, y2, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x3, y1, Color.FromArgb(pixel, pixel, pixel));
        }

        public static void Pixel_circle(this Bitmap bmp, int xc, int yc, int x, int y, int pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            x1 = xc + x < bmp.Width ? xc + x : bmp.Width - 1;
            x1 = x1 >= 0 ? x1 : 0;

            x2 = xc + y < bmp.Width ? xc + y : bmp.Width - 1;
            x2 = x2 >= 0 ? x2 : 0;

            x3 = xc - x < bmp.Width ? xc - x : bmp.Width - 1;
            x3 = x3 >= 0 ? x3 : 0;

            x4 = xc - y < bmp.Width ? xc - y : bmp.Width - 1;
            x4 = x4 >= 0 ? x4 : 0;

            y1 = yc + y < bmp.Height ? yc + y : bmp.Height - 1;
            y1 = y1 >= 0 ? y1 : 0;

            y2 = yc + x < bmp.Height ? yc + x : bmp.Height - 1;
            y2 = y2 >= 0 ? y2 : 0;

            y3 = yc - x < bmp.Height ? yc - x : bmp.Height - 1;
            y3 = y3 >= 0 ? y3 : 0;

            y4 = yc - y < bmp.Height ? yc - y : bmp.Height - 1;
            y4 = y4 >= 0 ? y4 : 0;

            bmp.SetPixel(x1, y1, Color.FromArgb(pixel, pixel, pixel));
            bmp.SetPixel(x2, y2, Color.FromArgb(pixel, pixel, pixel));
            bmp.SetPixel(x2, y3, Color.FromArgb(pixel, pixel, pixel));
            bmp.SetPixel(x1, y4, Color.FromArgb(pixel, pixel, pixel));
            bmp.SetPixel(x3, y4, Color.FromArgb(pixel, pixel, pixel));
            bmp.SetPixel(x4, y3, Color.FromArgb(pixel, pixel, pixel));
            bmp.SetPixel(x4, y2, Color.FromArgb(pixel, pixel, pixel));
            bmp.SetPixel(x3, y1, Color.FromArgb(pixel, pixel, pixel));
        }

        public static void V_MIcirc(this Bitmap bmp, int xc, int yc, int r, int pixel)
        {
            int x = 0, y = r, d = 3 - 2 * r;

            while (x < y)
            {
                bmp.Pixel_circle(xc, yc, x, y, pixel);
                if (d < 0)
                    d = d + 4 * x + 6;
                else
                {
                    d = d + 4 * (x - y) + 10;
                    --y;
                }
                ++x;
            }
            if (x == y)
                bmp.Pixel_circle(xc, yc, x, y, pixel);
        }

        public static void V_MIcirc16(this Bitmap bmp, int xc, int yc, int r, int pixel)
        {
            int x = 0, y = r, d = 3 - 2 * r;

            while (x < y)
            {
                bmp.Pixel_circle16(xc, yc, x, y, pixel);
                if (d < 0)
                    d = d + 4 * x + 6;
                else
                {
                    d = d + 4 * (x - y) + 10;
                    --y;
                }
                ++x;
            }
            if (x == y)
                bmp.Pixel_circle16(xc, yc, x, y, pixel);
        }

        public static void V_Brezenh(this Bitmap bmp, int x, int y, int r, int pixel)
        {
            int x1 = 0, y1 = r, yk = 0;
            int sigma, delta;
            bool f;
            delta = 2 * (1 - r);

            int xx1, xx2, yy1, yy2;

            do
            {
                xx1 = x + x1 < bmp.Width ? x + x1 : bmp.Width - 1;
                xx1 = xx1 >= 0 ? xx1 : 0;

                xx2 = x - x1 < bmp.Width ? x - x1 : bmp.Width - 1;
                xx2 = xx2 >= 0 ? xx2 : 0;

                yy1 = y + y1 < bmp.Height ? y + y1 : bmp.Height - 1;
                yy1 = yy1 >= 0 ? yy1 : 0;

                yy2 = y - y1 < bmp.Height ? y - y1 : bmp.Height - 1;
                yy2 = yy2 >= 0 ? yy2 : 0;

                bmp.SetPixel(xx1, yy1, Color.FromArgb(pixel, pixel, pixel));
                bmp.SetPixel(xx2, yy1, Color.FromArgb(pixel, pixel, pixel));
                bmp.SetPixel(xx1, yy2, Color.FromArgb(pixel, pixel, pixel));
                bmp.SetPixel(xx2, yy2, Color.FromArgb(pixel, pixel, pixel));

                f = false;
                if (y1 < yk)
                    break;
                if (delta < 0)
                {
                    sigma = 2 * (delta + y1) - 1;
                    if (sigma <= 0)
                    {
                        x1++;
                        delta += 2 * x1 + 1;
                        f = true;
                    }
                }
                else
                    if (delta > 0)
                {
                    sigma = 2 * (delta - x1) - 1;
                    if (sigma > 0)
                    {
                        y1--;
                        delta += 1 - 2 * y1;
                        f = true;
                    }
                }
                if (!f)
                {
                    x1++;
                    y--;
                    delta += 2 * (x1 - y1 - 1);
                }
            } while (true);
        }
    }
}