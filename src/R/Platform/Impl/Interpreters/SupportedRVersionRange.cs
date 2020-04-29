// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Platform.Interpreters {
    public sealed class SupportedRVersionRange : ISupportedRVersionRange {
        // TODO: this probably needs configuration file
        // or another dynamic source of supported versions.
        public int MinMajorVersion { get; }
        public int MinMinorVersion { get; }
        public int MaxMajorVersion { get; }
        public int MaxMinorVersion { get; }

        public SupportedRVersionRange() : this(3, 2, 4, 9) { }

        public SupportedRVersionRange(int minVersionMajorPart, int minVersionMinorPart, int maxVersionMajorPart, int maxVersionMinorPart) {
            MinMajorVersion = minVersionMajorPart;
            MinMinorVersion = minVersionMinorPart;
            MaxMajorVersion = maxVersionMajorPart;
            MaxMinorVersion = maxVersionMinorPart;
        }
    }
}
