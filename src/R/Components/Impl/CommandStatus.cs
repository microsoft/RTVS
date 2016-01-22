using System;

namespace Microsoft.R.Components {
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
