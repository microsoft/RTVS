// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Threading;
using Microsoft.Common.Core.Tasks;

namespace Microsoft.UnitTests.Core {
    public static class EventTaskSources {
        public static class Dispatcher {
            public static readonly EventTaskSource<System.Windows.Threading.Dispatcher, DispatcherUnhandledExceptionEventHandler, DispatcherUnhandledExceptionEventArgs> UnhandledException =
                new EventTaskSource<System.Windows.Threading.Dispatcher, DispatcherUnhandledExceptionEventHandler, DispatcherUnhandledExceptionEventArgs>(
                    (o, e) => o.UnhandledException += e,
                    (o, e) => o.UnhandledException -= e,
                    a => (o, e) => a(o, e));
        }
    }
}
