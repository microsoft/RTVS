// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client.Install {
    public class RInstallData {
        /// <summary>
        /// General status of the installation
        /// </summary>
        public RInstallStatus Status { get; set; }

        /// <summary>
        /// Path to the R installation folder (withoutt bin\x64)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Path to the R binaries (with bin\x64)
        /// </summary>
        public string BinPath { get; set; }

        /// <summary>
        /// Found R version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Exception encountered when looking for R, if any.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
