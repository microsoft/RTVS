// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Common.Core.Tasks;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public class TestTaskService : ITaskService {
        public bool Wait(Task task, CancellationToken cancellationToken = default(CancellationToken), int ms = Timeout.Infinite) 
            => TaskUtilities.IsOnBackgroundThread() ? task.Wait(ms, cancellationToken) : WaitOnMainThread(task, cancellationToken, ms);
        
        private static bool WaitOnMainThread(Task task, CancellationToken cancellationToken, int ms) {
            var frame = new DispatcherFrame();
            var resultTask = Task.Run(() => {
                try {
                    return task.Wait(ms, cancellationToken);
                } catch(Exception) {
                    return true;
                } finally {
                    frame.Continue = false;
                }
            });

            Dispatcher.PushFrame(frame);

            if (!resultTask.GetAwaiter().GetResult()) {
                return false;
            }

            task.GetAwaiter().GetResult();
            return true;
        }
    }
}