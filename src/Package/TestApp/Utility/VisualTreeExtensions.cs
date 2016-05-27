// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class VisualTreeExtensions {
        public static T FindChild<T>(DependencyObject o) where T : DependencyObject {
            if(o is T) {
                return o as T;
            }
            int childrenCount = VisualTreeHelper.GetChildrenCount(o);
            if (childrenCount > 0) {
                for (int i = 0; i < childrenCount; i++) {
                    var child = VisualTreeHelper.GetChild(o, i);
                    return FindChild<T>(child);
                }
            }
            return null;
        }
    }
}
