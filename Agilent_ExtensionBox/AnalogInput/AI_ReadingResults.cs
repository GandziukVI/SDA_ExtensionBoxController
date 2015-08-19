using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AI_ReadingResults
    {
        private AI_ReadingResult[] _data = new AI_ReadingResult[4]
        {
            new AI_ReadingResult(AnalogInChannelsEnum.AIn1),
            new AI_ReadingResult(AnalogInChannelsEnum.AIn2),
            new AI_ReadingResult(AnalogInChannelsEnum.AIn3),
            new AI_ReadingResult(AnalogInChannelsEnum.AIn4)
        };

        public AI_ReadingResult this[AnalogInChannelsEnum index]
        {
            get
            {
                return _data[(int)index];
            }
        }
    }
}
