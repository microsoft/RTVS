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
    [DebuggerDisplay("[{Length}]")]
    public class RList : RObject, IRVector<RObject>, IDictionary<RString, RObject> {
        private Lazy<HybridDictionary> properties = new Lazy<HybridDictionary>(() => new HybridDictionary());

        #region IRVector
        public RMode Mode {
            get { return RMode.List; }
        }

        public int Length {
            get { return this.Count; }
        }

        public RObject this[int index] {
            get {
                if (this.properties.IsValueCreated) {
                    return (RObject)properties.Value[index];
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set {
                this.properties.Value[index] = value;
            }
        }
        #endregion

        #region IDictionary
        public RObject this[RString key] {
            get {
                if (this.properties.IsValueCreated) {
                    return (RObject)this.properties.Value[key];
                }

                throw new ArgumentOutOfRangeException(nameof(key));
            }
            set {
                this.properties.Value[key] = value;
            }
        }

        public int Count {
            get { return this.properties.IsValueCreated ? this.properties.Value.Count : 0; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public ICollection<RString> Keys {
            get {
                List<RString> keys = new List<RString>();

                if (this.properties.IsValueCreated) {
                    foreach (RString key in this.properties.Value.Keys) {
                        keys.Add(key);
                    }
                }

                return keys;
            }
        }

        public ICollection<RObject> Values {
            get {
                List<RObject> values = new List<RObject>();

                if (this.properties.IsValueCreated) {
                    foreach (RObject value in this.properties.Value.Values) {
                        values.Add(value);
                    }
                }

                return values;
            }
        }

        public void Add(KeyValuePair<RString, RObject> item) {
            this.properties.Value.Add(item.Key, item.Value);
        }

        public void Add(RString key, RObject value) {
            this.properties.Value.Add(key, value);
        }

        public void Clear() {
            if (this.properties.IsValueCreated) {
                this.properties.Value.Clear();
            }
        }

        public bool Contains(KeyValuePair<RString, RObject> item) {
            if (this.properties.IsValueCreated) {
                return this.properties.Value.Contains(item.Key);
            }

            return false;
        }

        public bool ContainsKey(RString key) {
            if (this.properties.IsValueCreated) {
                return this.properties.Value.Contains(key);
            }

            return false;
        }

        public void CopyTo(KeyValuePair<RString, RObject>[] array, int arrayIndex) {
            if (this.properties.IsValueCreated) {
                foreach (DictionaryEntry de in this.properties.Value) {
                    array[arrayIndex++] = new KeyValuePair<RString, RObject>((RString)de.Key, (RObject)de.Value);
                }
            }
        }

        public bool Remove(KeyValuePair<RString, RObject> item) {
            return this.Remove(item.Key);
        }

        public bool Remove(RString key) {
            if (this.properties.IsValueCreated && this.ContainsKey(key)) {
                this.properties.Value.Remove(key);
                return true;
            }

            return false;
        }

        public bool TryGetValue(RString key, out RObject value) {
            if (this.properties.IsValueCreated && this.ContainsKey(key)) {
                value = (RObject)this.properties.Value[key];
                return true;
            }

            value = null;
            return false;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<KeyValuePair<RString, RObject>> GetEnumerator() {
            if (this.properties.IsValueCreated) {
                return new HybridDictionaryEnumerator<RString, RObject>(this.properties.Value.GetEnumerator());
            }

            return Collection<KeyValuePair<RString, RObject>>.Empty.GetEnumerator();
        }

        IEnumerator<RObject> IEnumerable<RObject>.GetEnumerator() {
            if (this.properties.IsValueCreated) {
                return new ListEnumerator<RObject>(this.properties.Value.Values.GetEnumerator());
            }

            return Collection<RObject>.Empty.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            if (this.properties.IsValueCreated) {
                return this.properties.Value.GetEnumerator();
            }

            return Collection<KeyValuePair<RString, RObject>>.Empty.GetEnumerator();
        }
        #endregion
    }
}
