using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AI_Channels
    {
        private AI_Channel[] _Items;
        public AI_Channel[] Items { get { return _Items; } }

        public AI_Channels(BoxController __Controller)
        {
            _Items = new AI_Channel[] { new AI_Channel(1, __Controller.Driver), new AI_Channel(2, __Controller.Driver), new AI_Channel(3, __Controller.Driver), new AI_Channel(4, __Controller.Driver) };
        }
    }
}
