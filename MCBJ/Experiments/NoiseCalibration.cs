using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments
{
    public interface IAmplifier
    {
        double[] GetIntrinsicNoiseCurve();
        double[] GetFrequencyResponseCurve();

        double[,] GetCalibratedCurve();
    }

    public class AmplifierBase : IAmplifier
    {
        public AmplifierBase()
        {

        }

        public AmplifierBase(string FormatFileName)
        {

        }

        public double[] GetIntrinsicNoiseCurve()
        {
            throw new NotImplementedException();
        }

        public double[] GetFrequencyResponseCurve()
        {
            throw new NotImplementedException();
        }

        public double[,] GetCalibratedCurve()
        {
            throw new NotImplementedException();
        }
    }

    public class HomamadeAmplifier
    {

    }

    public class StanfordSR560Amplifier
    {

    }

    public class NoiseCalibration
    {

    }
}
