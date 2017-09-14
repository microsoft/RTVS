// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.UnitTests.Core.Threading {
    [ExcludeFromCodeCoverage]
    public static class UIThreadTools {
        public static Task DoEvents() {
            return UIThreadHelper.Instance.DoEventsAsync();
        }

        public static Task InUI(Action action) {
            return UIThreadHelper.Instance.InvokeAsync(action);
        }

        public static Task<TResult> InUI<TResult>(Func<TResult> function) {
            return UIThreadHelper.Instance.InvokeAsync(function);
        }

        public static Task InUI(Func<Task> function) {
            return UIThreadHelper.Instance.InvokeAsync(function).Unwrap();
        }

        public static Task<TResult> InUI<TResult>(Func<Task<TResult>> function) {
            return UIThreadHelper.Instance.InvokeAsync(function).Unwrap();
        }
    }
}