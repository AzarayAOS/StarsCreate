﻿using System;
using System.Drawing;

namespace StarsCreate
{
    /// <summary>
    /// Класс, в котором генерируется одно изображение.
    /// </summary>
    public class CreatePicture
    {
        private int[,] PixValue16;      // массив значений пикселей 16 битного холста

        private int[,] Noiz16;          // массив шума

        private float[] PixTrack;       // одномерный массив трека

        public int H { get; set; }      // размеры холста, высота

        public int W { get; set; }      // размеры холста, ширина

        public string FileName { get; set; }    // пусть к сохраняемому файлу

        public int Noise { get; set; } // шум на изображение
        private Random rd;              // рандомайзер
        private Bitmap bm;              // 16 битный холст

        public int Kolstars { get; set; }           // Количество генерируемых звёзд

        public int PodstConst { get; set; }         // Подставка под матрицу

        public float Et { get; set; }               // Энергия трека
        public int Spp { get; set; }                // Шаг отрисовки трека
        public float Sigm { get; set; }             // Ширина трека и концентрация энергии

        /// <summary>
        /// конструктор класса генерации изображения
        /// </summary>
        /// <param name="w">Ширина изображения</param>
        /// <param name="h">Высота изображения</param>
        /// <param name="kolstars">Количество генерируемых звёзд на изображении</param>
        /// <param name="Noise">Шум, накладываемый на изображение</param>
        /// <param name="PodstConst">Константная подставка под изображение</param>
        /// <param name="Et">Энергия трека</param>
        /// <param name="spp">Шаг отрисовка трека</param>
        /// <param name="sigm">Сигма распределения трека по ширине</param>
        /// <param name="filename">Пусть к файлу сохранения</param>
        public CreatePicture(int w, int h, int kolstars, int Noise, int PodstConst, float Et, int spp, float sigm, string filename)
        {
            this.W = w;
            this.H = h;
            this.Kolstars = kolstars;
            this.Noise = Noise;
            this.PodstConst = PodstConst;
            this.Et = Et;
            this.Spp = spp;
            this.Sigm = sigm;
            this.FileName = filename;

            rd = new Random();

            PixValue16 = new int[W, H];
            Noiz16 = new int[W, H];
            PixTrack = new float[W * H];

            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
                    PixValue16[i, j] = this.PodstConst;

            bm = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
        }

        /// <summary>
        /// Возвращает массив пикселей
        /// </summary>
        /// <returns></returns>
        public int[,] getPixel16()
        {
            return PixValue16;
        }

        /// <summary>
        /// Возвращает карту пикселей
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmap()
        {
            return bm;
        }

        /// <summary>
        /// Сохранить BitMap в файл
        /// </summary>
        public void BitMapSave()
        {
            bm.Save16bitBitmapToPng(FileName);
        }

        /// <summary>
        /// Программа, выполняющая последовательность по созданию,
        /// генерированию звёзд, генерированию шума, наложению трека
        /// и сохранения в файл
        /// </summary>
        public void StartCreate()
        {
            CreateStars();  // создание звёзд
            AddNoise();     // добавляем шум
            CreateTrack();  // создаём трек
            PixToBitAll();  // переносим на холст
            BitMapSave();   // сохраняем в файл
        }

        /// <summary>
        /// Процедура генерации звёзд
        /// </summary>
        private void GenericStar()
        {
            for (int i = 0; i < Kolstars; i++)
                CretStarVal();
        }

        /// <summary>
        /// Добавляет в пиксель с координатами (<paramref name="x"/>, <paramref name="y"/>)
        /// значение <paramref name="value"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        private void AddZnach(int x, int y, int value)
        {
            //if (PixValue16[x, y] < value)
            PixValue16[x, y] = value;

            PixValue16[x, y] = PixValue16[x, y] >= 65535 ? 65535 : PixValue16[x, y];
            PixValue16[x, y] = PixValue16[x, y] <= 0 ? 0 : PixValue16[x, y];
        }

