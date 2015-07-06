using System.Collections.Generic;

namespace Microsoft.R.Core.AST.DataTypes
{
    internal class Collection<T>
    {
        public static readonly ICollection<T> Empty = new T[0];
    }
    internal class StaticDictionary<K, V>
    {
        public static readonly IReadOnlyDictionary<K, V> Empty = new Dictionary<K, V>(0);
    }
}
