using System;
using System.ComponentModel;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// double that updates only when bigger value is assigned
    /// </summary>
    public class MaxDouble {
        public MaxDouble() : this(double.NegativeInfinity) { }

        public MaxDouble(double initialValue) {
            _max = initialValue;
        }

        private double _max;
        /// <summary>
        /// Maximum value
        /// </summary>
        public double Max
        {
            get
            {
                return _max;
            }
            set
            {
                SetValue(value);
            }
        }

        public event EventHandler MaxChanged;

        private void SetValue(double value) {
            if (MaxChanged != null) {
                if (value > _max) {
                    _max = value;
                    MaxChanged(this, EventArgs.Empty);
                }
            } else {
                _max = Math.Max(_max, value);
            }
        }
    }
}
