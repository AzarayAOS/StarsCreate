using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace StarsCreate
{
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
            using (FileStream stream = new FileStream(file, FileMode.Create))
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
        /// Подсветка пикселя и пикселей вокруг стоящих, 16бит
        /// </summary>
        /// <param name="bmp">Палитра</param>
        /// <param name="x">координата по горизонталь</param>
        /// <param name="y">Координата по вертикале</param>
        /// <param name="pixel">Яркость пикселя</param>
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