// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.UI.Commands {
    [Flags]
    public enum CommandStatus {
        NotSupported = 0,
        Supported = 1,
        Enabled = 2,
        Latched = 4,
        Invisible = 8,

        SupportedAndEnabled = Supported | Enabled,
        SupportedAndInvisible = Supported | Invisible,
    }
}
