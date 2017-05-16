// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.Common.Core.Testing {
    public static class TestEnvironment {
        private static ITestEnvironment _current;

        public static ITestEnvironment Current {
            get => _current;
            set {
                var oldValue = Interlocked.CompareExchange(ref _current, value, null);
                if (oldValue != null) {
                    throw new InvalidOperationException("Only one test environment can be set per app domain");
                }
            }
        }
    }
}
