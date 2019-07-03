using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarsCreate
{
    /// <summary>
    /// Класс финкций добавления трека на изображение
    /// </summary>
    internal static class PNG_Trace_adder
    {
        public static void RenderLine(ref double[] frame, int fx, int fy, int lx, int ly, double xa, double ya, double xb, double yb, ref double Et, double sigm, int spp, int nx, int ny)
        {
            fx = 1;
            lx = nx - 1;
            double xl = xb - xa;

            double yl = yb - ya;
            double len = Math.Sqrt(xl * xl + yl * yl);
            int kmax = Convert.ToInt32(Math.Floor(len * spp) + 1);

            Et = Et / kmax;

            double xk = 0;
            double yk = 0;
            double tk = 0;

            for (int k = 0; k <= kmax; k++)
            {
                tk = Convert.ToDouble(k) / Convert.ToDouble(kmax);
                xk = xl * tk + xa;
                yk = yl * tk + ya;

                renderPoint(ref frame, fx, fy, lx, ly, xk, yk, ref Et, sigm, nx, ny);
            }
        }

        private static void renderPoint(ref double[] frame, int fx, int fy, int lx, int ly, double xp, double yp, ref double Et, double sigm, int nx, int ny)
        {
            double s = Math.Sqrt(2) * sigm;
            double c = Math.Sqrt(2) / (2 * sigm);
            double RL = (6 * s + 0.5);

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

            double[] erf_liney = new double[30];
            double[] erf_linex = new double[30];
            double t = 0;
            double t1 = 0;
            for (int jy = jy_f - 1; jy <= jy_l; jy++)
            {
                t = (Convert.ToDouble(jy) + 0.5 - yp) * c;
                //            erf_liney[jy - jy_f + 1] = (float)erf(t);
                t1 = 1 / (1 + 0.47047 * Math.Abs(t));
                if (t >= 0) erf_liney[jy - jy_f + 1] = Convert.ToDouble(1 - ((0.3480242) * t1 - 0.0958798 * t1 * t1 + 0.7478556 * t1 * t1 * t1) * Math.Exp(-1 * t * t));
                else erf_liney[jy - jy_f + 1] = -1 * Convert.ToDouble(1 - (Convert.ToDouble(0.3480242) * t1 - Convert.ToDouble(0.0958798) * t1 * t1 + Convert.ToDouble(0.7478556) * t1 * t1 * t1) * Math.Exp(-1 * t * t));
            }

            for (int ix = ix_f - 1; ix < ix_l; ix++)
            {
                t = (Convert.ToDouble(ix) + 0.5 - xp) * c;
                //            erf_linex[ix-ix_f+1] = (float)erf(t);
                t1 = 1.0 / (1.0 + 0.47047 * Math.Abs(t));
                if (t >= 0) erf_linex[ix - ix_f + 1] = Convert.ToDouble(1 - ((0.3480242) * t1 - 0.0958798 * t1 * t1 + 0.7478556 * t1 * t1 * t1) * Math.Exp(-1 * t * t));
                else erf_linex[ix - ix_f + 1] = -1 * Convert.ToDouble(1 - (Convert.ToDouble(0.3480242) * t1 - Convert.ToDouble(0.0958798) * t1 * t1 + Convert.ToDouble(0.7478556) * t1 * t1 * t1) * Math.Exp(-1 * t * t));
            }

            Et = Convert.ToDouble(Et * 0.25);

            for (int jy = jy_f; jy <= jy_l; jy++)
                for (int ix = ix_f; ix <= ix_l; ix++)
                {
                    t = Et * (erf_liney[jy - jy_f + 1] - erf_liney[jy - jy_f]) * (erf_linex[ix - ix_f + 1] - erf_linex[ix - ix_f]);
                    if (t > 0) frame[ix * ny + jy] += t;
                }
        }
    }
}