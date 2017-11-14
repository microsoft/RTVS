// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.UnitTests.Core.XUnit {
    public interface ITestMainThread : IDisposable {
        void CancelPendingTasks();
    }
}