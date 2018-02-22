using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ
{
    [Serializable]
    public class ExtendedDoubleUpDownViewModel : INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;
        private bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (Comparer<T>.Default.Compare(field, value) == 0)
                return false;
            else
            {
                field = value;
                var eventHandler = PropertyChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                }
                return true;
            }

        }

        private double m_value;

        public double Value
        {
            get { return m_value; }
            set
            {
                SetField(ref m_value, value, "Value");
            }
        }



        public double RealValue
        {
            get { return Value * Multiplier; }
        }

        private string[] m_multiplier_strings = new string[] { "", "m", "µ", "n" };
        private double[] m_multiplier_values = new double[] { 1, 1e-3, 1e-6, 1e-9 };

        public string[] MultiplierStrings
        {
            get { return m_multiplier_strings; }
        }


        public double Multiplier
        {
            get
            {
                if (MultiplierIndex < 0 || MultiplierIndex > MultiplierStrings.Length)
                {
                    return m_multiplier_values[0];
                }
                else
                {
                    return m_multiplier_values[MultiplierIndex];
                }
            }
        }

        private int m_multiplier_index;

        public int MultiplierIndex
        {
            get { return m_multiplier_index; }
            set
            {
                SetField(ref m_multiplier_index, value, "MultiplierIndex");
            }
        }


        private string m_unit;

        public string UnitAlias
        {
            get { return m_unit; }
            set
            {
                if (SetField(ref m_unit, value, "UnitAlias"))
                    RefreshUnits();
            }
        }

        private void RefreshUnits()
        {
            for (int i = 0; i < m_multiplier_strings.Length; i++)
            {
                var val = m_multiplier_strings[i];
                
                if (!string.IsNullOrEmpty(val))
                    m_multiplier_strings[i] = val.Substring(0, 1) + UnitAlias;
                else
                    m_multiplier_strings[i] = UnitAlias;
            }
        }

        public ExtendedDoubleUpDownViewModel()
        {

        }

        public ExtendedDoubleUpDownViewModel(string Unit)
        {
            this.UnitAlias = Unit;
            this.RefreshUnits();
        }
    }
}
