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
        private int[,] PixValue16;  // массив значений пикселей 16 битного холста

        private float[] PixTrack;     // одномерный массив трека
        private int h, w;           // размеры холста

        private Random rd;          // рандомайзер
        private Bitmap bm;          // 16 битный холст
        private Bitmap Imbm;        // 8 битный холст
        private int kolstars;       // Количество генерируемых звёзд
        private int PodstConst;     // Подставка под матрицу
        public float Et;
        public int spp = 4;
        public float sigm = 1;

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
            for (int i = 0; i < kolstars; i++)
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
                        toolStripStatusLabel1.Text = "Создано за " + st.Elapsed.ToString();

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
            PodstConst = Convert.ToInt32(textBox4.Text);

            Et = Convert.ToSingle(textBox5.Text);
            PixValue16 = new int[w, h];
            PixTrack = new float[w * h];
            //PixValue = new int[w, h];

            trackBar1.Maximum = Convert.ToInt32(textBox4.Text);
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    PixValue16[i, j] = PodstConst;

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
                    temp = Convert.ToInt32(Val16ToVal8(Val16ToVal8(PixValue16[i, j])));
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
                    temp = Convert.ToInt32(Val16ToVal8(PixValue16[i, j]));
                    temp = temp >= 0 ? temp : 0;
                    temp = temp <= 255 ? temp : 255;
                    Imbm.SetPixel(i, j, Color.FromArgb(temp, temp, temp));

                    temp16 = Convert.ToUInt16(PixValue16[i, j] >= 0 ? (PixValue16[i, j] <= 65535 ? PixValue16[i, j] : 65535) : 0);
                    bm.SetPixelFor16bit(i, j, temp16);
                }
        }

        /*
        /// <summary>
        /// Подсветка пикселя и пикселей вокруг стоящих
        /// </summary>
        /// <param name="x">координата по горизонтале</param>
        /// <param name="y">Координата по вертикале</param>
        /// <param name="pixel">Яроксть пикселя</param>
        private void Pixel_circle_cikl(int x, int y, int pixel)
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
            x3 = x3 < w ? x3 : w - 1;

            y2 = y2 >= 0 ? y2 : 0;
            y4 = y4 < h ? y4 : h - 1;

            //bmp.SetPixelFor16bit(x, y, ttt);
            //bmp.SetPixelFor16bit(x1, y1, ttt);
            //bmp.SetPixelFor16bit(x2, y2, ttt);
            //bmp.SetPixelFor16bit(x3, y3, ttt);
            //bmp.SetPixelFor16bit(x4, y4, ttt);

            if (Val16ToVal8(PixValue16[x, y]) < pixel)
                Val16ToVal8(PixValue16[x, y]) = pixel;

            if (Val16ToVal8(PixValue16[x1, y1]) < pixel)
                Val16ToVal8(PixValue16[x1, y1]) = pixel;

            if (Val16ToVal8(PixValue16[x2, y2]) < pixel)
                Val16ToVal8(PixValue16[x2, y2]) = pixel;

            if (Val16ToVal8(PixValue16[x3, y3]) < pixel)
                Val16ToVal8(PixValue16[x3, y3]) = pixel;

            if (Val16ToVal8(PixValue16[x4, y4]) < pixel)
                Val16ToVal8(PixValue16[x4, y4]) = pixel;
        }

        /// <summary>
        /// Подсветка определённых пикселей с учётом границ изображения
        /// </summary>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixel"></param>
        private void Pixel_circle(int xc, int yc, int x, int y, int pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            // Смотрим, чтоб отрисовка не вышла за границы холста
            x1 = xc + x < w ? xc + x : w - 1;
            x1 = x1 >= 0 ? x1 : 0;

            x2 = xc + y < w ? xc + y : w - 1;
            x2 = x2 >= 0 ? x2 : 0;

            x3 = xc - x < w ? xc - x : w - 1;
            x3 = x3 >= 0 ? x3 : 0;

            x4 = xc - y < w ? xc - y : w - 1;
            x4 = x4 >= 0 ? x4 : 0;

            y1 = yc + y < h ? yc + y : h - 1;
            y1 = y1 >= 0 ? y1 : 0;

            y2 = yc + x < h ? yc + x : h - 1;
            y2 = y2 >= 0 ? y2 : 0;

            y3 = yc - x < h ? yc - x : h - 1;
            y3 = y3 >= 0 ? y3 : 0;

            y4 = yc - y < h ? yc - y : h - 1;
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

        */

        /// <summary>
        /// Подсветка пикселя и пикселей вокруг стоящих, 16бит
        /// </summary>
        /// <param name="x">координата по горизонтале</param>
        /// <param name="y">Координата по вертикале</param>
        /// <param name="pixel">Яроксть пикселя</param>
        private void Pixel_circle16_cikl(int x, int y, int pixel)
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
            x3 = x3 < w ? x3 : w - 1;

            y2 = y2 >= 0 ? y2 : 0;
            y4 = y4 < h ? y4 : h - 1;

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
        private void Pixel_circle16(int xc, int yc, int x, int y, int pixel)
        {
            int x1, x2, x3, x4;
            int y1, y2, y3, y4;

            // Смотрим, чтоб отрисовка не вышла за границы холста
            x1 = xc + x < w ? xc + x : w - 1;
            x1 = x1 >= 0 ? x1 : 0;

            x2 = xc + y < w ? xc + y : w - 1;
            x2 = x2 >= 0 ? x2 : 0;

            x3 = xc - x < w ? xc - x : w - 1;
            x3 = x3 >= 0 ? x3 : 0;

            x4 = xc - y < w ? xc - y : w - 1;
            x4 = x4 >= 0 ? x4 : 0;

            y1 = yc + y < h ? yc + y : h - 1;
            y1 = y1 >= 0 ? y1 : 0;

            y2 = yc + x < h ? yc + x : h - 1;
            y2 = y2 >= 0 ? y2 : 0;

            y3 = yc - x < h ? yc - x : h - 1;
            y3 = y3 >= 0 ? y3 : 0;

            y4 = yc - y < h ? yc - y : h - 1;
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
        private void V_MIcirc16(int xc, int yc, int r, int pixel)
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
        /// Перевод значения пикслеля из 16 битного варианта
        /// в 8битный вариант, пропорцеонально
        /// </summary>
        /// <param name="x">значения пиксиля в 16битном варианте</param>
        /// <returns>выозращает пропорцеональное значение <paramref name="x"/> в 8битном формате.</returns>
        private int Val16ToVal8(int x)
        {
            return Convert.ToInt32((255 * x) / 65535);
        }

        /*
        /// <summary>
        /// Алгоритм попиксельного рендеринга окружности
        /// </summary>
        /// <param name="xc">центр окружности по горизонтале</param>
        /// <param name="yc">центр окружности по вертикале</param>
        /// <param name="r">Радиус окружности</param>
        /// <param name="pixel">Яркость окраски</param>
        private void V_MIcirc(int xc, int yc, int r, int pixel)
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

        */

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
                if (Val16ToVal8(PixValue16[xx, y]) <= temp)
                {
                    //V_MIcirc(x, y, i, temp);

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
            //for (int i = 0; i < kolstars; i++)
            // CretStarVal();

            //PixToBitAll();
            // st.Stop();
            ;
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
            bm.Save16bitBitmapToPng("D:\\JavaStarTrak\\Nikita\\Output_png\\1.png");
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            CretStarVal();

            pictureBox1.Image = Imbm;
        }

        /// <summary>
        /// Первод двумерного массива в одномерный
        /// </summary>
        private int[] Mass2To1(int[,] m2)
        {
            int[] m1 = new int[w * h];
            int z = 0;
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    m1[z] = m2[i, j];
                    z++;
                }

            return m1;
        }

        /// <summary>
        /// Перевод одномерного массива в двумерный
        /// </summary>
        private int[,] Mass1To2(int[] m1)
        {
            int[,] m2 = new int[w, h];

            int z = 0;

            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    m2[i, j] = m1[z];
                    z++;
                }

            return m2;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            //if (timer1.Enabled)
            //{
            //    // выключение таймера
            //    button4.Text = "Начать генерацию";
            //}
            //else
            //{
            //    // включение таймера
            //    button4.Text = "Остановить генерацию";
            //}

            //timer1.Enabled = !timer1.Enabled;

            // Create_on_Gauss.AddGaussianNoise(ref PixValue16, 50);

            //PixValue16 = Mass1To2(Create_on_Gauss.AddGaussianNoise(Mass2To1(PixValue16), Convert.ToInt32(textBox3.Text)));
            PixValue16 = Mass1To2(PNG_Trace_adder.AddGaussianNoise(Mass2To1(PixValue16), Convert.ToInt32(textBox3.Text)));
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            st.Start();
            PixToBitAll();
            st.Stop();
            toolStripStatusLabel1.Text = "Отображено за " + st.Elapsed.ToString();
            pictureBox1.Image = Imbm;
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            // генерация трека на изображении
            Et = Convert.ToSingle(textBox5.Text);
            ////Create_on_Gauss.Render_Line(PixTrack, pitch: 2, 100, 100, 200, 200, 110, 110, 190, 190, ref Et, sigm, spp);
            //int z = 0;
            //int count = 0;

            //int x0, y0, x1, y1;

            //x0 = rd.Next(500) + 1;
            //y0 = rd.Next(500) + 1;
            //x1 = rd.Next(500) + 1;
            //y1 = rd.Next(500) + 1;

            //int fx = 1;
            //int fy = 1;
            //int lx = w - 1;
            //int ly = h - 1;

            //double len;

            //while ((len = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0))) < 20)
            //{
            //    y1 = rd.Next(500) + 1;
            //}
            //;
            ////PNG_Trace_adder.RenderLine(ref PixTrack, fx, fy, lx, ly, x0, y0, x1, y1, Et, sigm, spp, w, h);
            //PNG_Trace_adder.RenderLine(ref PixTrack, 1, 1, 511, 511, 4.0f, 10.0f, 30.0f, 47.0f, Et, 1.0f, 4, w, h);

            //;
            //// перенос рендеренной линии на изображение
            //for (int i = 0; i < w; i++)
            //    for (int j = 0; j < h; j++)
            //    {
            //        //if (PixValue16[i, j] < (PixTrack[z] /*+ PodstConst*/))
            //        //if (PixTrack[z] > 0)
            //        {
            //            PixValue16[i, j] = Convert.ToInt32(PixTrack[z] /*+ PodstConst*/) + PixValue16[i, j];
            //            count++;
            //        }

            //        z++;
            //    }

            //;

            int z = 0;
            int count = 0;
            int x0, y0, x1, y1;     // левый верхний и праввый нижний углы окна

            double len;

            int x, y;   // ширина и высота окна

            x = rd.Next(Convert.ToInt32(Math.Sqrt(w) * 10));
            y = rd.Next(Convert.ToInt32(Math.Sqrt(h) * 10));

            x0 = rd.Next(1, w - x);
            y0 = rd.Next(1, h - y);

            //x1 = rd.Next(x0, w);
            //y1 = rd.Next(y0, h);

            x1 = x0 + x;
            y1 = y0 + y;

            //PNG_Trace_adder.RenderLine(ref PixTrack, fx, fy, lx, ly, x0, y0, x1, y1, Et, sigm, spp, w, h);
            PNG_Trace_adder.RenderLine(ref PixTrack, x0, y0, x1, y1, x0, y0, x1, y1, Et, 1.0f, 4, w, h);

            // перенос рендеренной линии на изображение
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    //if (PixValue16[i, j] < (PixTrack[z] /*+ PodstConst*/))
                    //if (PixTrack[z] > 0)
                    {
                        PixValue16[i, j] = Convert.ToInt32(PixTrack[z] /*+ PodstConst*/) + PixValue16[i, j];
                        count++;
                    }

                    z++;
                }
        }

        private void TextBox4_TextChanged(object sender, EventArgs e)
        {
        }

        private void TextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void TextBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                int a = Convert.ToInt32(textBox3.Text);
                if (a > PodstConst) textBox3.Text = PodstConst.ToString();
            }
        }

        private void TextBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void TrackBar1_ValueChanged(object sender, EventArgs e)
        {
            textBox3.Text = trackBar1.Value.ToString();
        }
    }
}