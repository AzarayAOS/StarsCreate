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
    internal static class Create_on_Gauss
    {
        private readonly static double c_0707 = Math.Sin(Math.PI / 4);     // sin(45)
        private readonly static double sqrt_2 = Math.Sqrt(2);              // корень из 2

        /// <summary>
        /// Возвращает квадрат агрумента <paramref name="x"/>
        /// </summary>
        /// <param name="x">Аргумент</param>
        /// <returns><paramref name="x"/>^2</returns>
        private static double sqr(double x)
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
                t = 1.0 / (1.0 + 0.47047 * x);
                y = 0.3480242 * t - 0.0958798 * t * t + 0.7478556 * t * t * t;
                y = 1 - y * Math.Exp(-x * x);
            }
            else
                y = 1.0;

            return s ? y : -y;
        }

        public static int[] AddGaussianNoise(int[] form, float noisePower = 20)
        {
            var rnd = new Random();

            //var t = (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 2) * noisePower;

            for (int i = 0; i < form.Length; i++)
            {
                form[i] = Convert.ToInt32(form[i] + (rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() + rnd.NextDouble() - 2) * noisePower);
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
        /// Создать точку света на (<paramref name="xp"/>, <paramref name="yp"/>) на <paramref name="frame"/>
        /// в границах (<paramref name="fx"/>, <paramref name="fy"/>) - (<paramref name="lx"/>, <paramref name="ly"/>) включительно
        /// в соответствии с гауссовым PSF с дисперсией <paramref name="Spsf"/>
        /// нормируется по общей излучаемой энергии <paramref name="Et"/>
        /// </summary>
        private static void Render_Point(ref double[] frame, int pitch, int fx, int fy, int lx, int ly, double xp, double yp, ref double Et, double Spsf)
        {
            double s = sqrt_2 * Spsf;
            double c = c_0707 * Spsf;
            double RL = 6.0 * s + 0.5;

            int jy_f = Convert.ToInt32(Math.Floor(-RL + yp) + 1);
            int jy_l = Convert.ToInt32(Math.Ceiling(RL + yp) - 1);

            jy_f = Math.Max(jy_f, fy);
            jy_l = Math.Min(jy_l, ly);

            int ix_f = Convert.ToInt32(Math.Floor(-RL + xp) + 1);
            int ix_l = Convert.ToInt32(Math.Ceiling(RL + xp) - 1);

            ix_f = Math.Max(ix_f, fx);
            ix_l = Math.Min(ix_l, lx);

            if ((jy_l < jy_f) || (ix_l < ix_f))
                return;

            double[] erf_liney = new double[jy_l - jy_f + 2];
            double[] erf_linex = new double[ix_l - ix_f + 2];

            for (int jy = jy_f - 1; jy <= jy_l; ++jy)
            {
                double t = (Convert.ToDouble(jy) + 0.5 - yp) * c;
                erf_linex[jy - jy_f + 1] = Erf(t);
            }

            for (int ix = ix_f - 1; ix <= ix_l; ++ix)
            {
                double t = (Convert.ToDouble(ix) + 0.5 - xp) * c;
                erf_linex[ix_f - ix_f + 1] = Erf(t);
            }

            Et *= 0.25;

            for (int jy = jy_f; jy <= jy_l; ++jy)
                for (int ix = ix_f; ix <= ix_l; ++ix)
                {
                    frame[jy * pitch + ix] += Et * (erf_liney[jy - jy_f + 1] - erf_liney[jy - jy_f])
                        * (erf_linex[ix - ix_f + 1] - erf_linex[ix - ix_f]);
                }
        }

        /// <summary>
        /// Отрисовка трассировки равномерно прямой линии движущейся точки на «рамке»
        /// из точки (<paramref name="xa"/>, <paramref name="ya"/>) в точку (<paramref name="xb"/>, <paramref name="yb"/>)
        /// в границах (<paramref name="fx"/>, <paramref name="fy"/>) - (<paramref name="lx"/>, <paramref name="ly"/>) включительно
        /// в соответствии с гауссовым PSF с дисперсией <paramref name="Spsf"/>
        /// делим длину траектории по шагам <paramref name="spp"/> на пиксель
        /// с амплитудой <paramref name="Et"/> в центральных точках
        /// </summary>
        public static double[] Render_Line(double[] frame, int pitch, int fx, int fy, int lx, int ly, double xa, double ya, double xb, double yb, ref double Et, double Spsf, double spp)
        {
            double xl = xb - xa;
            double yl = yb - ya;
            double len = Math.Sqrt(sqr(xl) + sqr(yl));
            int kmax = Convert.ToInt32(Math.Floor(len + spp) + 1);
            Et /= spp;

            for (int k = 0; k <= kmax; ++k)
            {
                double tk = Convert.ToDouble(k) / Convert.ToDouble(kmax);
                double xk = xl * tk + xa;
                double yk = yl * tk + ya;
                Render_Point(ref frame, pitch, fx, fy, lx, ly, xk, yk, ref Et, Spsf);
            }

            return frame;
        }
    }
}