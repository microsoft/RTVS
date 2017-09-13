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
        /// <param name="cancellationToken"></param>
        void Post(Action action, CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>
        /// Executed action on UI thread synchronously.
        /// </summary>
        /// <param name="action"></param>
        void Send(Action action);

        /// <summary>
        /// Executed cancellable action on UI thread.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        Task SendAsync(Action action, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Executed cancellable action on UI thread and return result.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        Task<T> InvokeAsync<T>(Func<T> action, CancellationToken cancellationToken = default(CancellationToken));
    }
}