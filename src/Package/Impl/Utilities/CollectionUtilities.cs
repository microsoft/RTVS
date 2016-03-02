// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class CollectionUtilities {
        /// <summary>
        /// Update an element with update content
        /// </summary>
        /// <param name="source">an object to be updated</param>
        /// <param name="update">new content for <paramref name="source"/></param>
        public delegate void ElementUpdater<T>(T source, T update);

        /// <summary>
        /// Update a collection to new one in place
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">source collection to which update is applied in place</param>
        /// <param name="update">update collection</param>
        /// <param name="Comparer">Comparer to evaluate equality</param>
        /// <param name="ElementUpdater">delegate update method for same element</param>
        public static void InplaceUpdate<T>(
            this IList<T> source,
            IList<T> update,
            Func<T, T, bool> Comparer,
            ElementUpdater<T> ElementUpdater) where T : class {
            int srcIndex = 0;
            int updateIndex = 0;

            while (srcIndex < source.Count) {
                int sameElementInUpdate = -1;
                for (int u = updateIndex; u < update.Count; u++) {
                    if (Comparer(source[srcIndex], update[u])) {
                        sameElementInUpdate = u;
                        break;
                    }
                }

                if (sameElementInUpdate != -1) {
                    int insertIndex = srcIndex;
                    for (int i = updateIndex; i < sameElementInUpdate; i++) {
                        source.Insert(insertIndex++, update[i]);
                        srcIndex++;
                    }

                    ElementUpdater(source[srcIndex], update[sameElementInUpdate]);
                    srcIndex++;

                    updateIndex = sameElementInUpdate + 1;
                } else {
                    source.RemoveAt(srcIndex);
                }
            }

            if (updateIndex < update.Count) {
                Debug.Assert(srcIndex == source.Count);

                int insertIndex = srcIndex;
                for (int i = updateIndex; i < update.Count; i++) {
                    source.Insert(insertIndex++, update[i]);
                }
            }
        }
    }
}
