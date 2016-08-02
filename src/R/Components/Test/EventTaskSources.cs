// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Test {
    internal static class EventTaskSources {
        public static class IRSession {
            public static readonly EventTaskSource<Host.Client.IRSession, EventArgs> Mutated =
                new EventTaskSource<Host.Client.IRSession, EventArgs>(
                    (o, e) => o.Mutated += e,
                    (o, e) => o.Mutated -= e);

            public static readonly EventTaskSource<Host.Client.IRSession, RAfterRequestEventArgs> AfterRequest =
                new EventTaskSource<Host.Client.IRSession, RAfterRequestEventArgs>(
                    (o, e) => o.AfterRequest += e,
                    (o, e) => o.AfterRequest -= e);
        }
    }
}
