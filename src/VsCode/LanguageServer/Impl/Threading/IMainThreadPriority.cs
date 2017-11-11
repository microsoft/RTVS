// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.LanguageServer.Threading {
    internal interface IMainThreadPriority: IMainThread {
        void Post(Action action, ThreadPostPriority priority);
        Task<T> SendAsync<T>(Func<Task<T>> action, ThreadPostPriority priority);
        void CancelIdle();
    }
}
