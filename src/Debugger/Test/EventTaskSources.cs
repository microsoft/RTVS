// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Tasks;

namespace Microsoft.R.Debugger.Test {
    internal static class EventTaskSources {
        public static class DebugSession {
            public static readonly EventTaskSource<Microsoft.R.Debugger.DebugSession, DebugBrowseEventArgs> Browse =
                new EventTaskSource<Microsoft.R.Debugger.DebugSession, DebugBrowseEventArgs>(
                    (o, e) => o.Browse += e,
                    (o, e) => o.Browse -= e);
        }
    }
}
