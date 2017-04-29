// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Threading;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    internal sealed class TestMainThread: IMainThread {
        #region IMainThread
        public int ThreadId => UIThreadHelper.Instance.Thread.ManagedThreadId;

        public void Post(Action action, CancellationToken cancellationToken) =>
            UIThreadHelper.Instance.InvokeAsync(action, cancellationToken).DoNotWait();
        #endregion
    }
}
