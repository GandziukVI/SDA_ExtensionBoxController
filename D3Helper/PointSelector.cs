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

        public static Point[] SelectPoints(ref Point[] arr, int N)
        {
            return arr.Where((value, index) => index % N == 0).ToArray();
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
