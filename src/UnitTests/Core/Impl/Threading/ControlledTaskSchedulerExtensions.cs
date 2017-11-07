// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks.Dataflow;

namespace Microsoft.UnitTests.Core.Threading {
    public static class ControlledTaskSchedulerExtensions {
        public static void Link<T>(this ControlledTaskScheduler scheduler, IReceivableSourceBlock<T> sourceBlock, Action<T> action) {
            sourceBlock.LinkTo(new ActionBlock<T>(action, new ExecutionDataflowBlockOptions { TaskScheduler = scheduler }));
        }

        public static void Wait(this ControlledTaskScheduler scheduler, IDataflowBlock block) {
            scheduler.Wait();
            if (block.Completion.IsFaulted && block.Completion.Exception != null) {
                throw block.Completion.Exception;
            }
        }
    }
}
