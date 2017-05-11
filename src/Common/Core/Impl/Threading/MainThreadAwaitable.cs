// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;

namespace Microsoft.Common.Core.Threading {
    public struct MainThreadAwaitable {
        private readonly IMainThread _mainThread;
        private readonly CancellationToken _cancellationToken;

        public MainThreadAwaitable(IMainThread mainThread, CancellationToken cancellationToken = default(CancellationToken)) {
            _mainThread = mainThread;
            _cancellationToken = cancellationToken;
        }

        public MainThreadAwaiter GetAwaiter() => new MainThreadAwaiter(_mainThread, _cancellationToken);
    }
}