using System;

namespace Microsoft.Languages.Editor
{
    [Flags]
    public enum CommandStatus : int
    {
        NotSupported = 0,
        Supported = 1,
        Enabled = 2,
        SupportedAndEnabled = 3,
        Latched = 4,
        Invisible = 8,
    }
}
