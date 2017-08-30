// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Adapter IList to IRange
    /// </summary>
    internal class ListToRange<T> : IRange<T> {
        private readonly IList<T> _list;

        public ListToRange(Range range, IList<T> list) {
            Check.Argument(nameof(range), () => range.Count <= list.Count);

            Range = range;
            _list = list;
        }

        public Range Range { get; }

        public T this[long index] {
            get => _list[checked((int)(index - Range.Start))];
            set => _list[checked((int)(index - Range.Start))] = value;
        }
    }
}
