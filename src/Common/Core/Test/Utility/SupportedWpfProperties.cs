// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Common.Core.Test.Utility {
    /// <summary>
    /// Properties written by the <see cref="VisualTreeWriter"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SupportedWpfProperties {
        private static HashSet<string> _hashset;

        public static bool IsSupported(string name) {
            Init();
            return _hashset.Contains(name);
        }

        private static void Init() {
            if (_hashset == null) {
                _hashset = new HashSet<string>();
                foreach (var s in _propertyNames) {
                    _hashset.Add(s);
                }
            }
        }

        private static readonly string[] _propertyNames = {
            "Content",
            "Name",
            "Text",
            "Data",
            "DataContext",
            "Visibility",
            "IsVirtualizing",
            "TextAlignment",
            "ToolTip",
            "ContextMenu",
            "HorizontalAlignment",
            "VerticalAlignment",
            "HorizontalScrollBarVisibility",
            "VerticalScrollBarVisibility",
            "Padding",
            "Margin",
            "ContentTemplate",
        };
    }
}
