// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.R.Host.Client.Install;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help.Packages {
    public sealed class BasePackagesCollection : PackageCollection {
        public BasePackagesCollection(IFunctionIndex functionIndex) :
            base(functionIndex, GetInstallPath()) {
        }

        private static string GetInstallPath() {
            var basePath = RToolsSettings.Current != null ? RToolsSettings.Current.RBasePath : null;
            string rInstallPath = RInstallation.GetRInstallPath(basePath, new SupportedRVersionRange());
            return rInstallPath != null ? Path.Combine(rInstallPath, "library") : null;
        }
    }
}
