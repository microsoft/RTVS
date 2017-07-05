// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Wpf.Extensions;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class VisualTreeTestExtensions {
        public static async Task<T> FindFirstVisualChildOfType<T>(DependencyObject o) where T : DependencyObject {
            var type = o as T;
            return type ?? 
                await UIThreadHelper.Instance.InvokeAsync(() => VisualTreeExtensions.FindFirstVisualChildOfType<T>(o));
        }
        public static Task<T> FindNextVisualSiblingOfType<T>(DependencyObject o) where T : DependencyObject 
            => UIThreadHelper.Instance.InvokeAsync(() => VisualTreeExtensions.FindNextVisualSiblingOfType<T>(o));
    }
}
