// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Adapter IList to IRange
    /// </summary>
    internal class ListToRange<T> : IRange<T> {
        private IList<T> _list;

        public ListToRange(Range range, IList<T> list) {
            if (range.Count != list.Count) {
                throw new ArgumentException("Range data cound doesn't match with range");
            }

            Range = range;

            _list = list;
        }

        public Range Range { get; }

        public T this[long index] {
            get {
                return _list[checked((int)(index - Range.Start))];
            }

            set {
                _list[checked((int)( index - Range.Start))] = value;
            }
        }
    }
}
