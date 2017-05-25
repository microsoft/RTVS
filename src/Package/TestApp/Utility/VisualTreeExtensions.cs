// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Microsoft.Common.Wpf.Extensions;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class VisualTreeTestExtensions {
        public static T FindFirstVisualChildOfType<T>(DependencyObject o) where T : DependencyObject {
            if (o is T) {
                return o as T;
            }
            return UIThreadHelper.Instance.Invoke(() => {
                return VisualTreeExtensions.FindFirstVisualChildOfType<T>(o);
            });
        }
        public static T FindNextVisualSiblingOfType<T>(DependencyObject o) where T : DependencyObject {
            return UIThreadHelper.Instance.Invoke(() => {
                return VisualTreeExtensions.FindNextVisualSiblingOfType<T>(o);
            });
        }
    }
}
