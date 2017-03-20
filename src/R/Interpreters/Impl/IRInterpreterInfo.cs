// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.UI;

namespace Microsoft.R.Interpreters {
    public interface IRInterpreterInfo {
        /// <summary>
        /// User-friendly name of the interpreter. Determined from the registry
        /// key for CRAN interpreters or via other means for MRO/MRC.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Found R version
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Path to the R installation folder (without bin\x64)
        /// </summary>
        string InstallPath { get; }

        /// <summary>
        /// Path to the R binaries (with bin\x64)
        /// </summary>
        string BinPath { get; }

        /// <summary>
        /// Verifies actual installation on disk
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        bool VerifyInstallation(ISupportedRVersionRange svl = null, IFileSystem fs = null, IUIService ui = null);
    }
}
