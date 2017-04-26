// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Languages.Core {
    public static class ListExtensions {
        public static void RemoveDuplicates<T>(this List<T> list, IComparer<T> comparison) where T : class {
            Check.ArgumentNull(nameof(list), list);

            var lastFilledIndex = 0;
            for (var i = 1; i < list.Count; i++) {
                if ((comparison != null && comparison.Compare(list[i], list[lastFilledIndex]) != 0) ||
                    comparison.Compare(list[i], list[lastFilledIndex]) != 0) {
                    list[++lastFilledIndex] = list[i];
                }
            }

            var firstUnfilledIndex = lastFilledIndex + 1;
            if (firstUnfilledIndex < list.Count) {
                list.RemoveRange(firstUnfilledIndex, list.Count - firstUnfilledIndex);
            }
        }
    }
}
