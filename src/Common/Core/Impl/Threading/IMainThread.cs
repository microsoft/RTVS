// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Threading {
    public interface IMainThread {
        /// <summary>
        /// Main (UI) thread id. 
        /// </summary>
        int ThreadId { get; }

        /// <summary>
        /// Posts cancellable action on UI thread.
        /// </summary>
        /// <param name="action"></param>
        void Post(Action action);

        /// <summary>
        /// Creates main thread awaiter implementation
        /// </summary>
        IMainThreadAwaiter CreateMainThreadAwaiter(CancellationToken cancellationToken);
    }
}