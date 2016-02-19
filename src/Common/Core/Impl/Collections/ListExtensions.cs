using System;
using System.Collections.Generic;

namespace Microsoft.Common.Core.Collections {
    public static class ListExtensions {

        public static bool AddSorted<T>(this IList<T> list, T value, IComparer<T> comparer = null) {
            var index = list.BinarySearch(value, comparer);
            if (index >= 0) {
                return false;
            }

            list.Insert(~index, value);
            return true;
        }

        public static bool RemoveSorted<T>(this IList<T> list, T value, IComparer<T> comparer = null) {
            var index = list.BinarySearch(value, comparer);
            if (index < 0) {
                return false;
            }

            list.RemoveAt(index);
            return true;
        }

        public static int BinarySearch<T>(this IList<T> list, T value, IComparer<T> comparer = null) {
            if (list == null) {
                throw new ArgumentNullException(nameof(list));
            }

            comparer = comparer ?? Comparer<T>.Default;

            int low = 0;
            int high = list.Count - 1;

            while (low <= high) {
                int mid = low + (high - low) / 2;
                int comparisonResult = comparer.Compare(list[mid], value);

                if (comparisonResult < 0) {
                    low = mid + 1;
                } else if (comparisonResult > 0) {
                    high = mid - 1;
                } else {
                    return mid;
                }
            }

            return ~low;
        }
    }
}