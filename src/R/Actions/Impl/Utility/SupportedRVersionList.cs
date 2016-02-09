using System;

namespace Microsoft.R.Actions.Utility {
    public static class SupportedRVersionList {
        // TODO: this probably needs configuration file
        // or another dynamic source of supported versions.
        public const int MinMajorVersion = 3;
        public const int MinMinorVersion = 2;
        public const int MaxMajorVersion = 3;
        public const int MaxMinorVersion = 2;

        public static readonly Version MinVersion = new Version(MinMajorVersion, MinMinorVersion);
        public static readonly Version MaxVersion = new Version(MaxMajorVersion, MaxMinorVersion);

        public static bool IsCompatibleVersion(Version v) {
            var verMajorMinor = new Version(v.Major, v.Minor);
            return verMajorMinor >= MinVersion && verMajorMinor <= MaxVersion;
        }
    }
}
