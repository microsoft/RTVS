using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes.Definitions;
using Microsoft.R.Core.AST.DataTypes.Helpers;

namespace Microsoft.R.Core.AST.DataTypes
{
    /// <summary>
    /// R List is a vector of elements that can be of different 
    /// modes (data types). Elements are normally named so it is
    /// effectively a dictionary of strings to R objects.
    /// </summary>
    [DebuggerDisplay("[{Length}]")]
    public class RList : RObject, IRVector<RObject>, IDictionary<RString, RObject>
    {
        private Lazy<HybridDictionary> propeties = new Lazy<HybridDictionary>(() => new HybridDictionary());

        #region IRVector
        public RMode Mode
        {
            get { return RMode.List; }
        }

        public int Length
        {
            get { return this.Count; }
        }

        public RObject this[int index]
        {
            get
            {
                if (this.propeties.IsValueCreated)
                    return (RObject)propeties.Value[index];

                throw new ArgumentOutOfRangeException("index");
            }
            set
            {
                this.propeties.Value[index] = value;
            }
        }
        #endregion

        #region IDictionary
        public RObject this[RString key]
        {
            get
            {
                if (this.propeties.IsValueCreated)
                    return (RObject)this.propeties.Value[key];

                throw new ArgumentOutOfRangeException("key");
            }
            set
            {
                this.propeties.Value[key] = value;
            }
        }

        public int Count
        {
            get { return this.propeties.IsValueCreated ? this.propeties.Value.Count : 0; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection<RString> Keys
        {
            get
            {
                List<RString> keys = new List<RString>();

                if (this.propeties.IsValueCreated)
                {
                    foreach (RString key in this.propeties.Value.Keys)
                    {
                        keys.Add(key);
                    }
                }

                return keys;
            }
        }

        public ICollection<RObject> Values
        {
            get
            {
                List<RObject> values = new List<RObject>();

                if (this.propeties.IsValueCreated)
                {
                    foreach (RObject value in this.propeties.Value.Values)
                    {
                        values.Add(value);
                    }
                }

                return values;
            }
        }

        public void Add(KeyValuePair<RString, RObject> item)
        {
            this.propeties.Value.Add(item.Key, item.Value);
        }

        public void Add(RString key, RObject value)
        {
            this.propeties.Value.Add(key, value);
        }

        public void Clear()
        {
            if (this.propeties.IsValueCreated)
                this.propeties.Value.Clear();
        }

        public bool Contains(KeyValuePair<RString, RObject> item)
        {
            if (this.propeties.IsValueCreated)
                return this.propeties.Value.Contains(item.Key);

            return false;
        }

        public bool ContainsKey(RString key)
        {
            if (this.propeties.IsValueCreated)
                return this.propeties.Value.Contains(key);

            return false;
        }

        public void CopyTo(KeyValuePair<RString, RObject>[] array, int arrayIndex)
        {
            if (this.propeties.IsValueCreated)
            {
                foreach (DictionaryEntry de in this.propeties.Value)
                {
                    array[arrayIndex++] = new KeyValuePair<RString, RObject>((RString)de.Key, (RObject)de.Value);
                }
            }
        }

        public bool Remove(KeyValuePair<RString, RObject> item)
        {
            return this.Remove(item.Key);
        }

        public bool Remove(RString key)
        {
            if (this.propeties.IsValueCreated && this.ContainsKey(key))
            {
                this.propeties.Value.Remove(key);
                return true;
            }

            return false;
        }

        public bool TryGetValue(RString key, out RObject value)
        {
            if (this.propeties.IsValueCreated && this.ContainsKey(key))
            {
                value = (RObject)this.propeties.Value[key];
                return true;
            }

            value = null;
            return false;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<KeyValuePair<RString, RObject>> GetEnumerator()
        {
            if (this.propeties.IsValueCreated)
            {
                return new HybridDictionaryEnumerator<RString, RObject>(this.propeties.Value.GetEnumerator());
            }

            return Collection<KeyValuePair<RString, RObject>>.Empty.GetEnumerator();
        }

        IEnumerator<RObject> IEnumerable<RObject>.GetEnumerator()
        {
            if (this.propeties.IsValueCreated)
            {
                return new ListEnumerator<RObject>(this.propeties.Value.Values.GetEnumerator());
            }

            return Collection<RObject>.Empty.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.propeties.IsValueCreated)
            {
                return this.propeties.Value.GetEnumerator();
            }

            return Collection<KeyValuePair<RString, RObject>>.Empty.GetEnumerator();
        }
        #endregion
    }
}