        /// <summary>
        /// Добавить шум на изображение
        /// </summary>
        public void AddNoise()
        {
            Noiz16 = Mass1To2(PNG_Trace_adder.AddGaussianNoise(Mass2To1(PixValue16), Noise));

            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
                    AddZnach(i, j, Noiz16[i, j]);
        }

        /// <summary>
        /// Пропорциональный перевод значения из одного диапазона, в другой
        /// </summary>
        /// <param name="Vakslokal">Максимальное значение из вычисленного</param>
        /// <param name="maxznach">Максимальное значение из нового диапазона</param>
        /// <param name="value">Текущее значение</param>
        /// <returns>Текущее значение в новом диапазоне</returns>
        private static int ValZnach(double Vakslokal, int maxznach, double value)
        {
            double t = value * maxznach / Vakslokal;
            return Convert.ToInt32(t);
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

        /// <summary>
        /// Перенос значения массива на холст 16 битный
        /// </summary>
        private void PixToBit16()
        {
            ushort temp16;

            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
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

            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
                {
                    temp = Convert.ToInt32(Val16ToVal8(Val16ToVal8(PixValue16[i, j])));
                }
        }

        /// <summary>
        /// Функция генерации трека
        /// </summary>
        public void CreateTrack()
        {
            int z = 0;
            //int count = 0;
            int x0, y0, x1, y1;     // левый верхний и правый нижний углы окна

            //double len;

            int x, y;   // ширина и высота окна

            x = rd.Next(Convert.ToInt32(Math.Sqrt(W) * 10));
            y = rd.Next(Convert.ToInt32(Math.Sqrt(H) * 10));

            x0 = rd.Next(1, W - x);
            y0 = rd.Next(1, H - y);

            //x1 = rd.Next(x0, W);
            //y1 = rd.Next(y0, H);

            x1 = x0 + x;
            y1 = y0 + y;

            PNG_Trace_adder.RenderLine(ref PixTrack, x0, y0, x1, y1, x0, y0, x1, y1, Et, 1.0f, 4, W, H);

            // перенос рендеренной линии на изображение
            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
                {
                    {
                        //PixValue16[i, j] = Convert.ToInt32(PixTrack[z] /*+ PodstConst*/) + PixValue16[i, j];

                        AddZnach(i, j, Convert.ToInt32(PixTrack[z]) + PixValue16[i, j]);
                        //count++;
                    }

                    z++;
                }
        }

        /// <summary>
        /// Переносит значения массивов на холсты
        /// </summary>
        public void PixToBitAll()
        {
            //int temp;
            ushort temp16;

            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
                {
                    //temp = Convert.ToInt32(Val16ToVal8(PixValue16[i, j]));
                    //temp = temp >= 0 ? temp : 0;
                    //temp = temp <= 255 ? temp : 255;
                    //Imbm.SetPixel(i, j, Color.FromArgb(temp, temp, temp));

                    temp16 = Convert.ToUInt16(PixValue16[i, j] >= 0 ? (PixValue16[i, j] <= 65535 ? PixValue16[i, j] : 65535) : 0);
                    bm.SetPixelFor16bit(i, j, temp16);
                }
        }

        /// <summary>
        /// Подсветка пикселя и пикселей вокруг стоящих, 16бит
        /// </summary>
        /// <param name="x">координата по горизонтале</param>
        /// <param name="y">Координата по вертикале</param>
        /// <param name="pixel">Яркость пикселя</param>
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
            x3 = x3 < W ? x3 : W - 1;

            y2 = y2 >= 0 ? y2 : 0;
            y4 = y4 < H ? y4 : H - 1;

            //if (PixValue16[x, y] < pixel)
            //    PixValue16[x, y] = pixel;

            //if (PixValue16[x1, y1] < pixel)
            //    PixValue16[x1, y1] = pixel;

            //if (PixValue16[x2, y2] < pixel)
            //    PixValue16[x2, y2] = pixel;

            //if (PixValue16[x3, y3] < pixel)
            //    PixValue16[x3, y3] = pixel;

            //if (PixValue16[x4, y4] < pixel)
            //    PixValue16[x4, y4] = pixel;

            AddZnach(x, y, pixel);
            AddZnach(x1, y1, pixel);
            AddZnach(x2, y2, pixel);
            AddZnach(x3, y3, pixel);
            AddZnach(x4, y4, pixel);
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
            x1 = xc + x < W ? xc + x : W - 1;
            x1 = x1 >= 0 ? x1 : 0;

            x2 = xc + y < W ? xc + y : W - 1;
            x2 = x2 >= 0 ? x2 : 0;

            x3 = xc - x < W ? xc - x : W - 1;
            x3 = x3 >= 0 ? x3 : 0;

            x4 = xc - y < W ? xc - y : W - 1;
            x4 = x4 >= 0 ? x4 : 0;

            y1 = yc + y < H ? yc + y : H - 1;
            y1 = y1 >= 0 ? y1 : 0;

            y2 = yc + x < H ? yc + x : H - 1;
            y2 = y2 >= 0 ? y2 : 0;

            y3 = yc - x < H ? yc - x : H - 1;
            y3 = y3 >= 0 ? y3 : 0;

            y4 = yc - y < H ? yc - y : H - 1;
            y4 = y4 >= 0 ? y4 : 0;

            Pixel_circle16_cikl(x1, y1, pixel);
            Pixel_circle16_cikl(x2, y2, pixel);
            Pixel_circle16_cikl(x2, y3, pixel);
            Pixel_circle16_cikl(x1, y4, pixel);
            Pixel_circle16_cikl(x3, y4, pixel);
            Pixel_circle16_cikl(x4, y3, pixel);
            Pixel_circle16_cikl(x4, y2, pixel);
            Pixel_circle16_cikl(x3, y1, pixel);
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
        /// Перевод значения пикселя из 16 битного варианта
        /// в 8битный вариант, пропорционально
        /// </summary>
        /// <param name="x">значения пикселя в 16битном варианте</param>
        /// <returns>возвращает пропорциональное значение <paramref name="x"/> в 8битном формате.</returns>
        private int Val16ToVal8(int x)
        {
            return Convert.ToInt32((255 * x) / 65535);
        }

        /// <summary>
        /// Создать звёзды
        /// </summary>
        public void CreateStars()
        {
            for (int i = 0; i < Kolstars; i++)
                CretStarVal();
        }

        /// <summary>
        /// Создать одну звезду в случайном месте
        /// </summary>
        private void CretStarVal()
        {
            double[] znach;
            int radius = rd.Next(1, Convert.ToInt32(Math.Min(W, H) / 100));
            int x, y;
            x = rd.Next(1, bm.Width);
            y = rd.Next(1, bm.Height);

            int radgaus = rd.Next(1, 5);

            int light = rd.Next(256, 65535);

            znach = new double[radius];

            for (int i = 0; i < radius; i++)
                znach[i] = Gauss(x + i, radgaus, x);

            for (int i = 1; i <= radius; i++)
            {
                int temp = ValZnach(znach[0], Convert.ToInt32(light / 256), znach[i - 1]);
                int xx = x + i < bm.Width ? x + i : bm.Width - 1;

                if (Val16ToVal8(PixValue16[xx, y]) <= temp)
                {
                    temp = ValZnach(znach[0], light, znach[i - 1]);

                    V_MIcirc16(x, y, i, temp);
                }
            }
        }

        /// <summary>
        /// Перевод двумерного массива в одномерный
        /// </summary>
        private int[] Mass2To1(int[,] m2)
        {
            int[] m1 = new int[W * H];
            int z = 0;
            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
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
            int[,] m2 = new int[W, H];

            int z = 0;

            for (int i = 0; i < W; i++)
                for (int j = 0; j < H; j++)
                {
                    m2[i, j] = m1[z];
                    z++;
                }

            return m2;
        }
    }
}