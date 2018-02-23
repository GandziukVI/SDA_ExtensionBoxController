using ControlAssist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomControls.ViewModels
{
    [Serializable]
    public class ExtendedDoubleUpDownViewModel : NotifyPropertyChangedBase
    {
        public ExtendedDoubleUpDownViewModel() 
        {
            if (mMultiplierStrings == null)
            {
                mMultiplierStrings = new string[mDefaultMultiplierStrings.Length];
                Array.Copy(mDefaultMultiplierStrings, mMultiplierStrings, mDefaultMultiplierStrings.Length);
            }
        }
        public ExtendedDoubleUpDownViewModel(string Unit)
        {
            this.UnitAlias = Unit;
            this.RefreshUnits();

            if (mMultiplierStrings == null)
            {
                mMultiplierStrings = new string[mDefaultMultiplierStrings.Length];
                Array.Copy(mDefaultMultiplierStrings, mMultiplierStrings, mDefaultMultiplierStrings.Length);
            }
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

        private readonly string[] mDefaultMultiplierStrings = new string[] { "", "m", "µ", "n" };
        private string[] mMultiplierStrings;
        private double[] mMultiplierValues = new double[] { 1, 1e-3, 1e-6, 1e-9 };

        public string[] MultiplierStrings
        {
            get { return mMultiplierStrings; }
            private set { SetField(ref mMultiplierStrings, value, "MultiplierStrings"); }
        }

        public double Multiplier
        {
            get
            {
                if (MultiplierIndex < 0 || MultiplierIndex > MultiplierStrings.Length)
                {
                    return mMultiplierValues[0];
                }
                else
                {
                    return mMultiplierValues[MultiplierIndex];
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
            var newMultiplierStrings = new string[mMultiplierStrings.Length];
            Array.Copy(mDefaultMultiplierStrings, newMultiplierStrings, mDefaultMultiplierStrings.Length);

            for (int i = 0; i < mMultiplierStrings.Length; i++)
                newMultiplierStrings[i] += UnitAlias;

            var index = MultiplierIndex;
            
            MultiplierStrings = newMultiplierStrings;
            MultiplierIndex = index;
        }
    }
}
