using ExperimentController;
using MotionManager;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments
{
    public class IV_DefinedResistance : ExperimentBase
    {
        private ISourceMeterUnit smu;
        private IMotionController1D motor;

        private readonly double ConductanceQuantum = 0.0000774809173;

        private Stopwatch stabilityStopwatch;

        public IV_DefinedResistance(ISourceMeterUnit SMU, IMotionController1D Motor)
        {
            smu = SMU;
            motor = Motor;

            stabilityStopwatch = new Stopwatch();
        }

        private string getIVString(ref IV_Data[] Data)
        {
            var resultString = "";
            for (int i = 0; i < Data.Length; i++)
                resultString += string.Format("{0}\t{1}\r\n", Data[i].Voltage.ToString(NumberFormatInfo.InvariantInfo), Data[i].Current.ToString(NumberFormatInfo.InvariantInfo));

            return resultString;
        }

        public override void ToDo(object Arg)
        {
            var setVolt = 0.02;
            var setCond = 15.0;
            var condDev = 5.0;
            var stabilizationTime = 30.0;

            var minPos = 0.0;
            var maxPos = 15.0;
            var setPos = minPos;

            var minConductance = 0.00001;
            var maxConductance = 10;

            var diff = Math.Abs(Math.Log10(maxConductance)) + Math.Abs(Math.Log10(minConductance));

            var minSpeed = 150.0;
            var maxSpeed = 15000.0;

            var inRangeCounter = 0;
            var outsiderCounter = 0;

            var minValue = -0.3;
            var maxValue = 0.3;
            var epsilon = 0.02;
            var nPoints = 101;
            var nCycles = 2;

            var sourceMode = SourceMode.Voltage;

            motor.Position = setPos;
            smu.Voltage = setVolt;
            smu.SwitchON();

            while (true)
            {
                if (!IsRunning)
                    break;

                var scaledConductance = (1.0 / smu.MeasureResistance()) / ConductanceQuantum;

                var speed = minSpeed;
                try
                {
                    if (scaledConductance >= 0.1)
                        speed = minSpeed + (maxSpeed - minSpeed) * Math.Abs(Math.Log10(scaledConductance) - Math.Log10(setCond));
                    else
                        speed = minSpeed + (maxSpeed - minSpeed) * Math.Abs(Math.Log10(scaledConductance) - Math.Log10(setCond)) / Math.Abs(Math.Log10(scaledConductance));
                }
                catch { speed = minSpeed; }

                motor.Velosity = speed;

                if ((scaledConductance >= setCond - (setCond * condDev / 100.0)) &&
                    (scaledConductance <= setCond + (setCond * condDev / 100.0)))
                {
                    if (motor.IsEnabled == true)
                        motor.Enabled = false;

                    if (!stabilityStopwatch.IsRunning)
                        stabilityStopwatch.Start();

                    ++inRangeCounter;
                }
                else
                {
                    if (motor.IsEnabled == false)
                        motor.Enabled = true;

                    if (scaledConductance > setCond)
                        motor.PositionAsync = maxPos;
                    else
                        motor.PositionAsync = minPos;

                    if (stabilityStopwatch.IsRunning == true && inRangeCounter != 0)
                        ++outsiderCounter;
                }

                if (stabilityStopwatch.IsRunning)
                {
                    if ((double)stabilityStopwatch.ElapsedMilliseconds / 1000.0 >= stabilizationTime)
                    {
                        var divider = outsiderCounter > 0 ? (double)outsiderCounter : 1.0;
                        if (Math.Log10((double)inRangeCounter / divider) >= 1.0)
                        {
                            stabilityStopwatch.Stop();
                            break;
                        }
                        else
                            stabilityStopwatch.Restart();
                    }
                }
            }

            var currCycle = 1;

            while (currCycle <= nCycles)
            {
                switch (sourceMode)
                {
                    case SourceMode.Voltage:
                        {
                            var ivData = smu.LinearVoltageSweep(-1.0 * epsilon, maxValue, nPoints);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref ivData)));

                            ivData = smu.LinearVoltageSweep(epsilon, minValue, nPoints);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref ivData)));
                        } break;
                    case SourceMode.Current:
                        {
                            var ivData = smu.LinearCurrentSweep(-1.0 * epsilon, maxValue, nPoints);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref ivData)));

                            ivData = smu.LinearCurrentSweep(epsilon, minValue, nPoints);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref ivData)));
                        } break;
                    case SourceMode.ModeNotSet:
                        throw new ArgumentException();
                }

                ++currCycle;
            }

            smu.SwitchOFF();
        }
    }
}
