// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Tasks;
using Microsoft.R.ExecutionTracing;

namespace Microsoft.R.Host.Client.Test {
    internal static class EventTaskSources {
        public static class IRSession {
            public static readonly EventTaskSource<Client.IRSession, ROutputEventArgs> Output =
                new EventTaskSource<Client.IRSession, ROutputEventArgs>(
                    (o, e) => o.Output += e,
                    (o, e) => o.Output -= e);

            public static readonly EventTaskSource<Client.IRSession, RConnectedEventArgs> Connected =
                new EventTaskSource<Client.IRSession, RConnectedEventArgs>(
                    (o, e) => o.Connected += e,
                    (o, e) => o.Connected -= e);

            public static readonly EventTaskSource<Client.IRSession, EventArgs> Disconnected =
                new EventTaskSource<Client.IRSession, EventArgs>(
                    (o, e) => o.Disconnected += e,
                    (o, e) => o.Disconnected -= e);
        }

        public static class IRExecutionTracer {
            public static readonly EventTaskSource<ExecutionTracing.IRExecutionTracer, RBrowseEventArgs> Browse =
                new EventTaskSource<ExecutionTracing.IRExecutionTracer, RBrowseEventArgs>(
                    (o, e) => o.Browse += e,
                    (o, e) => o.Browse -= e);
        }
    }
}
