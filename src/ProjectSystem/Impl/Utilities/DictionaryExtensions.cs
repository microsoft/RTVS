using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities
{
	public static class DictionaryExtensions
	{
		public static TKey GetFirstKeyByValueIgnoreCase<TKey>(this IDictionary<TKey, string> dictionary, string value)
		{
			return dictionary.GetFirstKeyByValue(value, StringComparer.OrdinalIgnoreCase);
		}

		public static TKey GetFirstKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, IEqualityComparer<TValue> comparer)
		{
			comparer = comparer ?? EqualityComparer<TValue>.Default;
            return dictionary.FirstOrDefault(kvp => comparer.Equals(kvp.Value, value)).Key;
		}
	}
}
