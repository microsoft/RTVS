// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Threading;

namespace Microsoft.Common.Wpf.Threading {
    public struct MainThreadAwaitable {
        private readonly IMainThread _mainThread;

        public MainThreadAwaitable(IMainThread mainThread) {
            _mainThread = mainThread;
        }

        public MainThreadAwaiter GetAwaiter() {
            return new MainThreadAwaiter(_mainThread);
        }
    }
}