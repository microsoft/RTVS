using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class PageItem<TData> : IIndexedItem, INotifyPropertyChanged {
        public PageItem(int index = -1) {
            Index = index;
        }

        public int Index { get; }

        private TData _data;
        public TData Data {
            get { return _data; }
            set {
                SetData(value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SetData(TData value) {
            SetValue<TData>(ref _data, value, "Data");
        }

        protected virtual bool SetValue<T>(ref T storage, T value, [CallerMemberName]string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value)) {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }

        protected void OnPropertyChanged(string propertyName) {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null) {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString() {
            if (_data != null) {
                return _data.ToString();
            }
            return Index.ToString();
        }
    }
}
