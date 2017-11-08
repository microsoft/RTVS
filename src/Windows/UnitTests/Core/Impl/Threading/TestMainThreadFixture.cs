// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.UnitTests.Core.Threading {
    public class TestMainThreadFixture : ITestMainThreadFixture {
        public ITestMainThread CreateTestMainThread() => UIThreadHelper.Instance.CreateTestMainThread();
        public bool CheckAccess() => UIThreadHelper.Instance.MainThread.CheckAccess();
        public Task<T> Invoke<T>(Func<Task<T>> action) => UIThreadHelper.Instance.Invoke(action);
        public void Post(SendOrPostCallback action, object argument) => UIThreadHelper.Instance.SyncContext.Post(action, argument);
    }
}