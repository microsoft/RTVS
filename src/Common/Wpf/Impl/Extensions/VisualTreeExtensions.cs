// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Common.Wpf.Extensions {
    public static class VisualTreeExtensions {
        public static T FindChild<T>(DependencyObject o) where T : DependencyObject {
            if (o is T) {
                return o as T;
            }
            int childrenCount = VisualTreeHelper.GetChildrenCount(o);
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(o, i);
                var inner = FindChild<T>(child);
                if (inner != null) {
                    return inner;
                }
            }
            return null;
        }

        public static T FindNextSibling<T>(DependencyObject o) where T : DependencyObject {
            var parent = VisualTreeHelper.GetParent(o);
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            int i = 0;
            for (; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == o) {
                    break;
                }
            }
            i++;
            for (; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T) {
                    return child as T;
                }
            }
            return null;
        }
    }
}
