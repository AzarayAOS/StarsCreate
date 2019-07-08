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

        public static void RenderLine(ref float[] frame, int fx, int fy, int lx, int ly, float xa, float ya, float xb, float yb, float Et, float sigm, int spp, int nx, int ny)
        {
            fx = 1;
            lx = nx - 1;
            float xl = Convert.ToSingle(xb - xa);

            float yl = Convert.ToSingle(yb - ya);
            float len = Convert.ToSingle(Math.Sqrt(xl * xl + yl * yl));
            int kmax = Convert.ToInt32(Math.Floor(len * spp) + 1);

            Et = Et / kmax;

            float xk = 0;
            float yk = 0;
            float tk = 0;

            for (int k = 0; k <= kmax; k++)
            {
                tk = Convert.ToSingle(k) / Convert.ToSingle(kmax);
                xk = Convert.ToSingle(xl * tk + xa);
                yk = Convert.ToSingle(yl * tk + ya);

                renderPoint(ref frame, fx, fy, lx, ly, xk, yk, Et, sigm, nx, ny);
            }
        }

        private static void renderPoint(ref float[] frame, int fx, int fy, int lx, int ly, float xp, float yp, float Et, float sigm, int nx, int ny)
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
            double t = 0;
            double t1 = 0;
            for (int jy = jy_f - 1; jy <= jy_l; jy++)
            {
                t = (Convert.ToDouble(jy) + 0.5 - yp) * c;
                //            erf_liney[jy - jy_f + 1] = (float)erf(t);
                t1 = 1 / (1 + 0.47047 * Math.Abs(t));
                if (t >= 0) erf_liney[jy - jy_f + 1] = Convert.ToSingle(1 - ((0.3480242) * t1 - 0.0958798 * t1 * t1 + 0.7478556 * t1 * t1 * t1) * Math.Exp(-1 * t * t));
                else erf_liney[jy - jy_f + 1] = -1 * Convert.ToSingle(1 - (Convert.ToSingle(0.3480242) * t1 - Convert.ToSingle(0.0958798) * t1 * t1 + Convert.ToSingle(0.7478556) * t1 * t1 * t1) * Math.Exp(-1 * t * t));
            }
            ;
            for (int ix = ix_f - 1; ix < ix_l; ix++)
            {
                t = (Convert.ToDouble(ix) + 0.5 - xp) * c;
                //            erf_linex[ix-ix_f+1] = (float)erf(t);
                t1 = 1.0 / (1.0 + 0.47047 * Math.Abs(t));
                if (t >= 0) erf_linex[ix - ix_f + 1] = Convert.ToSingle(1 - ((0.3480242) * t1 - 0.0958798 * t1 * t1 + 0.7478556 * t1 * t1 * t1) * Math.Exp(-1 * t * t));
                else erf_linex[ix - ix_f + 1] = -1 * Convert.ToSingle(1 - (Convert.ToSingle(0.3480242) * t1 - Convert.ToSingle(0.0958798) * t1 * t1 + Convert.ToSingle(0.7478556) * t1 * t1 * t1) * Math.Exp(-1 * t * t));
            }

            Et = Convert.ToSingle(Et * 0.25);

            for (int jy = jy_f; jy <= jy_l; jy++)
                for (int ix = ix_f; ix <= ix_l; ix++)
                {
                    t = Et * (erf_liney[jy - jy_f + 1] - erf_liney[jy - jy_f]) * (erf_linex[ix - ix_f + 1] - erf_linex[ix - ix_f]);
                    if (t > 0) frame[ix * ny + jy] += Convert.ToSingle(t);
                }

            ;
        }
    }
}