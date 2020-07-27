using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarsCreate
{
    public class Star
    {
        public Star(int id, double ra, double dec, double mgt)
        {
            Id = id;
            Ra = ra;
            Dec = dec;
            Mgt = mgt;
        }

        public int Id { get; set; }
        public double Ra { get; set; }
        public double Dec { get; set; }
        public double Mgt { get; set; }
    }
}