using System;
using System.ComponentModel;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// double that updates only when bigger value is assigned
    /// </summary>
    public class MaxDouble {
        public MaxDouble() { }
        public MaxDouble(double initialValue) {
            _max = initialValue;
        }

        /// <summary>
        /// bool 
        /// </summary>
        [DefaultValue(false)]
        public bool Frozen { get; set; }


        double? _max;
        /// <summary>
        /// Maximum value
        /// </summary>
        public double? Max {
            get {
                return _max;
            }
            set {
                if (!Frozen) {
                    if (_max.HasValue && value.HasValue) {
                        _max = Math.Max(_max.Value, value.Value);
                    } else {
                        _max = value;
                    }
                }
            }
        }
    }
}
