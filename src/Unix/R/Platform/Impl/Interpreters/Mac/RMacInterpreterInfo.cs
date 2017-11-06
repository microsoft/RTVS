// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core.IO;

namespace Microsoft.R.Platform.Interpreters.Mac {
    internal sealed class RMacInterpreterInfo : UnixInterpreterInfo {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the R interpreter</param>
        /// <param name="fileSystem"></param>
        public RMacInterpreterInfo(string name, string version, Version parsedVersion, IFileSystem fileSystem) :
            base(name, parsedVersion, fileSystem) {
            InstallPath = $"/Library/Frameworks/R.framework/Versions/{version}/Resources";
            BinPath = Path.Combine(InstallPath, "bin");
            DocPath = Path.Combine(InstallPath, "doc");
            LibPath = Path.Combine(InstallPath, "lib");
            IncludePath = Path.Combine(InstallPath, "include");
            RShareDir = Path.Combine(InstallPath, "share");
        }

        public override string LibName => "libR.dylib";
    }
}