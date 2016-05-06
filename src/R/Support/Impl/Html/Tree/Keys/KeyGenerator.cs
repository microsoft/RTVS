// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Tree.Keys {
    /// <summary>
    /// Generates unique keys for elements. Keys are used by validation 
    /// in order to track which items are still in a tree and which were 
    /// added or deleted.
    /// </summary>
    internal static class KeyGenerator {
        static int _currentKey = 0;

        /// <summary>
        /// Retrieves next available unique key
        /// </summary>
        /// <returns></returns>
        internal static int GetNextKey() {
            _currentKey++;
            return _currentKey;
        }
    }
}
