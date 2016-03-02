// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Common.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class SequentialTestExecutor {
        [ExcludeFromCodeCoverage]
        class ExecutionRequest {
            public ManualResetEventSlim Event { get; private set; }
            public Action<ManualResetEventSlim> Action { get; private set; }

            public ExecutionRequest(Action<ManualResetEventSlim> action) {
                Action = action;
                Event = new ManualResetEventSlim();
            }
        }

        private static readonly object _creatorLock = new object();
        private static Action _disposeAction;

        public static void ExecuteTest(Action<ManualResetEventSlim> action) {
            ExecuteTest(action, null, null);
        }

        public static void ExecuteTest(Action<ManualResetEventSlim> action, Action initAction, Action disposeAction) {
            lock (_creatorLock) {
                _disposeAction = disposeAction;
                AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

                if (initAction != null) {
                    initAction();
                }

                using (var evt = new ManualResetEventSlim()) {
                    action(evt);
                    evt.Wait();
                }
            }
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e) {
            if (_disposeAction != null) {
                _disposeAction();
                _disposeAction = null;
            }
        }
    }
}
