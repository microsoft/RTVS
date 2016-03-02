// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Controller {
    [Flags]
    public enum CommandStatus {
        NotSupported = 0,
        Supported = 1,
        Enabled = 2,
        SupportedAndEnabled = 3,
        Latched = 4,
        Invisible = 8,
    }
}
