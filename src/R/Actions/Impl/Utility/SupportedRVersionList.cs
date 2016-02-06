using System;

namespace Microsoft.R.Actions.Utility {
    public static class SupportedRVersionList {
        // TODO: this probably needs configuration file
        // or another dynamic source of supported versions.
        public const int MinMajorVersion = 3;
        public const int MinMinorVersion = 2;
        public const int MaxMajorVersion = 3;
        public const int MaxMinorVersion = 2;

        public static bool IsCompatibleVersion(Version v) {
            return v.Major >= MinMajorVersion && v.Minor >= MinMinorVersion &&
                   v.Major <= MaxMajorVersion && v.Minor <= MaxMinorVersion;
        }
    }
}
