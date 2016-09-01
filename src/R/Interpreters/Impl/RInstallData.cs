// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Interpreters {
    public class RInstallData {
        /// <summary>
        /// General status of the installation
        /// </summary>
        public RInstallStatus Status { get; set; }

        /// <summary>
        /// Exception encountered when looking for R, if any.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
