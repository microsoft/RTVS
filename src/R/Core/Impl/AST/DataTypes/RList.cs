// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes.Definitions;
using Microsoft.R.Core.AST.DataTypes.Helpers;

namespace Microsoft.R.Core.AST.DataTypes {
    /// <summary>
    /// R List is a vector of elements that can be of different 
    /// modes (data types). Elements are normally named so it is
    /// effectively a dictionary of strings to R objects.
    /// </summary>
    [DebuggerDisplay("[{" + nameof(Length) + "}]")]
    public class RList : RObject, IRVector<RObject>, IDictionary<RString, RObject> {
        private Lazy<HybridDictionary> properties = new Lazy<HybridDictionary>(() => new HybridDictionary());

        #region IRVector
        public RMode Mode => RMode.List;
        public int Length => Count;

        public RObject this[int index] {
            get {
                if (properties.IsValueCreated) {
                    return (RObject)properties.Value[index];
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set => properties.Value[index] = value;
        }
        #endregion

        #region IDictionary
        public RObject this[RString key] {
            get {
                if (properties.IsValueCreated) {
                    return (RObject)properties.Value[key];
                }

                throw new ArgumentOutOfRangeException(nameof(key));
            }
            set => properties.Value[key] = value;
        }

        public int Count => properties.IsValueCreated ? properties.Value.Count : 0;

        public bool IsReadOnly => false;

        public ICollection<RString> Keys {
            get {
                var keys = new List<RString>();

                if (properties.IsValueCreated) {
                    foreach (RString key in properties.Value.Keys) {
                        keys.Add(key);
                    }
                }

                return keys;
            }
        }

        public ICollection<RObject> Values {
            get {
                var values = new List<RObject>();

                if (properties.IsValueCreated) {
                    foreach (RObject value in properties.Value.Values) {
                        values.Add(value);
                    }
                }

                return values;
            }
        }

        public void Add(KeyValuePair<RString, RObject> item) => properties.Value.Add(item.Key, item.Value);
        public void Add(RString key, RObject value) => properties.Value.Add(key, value);

        public void Clear() {
            if (properties.IsValueCreated) {
                properties.Value.Clear();
            }
        }

        public bool Contains(KeyValuePair<RString, RObject> item) {
            if (properties.IsValueCreated) {
                return properties.Value.Contains(item.Key);
            }

            return false;
        }

        public bool ContainsKey(RString key) => properties.IsValueCreated && properties.Value.Contains(key);

        public void CopyTo(KeyValuePair<RString, RObject>[] array, int arrayIndex) {
            if (properties.IsValueCreated) {
                foreach (DictionaryEntry de in properties.Value) {
                    array[arrayIndex++] = new KeyValuePair<RString, RObject>((RString)de.Key, (RObject)de.Value);
                }
            }
        }

        public bool Remove(KeyValuePair<RString, RObject> item) => Remove(item.Key);

        public bool Remove(RString key) {
            if (properties.IsValueCreated && ContainsKey(key)) {
                properties.Value.Remove(key);
                return true;
            }

            return false;
        }

        public bool TryGetValue(RString key, out RObject value) {
            if (properties.IsValueCreated && ContainsKey(key)) {
                value = (RObject)properties.Value[key];
                return true;
            }

            value = null;
            return false;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<KeyValuePair<RString, RObject>> GetEnumerator() {
            if (properties.IsValueCreated) {
                return new HybridDictionaryEnumerator<RString, RObject>(properties.Value.GetEnumerator());
            }

            return Collection<KeyValuePair<RString, RObject>>.Empty.GetEnumerator();
        }

        IEnumerator<RObject> IEnumerable<RObject>.GetEnumerator() {
            if (properties.IsValueCreated) {
                return new ListEnumerator<RObject>(properties.Value.Values.GetEnumerator());
            }

            return Collection<RObject>.Empty.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            if (properties.IsValueCreated) {
                return properties.Value.GetEnumerator();
            }

            return Collection<KeyValuePair<RString, RObject>>.Empty.GetEnumerator();
        }
        #endregion
    }
}
