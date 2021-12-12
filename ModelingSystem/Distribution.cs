using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelingSystem
{
    public class Distribution
    {
        Random random;

        public Distribution(Random random) => this.random = random;

        public double Exponential(double lambda)
        {
            double val = 0;

            while (val == 0)
            {
                val = -1.0 / lambda * Math.Log(random.NextDouble());
            }

            return val;
        }

        public double Normal(double q, double m)
        {
            double val = 0;

            while (val == 0)
            {
                val = NormalVal(q, m);
            }

            return val;
        }

        public double NormalVal(double q, double m)
        {
            return q * Math.Cos(2 * Math.PI * random.NextDouble()) * Math.Sqrt(-2 * Math.Log(random.NextDouble())) + m;
        }


    }
}
