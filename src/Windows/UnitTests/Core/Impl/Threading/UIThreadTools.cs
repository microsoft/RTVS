// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.UnitTests.Core.Threading {
    public static class UIThreadTools {
        public static Task DoEvents() => UIThreadHelper.Instance.DoEventsAsync();
        public static Task InUI(Action action) => UIThreadHelper.Instance.InvokeAsync(action);
        public static Task<TResult> InUI<TResult>(Func<TResult> function) => UIThreadHelper.Instance.InvokeAsync(function);
        public static Task InUI(Func<Task> function) => UIThreadHelper.Instance.InvokeAsync(function).Unwrap();
        public static Task<TResult> InUI<TResult>(Func<Task<TResult>> function) => UIThreadHelper.Instance.InvokeAsync(function).Unwrap();
    }
}