// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.R.Editor.Functions {
    internal sealed class PersistentFunctionInfo : IPersistentFunctionInfo {
        public NamedItemType ItemType => NamedItemType.Function;
        public string Name { get; }
        public string Description => string.Empty;
        public bool IsInternal { get; }

        public PersistentFunctionInfo(string name, bool isInternal) {
            Check.ArgumentStringNullOrEmpty(nameof(name), name);
            Name = name;
            IsInternal = isInternal;
        }

        public static bool TryParse(string s, out IPersistentFunctionInfo info) {
            info = null;

            var start = s.IndexOf('`');
            var end = s.LastIndexOf('`');

            if (start < 0 || end < 0 || start >= end) {
                return false;
            }
            if (!bool.TryParse(s.Substring(end + 1), out bool isInternal)) {
                return false;
            }
            info = new PersistentFunctionInfo(s.Substring(start + 1, end - start - 1), isInternal);
            return true;
        }
    }
}
