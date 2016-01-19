using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// utility to have multiple access properties
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal class Indexer<TValue> {
        private Func<int, TValue> _getter;
        private Action<int, TValue> _setter;

        public Indexer(Func<int, TValue> getter, Action<int, TValue> setter) {
            _getter = getter;
            _setter = setter;
        }

        public TValue this[int index] {
            get {
                return _getter(index);
            }
            set {
                _setter(index, value);
            }
        }
    }
}
