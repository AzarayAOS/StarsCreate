using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarsCreate
{
    /// <summary>
    /// Класс функций для создания трека и шума на изображении
    /// </summary>
    internal static class PNG_Trace_adder
    {
        private readonly static double c_0707 = Math.Sin(Math.PI / 4);     // sin(45) 0,707106781186547

        // коэффициенты для поляризации разряда в эмпирических зависимостях
        private readonly static double P = 0.47047;

        private readonly static double A1 = 0.3480242;
        private readonly static double A2 = -0.0958798;
        private readonly static double A3 = 0.7478556;

        /// <summary>
        /// Возвращает квадрат агрумента <paramref name="x"/>
        /// </summary>
        /// <param name="x">Аргумент</param>
        /// <returns><paramref name="x"/>^2</returns>
        private static double Sqr(double x)
        {
            return x * x;
        }

        /// <summary>
        /// для данного <paramref name="x"/> вычисляет стандартную функцию "erf (<paramref name="x"/>)"
        /// </summary>
        /// <param name="x">Значение</param>
        /// <returns>Значение от функции "erf (<paramref name="x"/>)"</returns>
        private static double Erf(double x)
        {
            double t, y;

            bool s = (x >= 0.0);

            x = Math.Abs(x);

            if (x < 6.0)
            {
                t = 1.0 / (1.0 + P * x);
                y = A1 * t + A2 * t * t + A3 * t * t * t;
                y = 1 - y * Math.Exp(-x * x);
            }
            else
                y = 1.0;

            return s ? y : -y;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "SecurityIntelliSenseCS:MS Security rules violation", Justification = "<Ожидание>")]
        public static int[] AddGaussianNoise(int[] form, float noisePower = 20)
        {
            var rnd = new Random();

            //var t = (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 2) * noisePower;

            for (int i = 0; i < form.Length; i++)
            {
                form[i] = Convert.ToInt32(form[i] + (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 2) * noisePower);
                //form[i] = Convert.ToInt32(form[i] + (rnd.NextDouble() - 0.5) * noisePower);
            }

            return form;
        }

        /// <summary>
        /// Вычисляет интеграл в пределах (<paramref name="a"/>, <paramref name="b"/>)
        /// из гауссовой функции плотности вероятности
        /// со средним значением <paramref name="x"/> и дисперсией <paramref name="G"/>
        /// </summary>
        /// <param name="x">среднее значение</param>
        /// <param name="a">от</param>
        /// <param name="b">до</param>
        /// <param name="G">дисперсия</param>
        /// <returns></returns>
        private static double Int_Gaus(double x, double a, double b, double G)
        {
            double c = c_0707 / G;
            return (Erf(c * (b - x)) - Erf(c * (a - x))) * 0.5;
        }

        /// <summary>
        ///Отрисовка трассировки равномерно прямой линии движущейся точки на «рамке»
        /// из точки (<paramref name="xa"/>, <paramref name="ya"/>) в точку (<paramref name="xb"/>, <paramref name="yb"/>)
        /// в границах (<paramref name="fx"/>, <paramref name="fy"/>) - (<paramref name="lx"/>, <paramref name="ly"/>) включительно
        /// в соответствии с гауссовым PSF с дисперсией '<paramref name="sigm"/>'
        /// делим длину траектории по шагам '<paramref name="spp"/>' на пиксель
        /// с амплитудой '<paramref name="Et"/>' в центральных точках.
        /// </summary>
        /// <param name="frame">Массив пикселей изображения</param>
        /// <param name="fx">Левый верхний угол по оси ОХ</param>
        /// <param name="fy">Левый верхний угол по оси ОУ</param>
        /// <param name="lx">Правый нижний угол по оси ОХ</param>
        /// <param name="ly">Правый нижний угол по оси ОУ</param>
        /// <param name="xa">Начало трека по оси ОХ</param>
        /// <param name="ya">Начало трека по оси ОУ</param>
        /// <param name="xb">Конец трека по оси ОХ</param>
        /// <param name="yb">Конец трека по оси ОУ</param>
        /// <param name="Et">Энергия трека</param>
        /// <param name="sigm">Дисперсия распределения</param>
        /// <param name="spp">Шаг отрисовки трека</param>
        /// <param name="nx">Ширина изображения</param>
        /// <param name="ny">Высота изображения</param>
        public static void RenderLine(ref float[] frame, int fx, int fy, int lx, int ly, float xa, float ya, float xb, float yb, float Et, float sigm, int spp, int nx, int ny)
        {
            fx = 1;
            lx = nx - 1;
            float xl = Convert.ToSingle(xb - xa);

            float yl = Convert.ToSingle(yb - ya);
            float len = Convert.ToSingle(Math.Sqrt(xl * xl + yl * yl));
            int kmax = Convert.ToInt32(Math.Floor(len * spp) + 1);

            Et /= kmax;
            for (int k = 0; k <= kmax; k++)
            {
                float tk = Convert.ToSingle(k) / Convert.ToSingle(kmax);
                float xk = Convert.ToSingle(xl * tk + xa);
                float yk = Convert.ToSingle(yl * tk + ya);

                RenderPoint(ref frame, fx, fy, lx, ly, xk, yk, Et, sigm, nx, ny);
            }
        }

        private static void RenderPoint(ref float[] frame, int fx, int fy, int lx, int ly, float xp, float yp, float Et, float sigm, int nx, int ny)
        {
            float s = Convert.ToSingle(Math.Sqrt(2) * sigm);
            float c = Convert.ToSingle(Math.Sqrt(2) / (2 * sigm));
            float RL = Convert.ToSingle(6 * s + 0.5);

            int jy_f = Convert.ToInt32(Math.Floor(-RL + yp) + 1);
            int jy_l = Convert.ToInt32(Math.Ceiling(RL + yp) - 1);
            jy_f = Math.Max(jy_f, fy);
            jy_l = Math.Min(jy_l, ly);

            int ix_f = Convert.ToInt32(Math.Floor(-RL + xp) + 1);
            int ix_l = Convert.ToInt32(Math.Ceiling(RL + xp) - 1);
            ix_f = Math.Max(ix_f, fx);
            ix_l = Math.Min(ix_l, lx);

            if (jy_l < jy_f || ix_l < ix_f)
                return;

            float[] erf_liney = new float[30];
            float[] erf_linex = new float[30];
            double t;
            double t1;
            for (int jy = jy_f - 1; jy <= jy_l; jy++)
            {
                t = (Convert.ToDouble(jy) + 0.5 - yp) * c;
                t1 = 1 / (1 + P * Math.Abs(t));
                if (t >= 0)
                    erf_liney[jy - jy_f + 1] =
                        Convert.ToSingle(1 - ((A1) *
                        t1 + A2 * t1 * t1 +
                        A3 * t1 * t1 * t1) *
                        Math.Exp(-1 * t * t));
                else
                    erf_liney[jy - jy_f + 1] =
                        -1 * Convert.ToSingle(1 -
                        (Convert.ToSingle(A1) * t1 +
                        Convert.ToSingle(A2) * t1 * t1 +
                        Convert.ToSingle(A3) * t1 * t1 * t1)
                        * Math.Exp(-1 * t * t));
            }
            ;
            for (int ix = ix_f - 1; ix < ix_l; ix++)
            {
                t = (Convert.ToDouble(ix) + 0.5 - xp) * c;
                t1 = 1.0 / (1.0 + P * Math.Abs(t));
                if (t >= 0)
                    erf_linex[ix - ix_f + 1] =
                        Convert.ToSingle(1 - ((A1) *
                        t1 + A2 * t1 * t1 +
                        A3 * t1 * t1 * t1) *
                        Math.Exp(-1 * t * t));
                else
                    erf_linex[ix - ix_f + 1] =
                        -1 * Convert.ToSingle(1 -
                        (Convert.ToSingle(A1) * t1 +
                        Convert.ToSingle(A2) * t1 * t1 +
                        Convert.ToSingle(A3) * t1 * t1 * t1)
                        * Math.Exp(-1 * t * t));
            }

            Et = Convert.ToSingle(Et * 0.25);

            for (int jy = jy_f; jy <= jy_l; jy++)
                for (int ix = ix_f; ix <= ix_l; ix++)
                {
                    t = Et * (erf_liney[jy - jy_f + 1] -
                        erf_liney[jy - jy_f]) *
                        (erf_linex[ix - ix_f + 1] -
                        erf_linex[ix - ix_f]);
                    if (t > 0) frame[ix * ny + jy] += Convert.ToSingle(t);
                }

            ;
        }
    }
}