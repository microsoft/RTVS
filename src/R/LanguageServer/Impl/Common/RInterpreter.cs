// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.R.Interpreters;

namespace Microsoft.R.LanguageServer.Common {
    internal sealed class RInterpreterInfo: IRInterpreterInfo {
        public string Name { get; }
        public Version Version { get; }
        public string InstallPath { get; }
        public string BinPath { get; }
        public bool VerifyInstallation(ISupportedRVersionRange svr = null, IServiceContainer services = null) {
            throw new NotImplementedException();
        }

        public string DocPath { get; }
        public string IncludePath { get; }
        public string RShareDir { get; }
        public string[] SiteLibraryDirs { get; }
    }
}
