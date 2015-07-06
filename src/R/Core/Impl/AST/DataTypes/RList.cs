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
    public class RList : IRVector<IRVector>, IDictionary<RString, IRVector>
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

        public IRVector this[int index]
        {
            get
            {
                if (this.propeties.IsValueCreated)
                    return (IRVector)propeties.Value[index];

                throw new ArgumentOutOfRangeException("index");
            }
            set
            {
                this.propeties.Value[index] = value;
            }
        }
        #endregion

        #region IDictionary
        public IRVector this[RString key]
        {
            get
            {
                if (this.propeties.IsValueCreated)
                    return (IRVector)this.propeties.Value[key];

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

        public ICollection<IRVector> Values
        {
            get
            {
                List<IRVector> values = new List<IRVector>();

                if (this.propeties.IsValueCreated)
                {
                    foreach (IRVector value in this.propeties.Value.Values)
                    {
                        values.Add(value);
                    }
                }

                return values;
            }
        }

        public void Add(KeyValuePair<RString, IRVector> item)
        {
            this.propeties.Value.Add(item.Key, item.Value);
        }

        public void Add(RString key, IRVector value)
        {
            this.propeties.Value.Add(key, value);
        }

        public void Clear()
        {
            if (this.propeties.IsValueCreated)
                this.propeties.Value.Clear();
        }

        public bool Contains(KeyValuePair<RString, IRVector> item)
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

        public void CopyTo(KeyValuePair<RString, IRVector>[] array, int arrayIndex)
        {
            if (this.propeties.IsValueCreated)
            {
                foreach (DictionaryEntry de in this.propeties.Value)
                {
                    array[arrayIndex++] = new KeyValuePair<RString, IRVector>((RString)de.Key, (IRVector)de.Value);
                }
            }
        }

        public bool Remove(KeyValuePair<RString, IRVector> item)
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

        public bool TryGetValue(RString key, out IRVector value)
        {
            if (this.propeties.IsValueCreated && this.ContainsKey(key))
            {
                value = (IRVector)this.propeties.Value[key];
                return true;
            }

            value = null;
            return false;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<KeyValuePair<RString, IRVector>> GetEnumerator()
        {
            if (this.propeties.IsValueCreated)
            {
                return new HybridDictionaryEnumerator<RString, IRVector>(this.propeties.Value.GetEnumerator());
            }

            return Collection<KeyValuePair<RString, IRVector>>.Empty.GetEnumerator();
        }

        IEnumerator<IRVector> IEnumerable<IRVector>.GetEnumerator()
        {
            if (this.propeties.IsValueCreated)
            {
                return new ListEnumerator<IRVector>(this.propeties.Value.Values.GetEnumerator());
            }

            return Collection<IRVector>.Empty.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.propeties.IsValueCreated)
            {
                return this.propeties.Value.GetEnumerator();
            }

            return Collection<KeyValuePair<RString, IRVector>>.Empty.GetEnumerator();
        }
        #endregion
    }
}
