using ExperimentController;
using MotionManager;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments
{
    class IV_DataComparer : IComparer<IV_Data>
    {
        int IComparer<IV_Data>.Compare(IV_Data x, IV_Data y)
        {
            return x.Voltage.CompareTo(y.Voltage);
        }
    }
    public class IV_DefinedResistance : ExperimentBase
    {
        private ISourceMeterUnit smu;
        private IMotionController1D motor;

        private readonly double ConductanceQuantum = 0.0000774809173;

        private Stopwatch stabilityStopwatch;


        LinkedList<IV_Data[]> IVData;

        public IV_DefinedResistance(ISourceMeterUnit SMU, IMotionController1D Motor)
            : base()
        {
            smu = SMU;
            motor = Motor;

            stabilityStopwatch = new Stopwatch();

            IVData = new LinkedList<IV_Data[]>();
        }

        private string getIVString(ref IV_Data[] Data)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Data.Length; i++)
                sb.AppendFormat("{0}\t{1}\t{2}\r\n", Data[i].Time.ToString(NumberFormatInfo.InvariantInfo), Data[i].Voltage.ToString(NumberFormatInfo.InvariantInfo), Data[i].Current.ToString(NumberFormatInfo.InvariantInfo));

            return sb.ToString();
        }

        public override void ToDo(object Arg)
        {
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

            var fileName = string.Join("\\", settings.FilePath, settings.SaveFileName);

            var inRangeCounter = 0;
            var outsiderCounter = 0;

            onStatusChanged(new StatusEventArgs("Starting the measurement."));
            onProgressChanged(new ProgressEventArgs(0.0));

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
                    if (stabilityStopwatch.ElapsedMilliseconds > 0)
                        onProgressChanged(new ProgressEventArgs((double)stabilityStopwatch.ElapsedMilliseconds / 1000.0 / stabilizationTime * 100));

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
                            var ivDataFirstBranch = smu.LinearVoltageSweep(-1.0 * epsilon, maxValue, nPoints);
                            var ivDataSecondBranch = smu.LinearVoltageSweep(epsilon, minValue, nPoints);

                            IV_Data[] res;

                            selectIV_Data(ref ivDataFirstBranch, ref ivDataSecondBranch, out res);
                            IVData.AddLast(res);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref res)));
                        } break;
                    case SMUSourceMode.Current:
                        {
                            var ivDataFirstBranch = smu.LinearCurrentSweep(-1.0 * epsilon, maxValue, nPoints);
                            var ivDataSecondBranch = smu.LinearCurrentSweep(epsilon, minValue, nPoints);

                            IV_Data[] res;

                            selectIV_Data(ref ivDataFirstBranch, ref ivDataSecondBranch, out res);
                            IVData.AddLast(res);

                            onDataArrived(new ExpDataArrivedEventArgs(getIVString(ref res)));
                        } break;
                    case SMUSourceMode.ModeNotSet:
                        throw new ArgumentException();
                }

                ++currCycle;
            }

            smu.SwitchOFF();

            onStatusChanged(new StatusEventArgs("Saving data to file."));
            SaveToFile(fileName);
            onStatusChanged(new StatusEventArgs("Measurement is done!"));

            if (motor != null)
                motor.Dispose();
            if (smu != null)
                smu.Dispose();

            this.Stop();
        }

        void selectIV_Data(ref IV_Data[] positiveBranch, ref IV_Data[] negativeBranch, out IV_Data[] res)
        {
            var dataComparer = new IV_DataComparer();

            var first = (from item in positiveBranch
                         where item.Voltage >= 0
                         select item).ToArray();
            var second = (from item in negativeBranch
                          where item.Voltage < 0
                          select item).ToArray();

            res = new IV_Data[first.Length + second.Length];

            Array.Copy(first, res, first.Length);
            Array.Copy(second, 0, res, first.Length, second.Length);
            Array.Sort(res, dataComparer);
        }

        public override void SaveToFile(string FileName)
        {
            var formatBiulder = new StringBuilder();
            var dataBuilder = new StringBuilder();

            var counter = 0;
            for (int i = 0; i < IVData.Count; i++)
            {
                formatBiulder.AppendFormat("{{0}}\t{{1}}\t", counter.ToString(NumberFormatInfo.InvariantInfo), (counter + 1).ToString(NumberFormatInfo.InvariantInfo));
                counter += 2;
            }

            var stringFormat = formatBiulder.ToString().TrimEnd('\t') + "\r\n";

            var minLen = IVData.First.Value.Length;

            foreach (var item in IVData)
            {
                if (item.Length > minLen)
                    minLen = item.Length;
            }

            for (int i = 0; i < minLen; i++)
            {
                counter = 0;
                var arr = new string[IVData.Count * 2];
                for (int j = 0; j < IVData.Count; j++)
                {
                    arr[counter] = IVData.ElementAt(j)[i].Voltage.ToString(NumberFormatInfo.InvariantInfo);
                    arr[counter + 1] = IVData.ElementAt(j)[i].Current.ToString(NumberFormatInfo.InvariantInfo);

                    counter += 2;
                }

                dataBuilder.AppendFormat(stringFormat, arr);
            }

            using (var writer = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(dataBuilder.ToString());
            }
        }
    }
}
