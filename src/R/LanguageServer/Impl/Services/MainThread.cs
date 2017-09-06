// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.LanguageServer.Services {
    internal sealed class MainThread : IMainThread {
        public int ThreadId { get; } = Thread.CurrentThread.ManagedThreadId;
        public void Post(Action action, CancellationToken cancellationToken = new CancellationToken()) => action();
    }
}
