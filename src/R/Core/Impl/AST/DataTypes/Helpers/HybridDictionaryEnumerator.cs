// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.R.Core.AST.DataTypes.Helpers {
    internal sealed class HybridDictionaryEnumerator<K, V> : IEnumerator<KeyValuePair<K, V>> {
        private readonly IDictionaryEnumerator _enumerator;

        public HybridDictionaryEnumerator(IDictionaryEnumerator enumerator) {
            _enumerator = enumerator;
        }

        public KeyValuePair<K, V> Current {
            get {
                var de = (DictionaryEntry)_enumerator.Current;
                return new KeyValuePair<K, V>((K)de.Key, (V)de.Value);
            }
        }

        object IEnumerator.Current => _enumerator.Current;
        public void Dispose() { }
        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => _enumerator.Reset();
    }
}
