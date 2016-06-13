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
        public static async Task<T> FindChildAsync<T>(DependencyObject o) where T : DependencyObject {
            if (o is T) {
                return o as T;
            }
            return await UIThreadHelper.Instance.InvokeAsync(() => {
                return VisualTreeExtensions.FindChild<T>(o);
            });
        }
        public static async Task<T> FindNextSiblingAsync<T>(DependencyObject o) where T : DependencyObject {
            return await UIThreadHelper.Instance.InvokeAsync(() => {
                return VisualTreeExtensions.FindNextSibling<T>(o);
            });
        }
    }
}
