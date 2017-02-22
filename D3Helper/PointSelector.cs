using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace D3Helper
{
    public static class PointSelector
    {
        public static double[] SelectPoints(ref double[] arr, int N)
        {
            return arr.Where((value, index) => index % N == 0).ToArray();
        }

        public static double[] SelectPoints(ref double[] arr, int N, bool averaging = false)
        {
            if (!averaging)
                return arr.Where((value, index) => index % N == 0).ToArray();
            else
            {
                var res = new LinkedList<double>();

                for (int i = 0; ; )
                {
                    var selection = arr.Where((value, index) => index >= i * N && index < (i + 1) * N).Select(x => x);
                    
                    var xAver = selection.Average();
                    res.AddLast(xAver);

                    if ((i + 1) * N >= arr.Length)
                        break;

                    ++i;
                }

                return res.ToArray();
            }
        }

        public static Point[] SelectPoints(ref Point[] arr, int N, bool averaging = false)
        {
            if (!averaging)
                return arr.Where((value, index) => index % N == 0).ToArray();
            else
            {
                var res = new Point[N];

                var step = (int)Math.Ceiling((double)arr.Length / (double)N);

                for (int i = 0; i < N; i++)
                {
                    var selection = from p in arr
                                    where p.X >= i * step && p.X < (i + 1) * step
                                    select p;

                    var xAver = selection.Average(p => p.X);
                    var yAver = selection.Average(p => p.Y);

                    res[i] = new Point(xAver, yAver);
                }

                return res;
            }
        }

        public static Point[] SelectNPointsPerDecade(ref Point[] arr, int N)
        {
            var minFreq = arr[0].X;
            var maxFreq = arr[arr.Length - 1].X;

            var nDecades = (int)Math.Log10(maxFreq);

            var res = new Point[] { };

            for (int i = 0; i <= nDecades; i++)
            {
                var selection = (from p in arr
                                 where Math.Log10(p.X) >= i && Math.Log10(p.X) < i + 1
                                 select p).ToArray();

                if (selection.Length > N)
                {
                    var factor = (int)(Math.Ceiling((double)selection.Length / N));
                    selection = SelectPoints(ref selection, factor);
                }

                res = res.Concat(selection).ToArray();
            }


            return res;
        }
    }
}
