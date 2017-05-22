// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.R.Core.AST.DataTypes.Helpers {
    internal sealed class ListEnumerator<T> : IEnumerator<T> {
        private readonly IEnumerator _enumerator;

        public ListEnumerator(IEnumerator enumerator) {
            _enumerator = enumerator;
        }

        public T Current => (T)_enumerator.Current;
        object IEnumerator.Current => _enumerator.Current;
        public void Dispose() { }
        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
    }
}
