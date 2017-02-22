// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// utility to have multiple access properties
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal class Indexer<TValue> {
        private Func<long, TValue> _getter;
        private Action<long, TValue> _setter;

        public Indexer(Func<long, TValue> getter, Action<long, TValue> setter) {
            _getter = getter;
            _setter = setter;
        }

        public TValue this[long index] {
            get {
                return _getter(index);
            }
            set {
                _setter(index, value);
            }
        }
    }
}
