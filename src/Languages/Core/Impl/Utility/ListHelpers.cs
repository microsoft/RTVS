// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Languages.Core.Utility {
    public static class ListHelpers {
        public static void RemoveDuplicates<T>(this List<T> list) where T : IComparable<T> {
            if (list == null) {
                throw new ArgumentNullException("list");
            }

            list.Sort();

            int lastFilledIndex = 0;
            for (int i = 1; i < list.Count; i++) {
                if (list[i].CompareTo(list[lastFilledIndex]) != 0) {
                    list[++lastFilledIndex] = list[i];
                }
            }

            int firstUnfilledIndex = lastFilledIndex + 1;
            if (firstUnfilledIndex < list.Count) {
                list.RemoveRange(firstUnfilledIndex, list.Count - firstUnfilledIndex);
            }
        }

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
