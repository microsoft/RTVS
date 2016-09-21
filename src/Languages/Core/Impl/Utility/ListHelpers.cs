// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Languages.Core.Utility {
    public static class ListHelpers {
        public static void RemoveDuplicates<T>(this List<T> list, Comparison<T> comparison = null) where T : IComparable<T> {
            if (list == null) {
                throw new ArgumentNullException(nameof(list));
            }

            list.Sort();

            int lastFilledIndex = 0;
            for (int i = 1; i < list.Count; i++) {
                if((comparison != null && comparison(list[i], list[lastFilledIndex]) != 0) ||
                    list[i].CompareTo(list[lastFilledIndex]) != 0) {
                    list[++lastFilledIndex] = list[i];
                }
            }

            int firstUnfilledIndex = lastFilledIndex + 1;
            if (firstUnfilledIndex < list.Count) {
                list.RemoveRange(firstUnfilledIndex, list.Count - firstUnfilledIndex);
            }
        }
    }
}
