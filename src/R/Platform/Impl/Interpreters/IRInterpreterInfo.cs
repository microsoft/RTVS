// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.Platform.Interpreters {
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
        /// Path to R.dll (Windows) or libR.dylib (MacOS)
        /// </summary>
        string LibPath { get; }

        /// <summary>
        /// Name of the R dynamic library such as  R.dll (Windows) or libR.dylib (MacOS)
        /// </summary>
        string LibName { get; }

        /// <summary>
        /// Verifies actual installation on disk
        /// </summary>
        bool VerifyInstallation(ISupportedRVersionRange svr = null, IServiceContainer services = null);

        string DocPath { get; }

        string IncludePath { get; }

        string RShareDir { get; }

        string[] SiteLibraryDirs { get; }
    }
}
