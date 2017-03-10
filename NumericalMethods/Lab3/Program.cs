using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    class Program
    {
        // Romberg integration
        static double Romberg(Func<double, double> func, double a, double b)
        {
            int N = 5;

            double[] h = new double[N + 1];
            double[,] r = new double[N + 1, N + 1];

            for (int i = 1; i < N + 1; ++i)
            {
                h[i] = (b - a) / Math.Pow(2, i - 1);
            }
            r[1, 1] = h[1] / 2 * (func(a) + func(b));
            for (int i = 2; i < N + 1; ++i)
            {
                double coeff = 0;
                for (int k = 1; k <= Math.Pow(2, i - 2); ++k)
                {
                    coeff += func(a + (2 * k - 1) * h[i]);
                }
                r[i, 1] = 0.5 * (r[i - 1, 1] + h[i - 1] * coeff);
            }

            for (int i = 2; i < N + 1; ++i)
            {
                for (int j = 2; j <= i; ++j)
                {
                    r[i, j] = r[i, j - 1] + (r[i, j - 1] - r[i - 1, j - 1]) / (Math.Pow(4, j - 1) - 1);
                }
            }
            return r[N, N];
        }
        static void Main(string[] args)
        {
            //Console.WriteLine(Romberg((n) => 1 / n, 1, 10));
            Console.WriteLine(Romberg((n) => Math.Exp(-n * n / 2) / Math.Sqrt(2 * Math.PI), -3, 3));


            Console.ReadKey();
        }


    }
}
