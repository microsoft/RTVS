// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.UnitTests.Core.XUnit {
    /// <summary>
    /// Temporary workaround to avoid massive changes in test attributes
    /// Provides access to UIThreadHelper for test infrastructure
    /// Implemented as assembly fixture 
    /// </summary>
    public interface ITestMainThreadFixture {
        ITestMainThread CreateTestMainThread();
        bool CheckAccess();
        void Post(Action<object> action, object argument);
    }
}