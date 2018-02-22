using ControlAssist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomControls.ViewModels
{
    public class ExtendedDoubleUpDownViewModel : NotifyPropertyChangedBase
    {
        public ExtendedDoubleUpDownViewModel() { }
        public ExtendedDoubleUpDownViewModel(string Unit)
        {
            this.UnitAlias = Unit;
            this.RefreshUnits();
        }

        private double mValue;
        public double Value
        {
            get { return mValue; }
            set
            {
                SetField(ref mValue, value, "Value");
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

        private int mMultiplierIndex;
        public int MultiplierIndex
        {
            get { return mMultiplierIndex; }
            set
            {
                SetField(ref mMultiplierIndex, value, "MultiplierIndex");
            }
        }


        private string mUnit;
        public string UnitAlias
        {
            get { return mUnit; }
            set
            {
                if (SetField(ref mUnit, value, "UnitAlias"))
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
    }
}
