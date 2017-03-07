using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltageApply
{
    public class VoltagesApplySettings : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        void onPropertyChanged(string PropertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion

        private double vDrainSource = 0.1;
        public double VDrainSource
        {
            get { return vDrainSource; }
            set 
            {
                vDrainSource = value;
                onPropertyChanged("VDrainSource");
            }
        }

        private double vDSAccuracy = 0.002;
        public double VDSAccuracy
        {
            get { return vDSAccuracy; }
            set 
            {
                vDSAccuracy = value;
                onPropertyChanged("VDSAccuracy");
            }
        }

        private double vGate = 0.0;
        public double VGate
        {
            get { return vGate; }
            set
            { 
                vGate = value;
                onPropertyChanged("VGate");
            }
        }

        private double vGateAccuracy = 0.005;
        public double VGateAccuracy
        {
            get { return vGateAccuracy; }
            set
            {
                vGateAccuracy = value; 
            }
        }
    }
}
