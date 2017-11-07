// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

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
        /// <param name="cancellationToken"></param>
        void Post(Action action, CancellationToken cancellationToken = default(CancellationToken));
    }
}