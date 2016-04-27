// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Windows.Threading;

namespace Microsoft.Common.Wpf.Threading {
    public struct MainThreadAwaitable {
        private readonly Dispatcher _dispatcher;

        public MainThreadAwaitable(Thread mainThread) {
            _dispatcher = Dispatcher.FromThread(mainThread);
        }

        public MainThreadAwaitable(Dispatcher dispatcher) {
            _dispatcher = dispatcher;
        }

        public MainThreadAwaiter GetAwaiter() {
            return new MainThreadAwaiter(_dispatcher);
        }
    }
}