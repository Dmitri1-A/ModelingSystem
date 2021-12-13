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

        List<double> C;
        List<double> list2;

        public Distribution(Random random)
        {
            C = new List<double>(4) { 0.367, 0.9198, 0.0287, 0.1357};
            list2 = new List<double>();

            this.random = random;
        }

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

        public double CountRandomT2(double G, double M)
        {
            list2.Clear();

            for (int i = 0; i < C.Count; i++)
            {
                list2.Add(NormalVal(G, M));
            }

            double value = Enumerable.Range(0, C.Count).Select(i => C[i] * list2[i]).Sum() - 10;

            return value;
        }
    }
}
