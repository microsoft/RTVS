using System.Collections;
using System.Collections.Generic;

namespace Microsoft.R.Core.AST.DataTypes.Helpers
{
    internal sealed class HybridDictionaryEnumerator<K, V> : IEnumerator<KeyValuePair<K, V>>
    {
        private IDictionaryEnumerator enumerator;

        public HybridDictionaryEnumerator(IDictionaryEnumerator enumerator)
        {
            this.enumerator = enumerator;
        }

        public KeyValuePair<K, V> Current
        {
            get
            {
                DictionaryEntry de = (DictionaryEntry)this.enumerator.Current;
                return new KeyValuePair<K, V>((K)de.Key, (V)de.Value);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.enumerator.Current;
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return this.enumerator.MoveNext();
        }

        public void Reset()
        {
            this.enumerator.Reset();
        }
    }
}
