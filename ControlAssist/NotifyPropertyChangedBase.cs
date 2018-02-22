using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAssist
{
    [Serializable]
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;
        public bool SetField<T>(ref T field, T value, string propertyName)
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
    }
}
