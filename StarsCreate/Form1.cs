using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace StarsCreate
{
    public partial class Form1 : Form
    {
        private long[,] PixValue16; // массив значений пикселей 16 битного холста
        private long[,] PixValue;   // массив значений пикселей 8 битного холста
        private int h, w;           // размеры холста
        private Random rd;          // рандомайзер
        private Bitmap bm;          // 16 битный холст
        private Bitmap Imbm;        // 8 битный холст
        private long kolstars;      // Количество генерируемых звёзд

        // контекст синхронизации
        private SynchronizationContext uiContext;

        private Thread Thread;

        private Stopwatch st = new Stopwatch();     // для подсчёта времени

        /// <summary>
        /// Вычисляет текущее значение яркости пикселя относительно гауссового распределения
        /// </summary>
        /// <param name="Vakslokal">Максимальное значение из вычесленного</param>
        /// <param name="maxznach">Максимальное значение из нового диапазона</param>
        /// <param name="value">Текущее значение</param>
        /// <returns>Текущее значение в новом диапазоне</returns>
        private static int ValZnach(double Vakslokal, int maxznach, double value)
        {
            double t = value * maxznach / Vakslokal;
            return Convert.ToInt32(t);
        }

        private void ProcessRun(object state)
        {
            // вытащим контекст синхронизации
            SynchronizationContext uiContext = state as SynchronizationContext;
            //Stopwatch st = new Stopwatch();

            st.Start();
            for (long i = 0; i < kolstars; i++)
            {
                CretStarVal();
                uiContext.Post(UpdateUI, "WorkProcces");
            }
            //PixToBitAll();
            st.Stop();

            uiContext.Post(UpdateUI, "StopProcces");
        }

        public void UpdateUI(object state)
        {
            string text = state as string;

            switch (text)
            {
                case "WorkProcces":
                    {
                        progressBar1.PerformStep();
                        break;
                    }
                case "StopProcces":
                    {
                        progressBar1.Value = progressBar1.Maximum;
                        toolStripStatusLabel1.Text = st.Elapsed.ToString();

                        break;
                    }
            }
        }

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
            PixValue16 = new long[w, h];
            PixValue = new long[w, h];
            bm = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
            Imbm = new Bitmap(w, h);
            ValToBitMap();

            //pictureBox1.Image = Image.FromHbitmap(bm.GetHbitmap());
            pictureBox1.Image = Imbm;
        }

        /// <summary>
        /// Перенос значения массива на холст 16 битный
        /// </summary>
        private void PixToBit16()
        {
            ushort temp16;

            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    temp16 = Convert.ToUInt16(PixValue16[i, j]);
                    bm.SetPixelFor16bit(i, j, temp16);
                }
        }

        /// <summary>
        /// Перенос значения массива на холст 8 битный
        /// </summary>
        private void PixToBit8()
        {
            int temp;

            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    temp = Convert.ToInt32(PixValue[i, j]);
                    Imbm.SetPixel(i, j, Color.FromArgb(temp, temp, temp));
                }
        }

        /// <summary>
        /// Переносит значения массивов на холсты
        /// </summary>
        private void PixToBitAll()
        {
            int temp;
            ushort temp16;

            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    temp = Convert.ToInt32(PixValue[i, j]);
                    Imbm.SetPixel(i, j, Color.FromArgb(temp, temp, temp));
                    temp16 = Convert.ToUInt16(PixValue16[i, j]);
                    bm.SetPixelFor16bit(i, j, temp16);
                }
        }

        /// <summary>
        /// Подсветка пиксеья и пикселей вокругстоящих, 16бит
        /// </summary>
        /// <param name="x">координата по горизонтале</param>
        /// <param name="y">Координата по вертикале</param>
        /// <param name="pixel">Яроксть пикселя</param>
        private void Pixel_circle_cikl(int x, int y, long pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            x1 = x - 1;
            x2 = x;
            x3 = x + 1;
            x4 = x;

            y1 = y;
            y2 = y - 1;
            y3 = y;
            y4 = y + 1;

            x1 = x1 >= 0 ? x1 : 0;
            x3 = x3 < PixValue.GetLength(0) ? x3 : PixValue.GetLength(0) - 1;

            y2 = y2 >= 0 ? y2 : 0;
            y4 = y4 < PixValue.GetLength(1) ? y4 : PixValue.GetLength(1) - 1;

            //bmp.SetPixelFor16bit(x, y, ttt);
            //bmp.SetPixelFor16bit(x1, y1, ttt);
            //bmp.SetPixelFor16bit(x2, y2, ttt);
            //bmp.SetPixelFor16bit(x3, y3, ttt);
            //bmp.SetPixelFor16bit(x4, y4, ttt);

            if (PixValue[x, y] < pixel)
                PixValue[x, y] = pixel;

            if (PixValue[x1, y1] < pixel)
                PixValue[x1, y1] = pixel;

            if (PixValue[x2, y2] < pixel)
                PixValue[x2, y2] = pixel;

            if (PixValue[x3, y3] < pixel)
                PixValue[x3, y3] = pixel;

            if (PixValue[x4, y4] < pixel)
                PixValue[x4, y4] = pixel;
        }

        /// <summary>
        /// Подсветка определённых пикселей с учётом границ изображения
        /// </summary>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixel"></param>
        private void Pixel_circle(int xc, int yc, int x, int y, long pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            // Смотрим, чтоб отрисовка не вышла за границы холста
            x1 = xc + x < PixValue.GetLength(0) ? xc + x : PixValue.GetLength(0) - 1;
            x1 = x1 >= 0 ? x1 : 0;

            x2 = xc + y < PixValue.GetLength(0) ? xc + y : PixValue.GetLength(0) - 1;
            x2 = x2 >= 0 ? x2 : 0;

            x3 = xc - x < PixValue.GetLength(0) ? xc - x : PixValue.GetLength(0) - 1;
            x3 = x3 >= 0 ? x3 : 0;

            x4 = xc - y < PixValue.GetLength(0) ? xc - y : PixValue.GetLength(0) - 1;
            x4 = x4 >= 0 ? x4 : 0;

            y1 = yc + y < PixValue.GetLength(1) ? yc + y : PixValue.GetLength(1) - 1;
            y1 = y1 >= 0 ? y1 : 0;

            y2 = yc + x < PixValue.GetLength(1) ? yc + x : PixValue.GetLength(1) - 1;
            y2 = y2 >= 0 ? y2 : 0;

            y3 = yc - x < PixValue.GetLength(1) ? yc - x : PixValue.GetLength(1) - 1;
            y3 = y3 >= 0 ? y3 : 0;

            y4 = yc - y < PixValue.GetLength(1) ? yc - y : PixValue.GetLength(1) - 1;
            y4 = y4 >= 0 ? y4 : 0;

            Pixel_circle_cikl(x1, y1, pixel);
            Pixel_circle_cikl(x2, y2, pixel);
            Pixel_circle_cikl(x2, y3, pixel);
            Pixel_circle_cikl(x1, y4, pixel);
            Pixel_circle_cikl(x3, y4, pixel);
            Pixel_circle_cikl(x4, y3, pixel);
            Pixel_circle_cikl(x4, y2, pixel);
            Pixel_circle_cikl(x3, y1, pixel);
        }

        private void Pixel_circle16_cikl(int x, int y, long pixel)
        {
            ushort ttt = Convert.ToUInt16(pixel);
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            x1 = x - 1;
            x2 = x;
            x3 = x + 1;
            x4 = x;

            y1 = y;
            y2 = y - 1;
            y3 = y;
            y4 = y + 1;

            x1 = x1 >= 0 ? x1 : 0;
            x3 = x3 < PixValue16.GetLength(0) ? x3 : PixValue16.GetLength(0) - 1;

            y2 = y2 >= 0 ? y2 : 0;
            y4 = y4 < PixValue16.GetLength(1) ? y4 : PixValue16.GetLength(1) - 1;

            if (PixValue16[x, y] < pixel)
                PixValue16[x, y] = pixel;

            if (PixValue16[x1, y1] < pixel)
                PixValue16[x1, y1] = pixel;

            if (PixValue16[x2, y2] < pixel)
                PixValue16[x2, y2] = pixel;

            if (PixValue16[x3, y3] < pixel)
                PixValue16[x3, y3] = pixel;

            if (PixValue16[x4, y4] < pixel)
                PixValue16[x4, y4] = pixel;

            //bmp.SetPixelFor16bit(x, y, ttt);
            //bmp.SetPixelFor16bit(x1, y1, ttt);
            //bmp.SetPixelFor16bit(x2, y2, ttt);
            //bmp.SetPixelFor16bit(x3, y3, ttt);
            //bmp.SetPixelFor16bit(x4, y4, ttt);
        }

        /// <summary>
        /// Подсветка определённых пикселей с учётом границ изображения, 16 бит
        /// </summary>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixel"></param>
        private void Pixel_circle16(int xc, int yc, int x, int y, long pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            // Смотрим, чтоб отрисовка не вышла за границы холста
            x1 = xc + x < PixValue16.GetLength(0) ? xc + x : PixValue16.GetLength(0) - 1;
            x1 = x1 >= 0 ? x1 : 0;

            x2 = xc + y < PixValue16.GetLength(0) ? xc + y : PixValue16.GetLength(0) - 1;
            x2 = x2 >= 0 ? x2 : 0;

            x3 = xc - x < PixValue16.GetLength(0) ? xc - x : PixValue16.GetLength(0) - 1;
            x3 = x3 >= 0 ? x3 : 0;

            x4 = xc - y < PixValue16.GetLength(0) ? xc - y : PixValue16.GetLength(0) - 1;
            x4 = x4 >= 0 ? x4 : 0;

            y1 = yc + y < PixValue16.GetLength(1) ? yc + y : PixValue16.GetLength(1) - 1;
            y1 = y1 >= 0 ? y1 : 0;

            y2 = yc + x < PixValue16.GetLength(1) ? yc + x : PixValue16.GetLength(1) - 1;
            y2 = y2 >= 0 ? y2 : 0;

            y3 = yc - x < PixValue16.GetLength(1) ? yc - x : PixValue16.GetLength(1) - 1;
            y3 = y3 >= 0 ? y3 : 0;

            y4 = yc - y < PixValue16.GetLength(1) ? yc - y : PixValue16.GetLength(1) - 1;
            y4 = y4 >= 0 ? y4 : 0;

            //ushort ttt = Convert.ToUInt16(pixel);

            Pixel_circle16_cikl(x1, y1, pixel);
            Pixel_circle16_cikl(x2, y2, pixel);
            Pixel_circle16_cikl(x2, y3, pixel);
            Pixel_circle16_cikl(x1, y4, pixel);
            Pixel_circle16_cikl(x3, y4, pixel);
            Pixel_circle16_cikl(x4, y3, pixel);
            Pixel_circle16_cikl(x4, y2, pixel);
            Pixel_circle16_cikl(x3, y1, pixel);

            //bmp.SetPixelFor16bit(x1, y1, ttt);
            //bmp.SetPixelFor16bit(x2, y2, ttt);
            //bmp.SetPixelFor16bit(x2, y3, ttt);
            //bmp.SetPixelFor16bit(x1, y4, ttt);
            //bmp.SetPixelFor16bit(x3, y4, ttt);
            //bmp.SetPixelFor16bit(x4, y3, ttt);
            //bmp.SetPixelFor16bit(x4, y2, ttt);
            //bmp.SetPixelFor16bit(x3, y1, ttt);

            //bmp.SetPixel(x1, y1, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x2, y2, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x2, y3, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x1, y4, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x3, y4, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x4, y3, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x4, y2, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x3, y1, Color.FromArgb(pixel, pixel, pixel));
        }

        /// <summary>
        /// Алгоритм попиксельного рендеринга окружности, 16 бит
        /// </summary>
        /// <param name="xc">центр окружности по горизонтале</param>
        /// <param name="yc">центр окружности по вертикале</param>
        /// <param name="r">Радиус окружности</param>
        /// <param name="pixel">Яркость окраски</param>
        private void V_MIcirc16(int xc, int yc, int r, long pixel)
        {
            int x = 0, y = r, d = 3 - 2 * r;

            while (x < y)
            {
                Pixel_circle16(xc, yc, x, y, pixel);
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
                Pixel_circle16(xc, yc, x, y, pixel);
        }

        /// <summary>
        /// Алгоритм попиксельного рендеринга окружности
        /// </summary>
        /// <param name="xc">центр окружности по горизонтале</param>
        /// <param name="yc">центр окружности по вертикале</param>
        /// <param name="r">Радиус окружности</param>
        /// <param name="pixel">Яркость окраски</param>
        private void V_MIcirc(int xc, int yc, int r, long pixel)
        {
            int x = 0, y = r, d = 3 - 2 * r;

            while (x < y)
            {
                Pixel_circle(xc, yc, x, y, pixel);
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
                Pixel_circle(xc, yc, x, y, pixel);
        }

        /// <summary>
        /// Создать одну звезду в случайном месте
        /// </summary>
        private void CretStarVal()
        {
            double[] znach;
            int radius = rd.Next(1, Convert.ToInt32(Math.Min(bm.Width, bm.Height) / 100));
            int x, y;
            x = rd.Next(1, bm.Width);
            y = rd.Next(1, bm.Height);

            int radgaus = rd.Next(1, 5);

            int light = rd.Next(256, 65535);

            //for (int i = 1; i <= 10; i++)
            //   Imbm.V_MIcirc(x, y, i, 255 - (i));
            //Imbm.V_Brezenh(x, y, 10, 255);
            znach = new double[radius];

            for (int i = 0; i < radius; i++)
                znach[i] = Gauss(x + i, radgaus, x);

            for (int i = 1; i <= radius; i++)
            {
                int temp = ValZnach(znach[0], Convert.ToInt32(light / 256), znach[i - 1]);
                int xx = x + i < Imbm.Width ? x + i : Imbm.Width - 1;
                //int col = Imbm.GetPixel(xx, y).G;
                if (PixValue[xx, y] <= temp)
                {
                    V_MIcirc(x, y, i, temp);

                    temp = ValZnach(znach[0], light, znach[i - 1]);

                    V_MIcirc16(x, y, i, temp);
                }
            }

            //using (Graphics gr = Graphics.FromImage(Imbm))
            //{
            //    Pen WPwn = new Pen(Color.White, 1);
            //    gr.DrawEllipse(WPwn, new Rectangle(x - r, y - r, 2 * r, 2 * r));
            //}
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            kolstars = rd.Next(100, 500);

            //kolstars = Convert.ToInt32(Math.Pow(10, 7));
            uiContext = SynchronizationContext.Current;
            progressBar1.Maximum = Convert.ToInt32(kolstars);
            Thread = new Thread(ProcessRun);

            Thread.Start(uiContext);
            //st.Start();
            //for (long i = 0; i < kolstars; i++)
            // CretStarVal();

            //PixToBitAll();
            // st.Stop();

            //pictureBox1.Image = Imbm;
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
            CretStarVal();

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

        private void Button5_Click(object sender, EventArgs e)
        {
            PixToBitAll();
            pictureBox1.Image = Imbm;
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

        /// <summary>
        /// Подсветка пиксеья и пикселей вокругстоящих, 16бит
        /// </summary>
        /// <param name="bmp">Палитра</param>
        /// <param name="x">координата по горизонтале</param>
        /// <param name="y">Координата по вертикале</param>
        /// <param name="pixel">Яроксть пикселя</param>
        public static void Pixel_circle16_cikl(this Bitmap bmp, int x, int y, int pixel)
        {
            ushort ttt = Convert.ToUInt16(pixel);
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            x1 = x - 1;
            x2 = x;
            x3 = x + 1;
            x4 = x;

            y1 = y;
            y2 = y - 1;
            y3 = y;
            y4 = y + 1;

            x1 = x1 >= 0 ? x1 : 0;
            x3 = x3 < bmp.Width ? x3 : bmp.Width - 1;

            y2 = y2 >= 0 ? y2 : 0;
            y4 = y4 < bmp.Height ? y4 : bmp.Height - 1;

            bmp.SetPixelFor16bit(x, y, ttt);
            bmp.SetPixelFor16bit(x1, y1, ttt);
            bmp.SetPixelFor16bit(x2, y2, ttt);
            bmp.SetPixelFor16bit(x3, y3, ttt);
            bmp.SetPixelFor16bit(x4, y4, ttt);
        }

        /// <summary>
        /// Подсветка определённых пикселей с учётом границ изображения, 16 бит
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixel"></param>
        public static void Pixel_circle16(this Bitmap bmp, int xc, int yc, int x, int y, int pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            // Смотрим, чтоб отрисовка не вышла за границы холста
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

            //ushort ttt = Convert.ToUInt16(pixel);

            bmp.Pixel_circle16_cikl(x1, y1, pixel);
            bmp.Pixel_circle16_cikl(x2, y2, pixel);
            bmp.Pixel_circle16_cikl(x2, y3, pixel);
            bmp.Pixel_circle16_cikl(x1, y4, pixel);
            bmp.Pixel_circle16_cikl(x3, y4, pixel);
            bmp.Pixel_circle16_cikl(x4, y3, pixel);
            bmp.Pixel_circle16_cikl(x4, y2, pixel);
            bmp.Pixel_circle16_cikl(x3, y1, pixel);

            //bmp.SetPixelFor16bit(x1, y1, ttt);
            //bmp.SetPixelFor16bit(x2, y2, ttt);
            //bmp.SetPixelFor16bit(x2, y3, ttt);
            //bmp.SetPixelFor16bit(x1, y4, ttt);
            //bmp.SetPixelFor16bit(x3, y4, ttt);
            //bmp.SetPixelFor16bit(x4, y3, ttt);
            //bmp.SetPixelFor16bit(x4, y2, ttt);
            //bmp.SetPixelFor16bit(x3, y1, ttt);

            //bmp.SetPixel(x1, y1, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x2, y2, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x2, y3, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x1, y4, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x3, y4, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x4, y3, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x4, y2, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x3, y1, Color.FromArgb(pixel, pixel, pixel));
        }

        /// <summary>
        /// Подсветка пиксеья и пикселей вокругстоящих, 16бит
        /// </summary>
        /// <param name="bmp">Палитра</param>
        /// <param name="x">координата по горизонтале</param>
        /// <param name="y">Координата по вертикале</param>
        /// <param name="pixel">Яроксть пикселя</param>
        public static void Pixel_circle_cikl(this Bitmap bmp, int x, int y, int pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            x1 = x - 1;
            x2 = x;
            x3 = x + 1;
            x4 = x;

            y1 = y;
            y2 = y - 1;
            y3 = y;
            y4 = y + 1;

            x1 = x1 >= 0 ? x1 : 0;
            x3 = x3 < bmp.Width ? x3 : bmp.Width - 1;

            y2 = y2 >= 0 ? y2 : 0;
            y4 = y4 < bmp.Height ? y4 : bmp.Height - 1;

            //bmp.SetPixelFor16bit(x, y, ttt);
            //bmp.SetPixelFor16bit(x1, y1, ttt);
            //bmp.SetPixelFor16bit(x2, y2, ttt);
            //bmp.SetPixelFor16bit(x3, y3, ttt);
            //bmp.SetPixelFor16bit(x4, y4, ttt);

            if (bmp.GetPixel(x, y).G < pixel)
                bmp.SetPixel(x, y, Color.FromArgb(pixel, pixel, pixel));

            if (bmp.GetPixel(x1, y1).G < pixel)
                bmp.SetPixel(x1, y1, Color.FromArgb(pixel, pixel, pixel));

            if (bmp.GetPixel(x2, y2).G < pixel)
                bmp.SetPixel(x2, y2, Color.FromArgb(pixel, pixel, pixel));

            if (bmp.GetPixel(x3, y3).G < pixel)
                bmp.SetPixel(x3, y3, Color.FromArgb(pixel, pixel, pixel));

            if (bmp.GetPixel(x4, y4).G < pixel)
                bmp.SetPixel(x4, y4, Color.FromArgb(pixel, pixel, pixel));
        }

        /// <summary>
        /// Подсветка определённых пикселей с учётом границ изображения
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixel"></param>
        public static void Pixel_circle(this Bitmap bmp, int xc, int yc, int x, int y, int pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            // Смотрим, чтоб отрисовка не вышла за границы холста
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

            //bmp.SetPixel(x1, y1, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x2, y2, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x2, y3, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x1, y4, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x3, y4, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x4, y3, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x4, y2, Color.FromArgb(pixel, pixel, pixel));
            //bmp.SetPixel(x3, y1, Color.FromArgb(pixel, pixel, pixel));

            bmp.Pixel_circle_cikl(x1, y1, pixel);
            bmp.Pixel_circle_cikl(x2, y2, pixel);
            bmp.Pixel_circle_cikl(x2, y3, pixel);
            bmp.Pixel_circle_cikl(x1, y4, pixel);
            bmp.Pixel_circle_cikl(x3, y4, pixel);
            bmp.Pixel_circle_cikl(x4, y3, pixel);
            bmp.Pixel_circle_cikl(x4, y2, pixel);
            bmp.Pixel_circle_cikl(x3, y1, pixel);
        }

        /// <summary>
        /// Алгоритм попиксельного рендеринга окружности
        /// </summary>
        /// <param name="bmp">Холст</param>
        /// <param name="xc">центр окружности по горизонтале</param>
        /// <param name="yc">центр окружности по вертикале</param>
        /// <param name="r">Радиус окружности</param>
        /// <param name="pixel">Яркость окраски</param>
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

        /// <summary>
        /// Алгоритм попиксельного рендеринга окружности, 16 бит
        /// </summary>
        /// <param name="bmp">Холст</param>
        /// <param name="xc">центр окружности по горизонтале</param>
        /// <param name="yc">центр окружности по вертикале</param>
        /// <param name="r">Радиус окружности</param>
        /// <param name="pixel">Яркость окраски</param>
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
    }
}