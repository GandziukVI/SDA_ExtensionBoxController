using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FET_Characterization.Experiments
{
    public class AdvancedValueSetter
    {
        public AdvancedValueSetter()
        {
            performingCollection = new List<Func<bool>>();
            performingIndex = -1;
        }

        #region Public Functions

        public void RegisterStep(ref Func<bool> workingFunction)
        {
            performingCollection.Add(workingFunction);
        }

        public void SetPrev()
        {
            if (HasPrev())
                --performingIndex;
        }

        public void SetNext()
        {
            if (HasNext())
                ++performingIndex;
        }

        public void Reset()
        {
            performingCollection.Clear();
        }

        public void SetValue()
        {
            while (HasNext())
            {
                SetNext();
                var currentFunc = performingCollection[performingIndex];
                if (!currentFunc.Invoke())
                    SetPrev();
            }
        }

        #endregion

        #region Private Members

        private List<Func<bool>> performingCollection;
        private int performingIndex;

        #endregion

        #region Private Functions

        private bool HasPrev()
        {
            return performingIndex > 0;
        }

        private bool HasNext()
        {
            return (performingIndex < performingCollection.Count - 1) && (performingCollection.Count > 0);
        }

        #endregion
    }
}
