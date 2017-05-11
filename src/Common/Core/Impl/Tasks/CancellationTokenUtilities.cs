// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Tasks {
    public static class CancellationTokenUtilities {
        public static void UnregisterOnCompletion(this CancellationTokenRegistration registration, Task task) => task.ContinueWith(UnregisterCancellationToken, registration, default(CancellationToken), TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

        private static void UnregisterCancellationToken(Task task, object state) => ((CancellationTokenRegistration)state).Dispose();

        public static CancellationTokenSource Link(ref CancellationToken ct1, CancellationToken ct2) {
            try {
                // First try to link
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct1, ct2);
                ct1 = cts.Token;
                return cts;
            } catch (ObjectDisposedException) {
                ct1.ThrowIfCancellationRequested();
                ct2.ThrowIfCancellationRequested();

                // If can't link and no cancellation requested, try to wrap ct1
                if (ct1.CanBeCanceled) {
                    try {
                        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct1);
                        ct1 = cts.Token;
                        return cts;
                    } catch (ObjectDisposedException) {
                        ct1.ThrowIfCancellationRequested();
                    }
                }

                return new CancellationTokenSource();
            }
        }
    }
}