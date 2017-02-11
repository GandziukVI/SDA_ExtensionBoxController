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
                resultString += string.Format("{0},{1}\r\n", Data[i].Voltage.ToString(NumberFormatInfo.InvariantInfo), Data[i].Current.ToString(NumberFormatInfo.InvariantInfo));

            return resultString;
        }

        public override void ToDo(object Arg)
        {
            onStatusChanged(new StatusEventArgs("Starting the measurement."));

            var settings = (IV_DefinedResistanceInfo)Arg;

            var setVolt = settings.ScanningVoltage;
            var setCond = settings.SetConductance;
            var condDev = settings.Deviation;
            var stabilizationTime = settings.StabilizationTime;

            var minSpeed = settings.MinSpeed;
            var maxSpeed = settings.MaxSpeed;

            var minValue = settings.IVMinvalue;
            var maxValue = settings.IVMaxvalue;

            var epsilon = settings.Epsilon;
            var nPoints = settings.NPoints;
            var nCycles = settings.NCycles;

            var sourceMode = settings.SourceMode;
            var compliance = settings.Compliance;

            var minPos = settings.MotorMinPos;
            var maxPos = settings.MotorMaxPos;

            var inRangeCounter = 0;
            var outsiderCounter = 0;

            motor.Enabled = true;
            motor.Velosity = maxSpeed;

            smu.SourceMode = sourceMode;
            smu.Compliance = compliance;
            smu.Voltage = setVolt;
            smu.SwitchON();

            onStatusChanged(new StatusEventArgs("Reaching the specified resistance / conductance value."));

            while (true)
            {
                if (!IsRunning)
                    break;

                var scaledConductance = (1.0 / smu.MeasureResistance()) / ConductanceQuantum;

                var speed = minSpeed;
                try
                {
                    var k = (scaledConductance >= 1.0) ? Math.Log10(scaledConductance) : scaledConductance;
                    var x = Math.Abs(scaledConductance - setCond);

                    var factor = (1.0 - Math.Tanh((-1.0 * x + Math.PI / k) * k)) / 2.0;

                    speed = minSpeed + (maxSpeed - minSpeed) * factor;
                }
                catch { speed = minSpeed; }

                motor.Velosity = speed;

                if ((scaledConductance >= setCond - (setCond * condDev / 100.0)) &&
                    (scaledConductance <= setCond + (setCond * condDev / 100.0)))
                {
                    if (motor.IsEnabled == true)
                        motor.Enabled = false;

                    if (!stabilityStopwatch.IsRunning)
                    {
                        inRangeCounter = 0;
                        outsiderCounter = 0;

                        stabilityStopwatch.Start();

                        onStatusChanged(new StatusEventArgs("Stabilizing the specified resistance / conductance value."));
                    }

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

                    if (stabilityStopwatch.IsRunning == true)
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
                        {
                            inRangeCounter = 0;
                            outsiderCounter = 0;

                            stabilityStopwatch.Restart();
                        }
                    }
                }
            }

            onStatusChanged(new StatusEventArgs("Measuring I-V curve(s) in a defined range."));

            var currCycle = 1;

            while (currCycle <= nCycles)
            {
                switch (sourceMode)
                {
                    case SMUSourceMode.Voltage:
                        {
                            var ivData = smu.LinearVoltageSweep(-1.0 * epsilon, maxValue, nPoints);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref ivData)));

                            ivData = smu.LinearVoltageSweep(epsilon, minValue, nPoints);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref ivData)));
                        } break;
                    case SMUSourceMode.Current:
                        {
                            var ivData = smu.LinearCurrentSweep(-1.0 * epsilon, maxValue, nPoints);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref ivData)));

                            ivData = smu.LinearCurrentSweep(epsilon, minValue, nPoints);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref ivData)));
                        } break;
                    case SMUSourceMode.ModeNotSet:
                        throw new ArgumentException();
                }

                ++currCycle;
            }

            smu.SwitchOFF();

            if (motor != null)
                motor.Dispose();
            if (smu != null)
                smu.Dispose();

            this.Stop();

            onStatusChanged(new StatusEventArgs("Measurement is done!"));
        }
    }
}
