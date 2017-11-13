// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Tasks;

namespace Microsoft.Common.Core.OS {
    public static class ProcessExtensions {
        public static Task WaitForExitAsync(this IProcess process, int milliseconds, CancellationToken cancellationToken = default(CancellationToken)) {
            var tcs = new TaskCompletionSource<int>();
            process.Exited += (o, e) => tcs.TrySetResult(0);
            tcs.RegisterForCancellation(milliseconds, cancellationToken).UnregisterOnCompletion(tcs.Task);
            return tcs.Task;
        }
    }
}