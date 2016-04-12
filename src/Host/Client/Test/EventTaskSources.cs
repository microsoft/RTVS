// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Tasks;

namespace Microsoft.R.Host.Client.Test {
    internal static class EventTaskSources {
        public static class IRSession {
            public static readonly EventTaskSource<Microsoft.R.Host.Client.IRSession, Microsoft.R.Host.Client.ROutputEventArgs> Output =
                new EventTaskSource<Microsoft.R.Host.Client.IRSession, Microsoft.R.Host.Client.ROutputEventArgs>(
                    (o, e) => o.Output += e,
                    (o, e) => o.Output -= e);
        }
    }
}
