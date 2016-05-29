// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class VisualTreeExtensions {
        public static async Task<T> FindChildAsync<T>(DependencyObject o) where T : DependencyObject {
            if (o is T) {
                return o as T;
            }
            return await await UIThreadHelper.Instance.InvokeAsync(async () => {
                int childrenCount = VisualTreeHelper.GetChildrenCount(o);
                for (int i = 0; i < childrenCount; i++) {
                    var child = VisualTreeHelper.GetChild(o, i);
                    var inner = await FindChildAsync<T>(child);
                    if (inner != null) {
                        return inner;
                    }
                }
                return null;
            });
        }
        public static async Task<T> FindNextSiblingAsync<T>(DependencyObject o) where T : DependencyObject {
            return await UIThreadHelper.Instance.InvokeAsync(() => {
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
            });
        }
    }
}
