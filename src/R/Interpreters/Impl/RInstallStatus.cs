// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Interpreters {
    public enum RInstallStatus {
        /// <summary>
        /// R is installed and compatible
        /// </summary>
        OK,

        /// <summary>
        /// R is installed but version does not match
        /// </summary>
        UnsupportedVersion,

        /// <summary>
        /// Path appears to exist but no R.dll can be found
        /// </summary>
        NoRBinaries,
    }
}
