using ExperimentController;
using MotionManager;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public override void ToDo(object Arg)
        {
            var setCond = 12900.0;
            var condDev = 20.0;
            var stabilizationTime = 1.5;

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

            motor.Position = setPos;
            
            while (true)
            {
                if (!IsRunning)
                    break;

                var scaledConductance = (1.0 / smu.Resistance) / ConductanceQuantum;

                if (scaledConductance > maxConductance)
                    motor.Velosity = maxSpeed;
                else if (scaledConductance < minConductance)
                    motor.Velosity = minSpeed;
                else
                {
                    var speed = maxSpeed - Math.Abs(Math.Log10(scaledConductance) - Math.Log10(maxConductance)) / diff * maxSpeed;
                    motor.Velosity = speed;
                }

                if (scaledConductance > setCond)
                    motor.Position = maxPos;
                else
                    motor.Position = minPos;
                
                if((scaledConductance >= setCond - (setCond * condDev / 100.0)) &&
                    (scaledConductance <= setCond + (setCond * condDev / 100.0)))
                {
                    if(!stabilityStopwatch.IsRunning)
                        stabilityStopwatch.Start();

                    ++inRangeCounter;
                }
                else
                {
                    if (inRangeCounter != 0)
                        ++outsiderCounter;
                }

                if (stabilityStopwatch.IsRunning)
                {
                    if ((double)stabilityStopwatch.ElapsedMilliseconds / 1000.0 >= stabilizationTime)
                    {
                        var divider = outsiderCounter > 0 ? (double)outsiderCounter : 1.0;
                        if ((double)inRangeCounter / divider >= 10.0)
                        {
                            stabilityStopwatch.Stop();
                            break;
                        }
                        else
                            stabilityStopwatch.Restart();
                    }
                }
            }
        }
    }
}
