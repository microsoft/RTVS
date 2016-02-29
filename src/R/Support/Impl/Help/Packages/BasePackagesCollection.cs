// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help.Packages {
    public sealed class BasePackagesCollection : PackageCollection {
        public BasePackagesCollection() :
            base(GetInstallPath()) {
        }

        private static string GetInstallPath() {
            string rInstallPath = RInstallation.GetRInstallPath(RToolsSettings.Current != null ? RToolsSettings.Current.RBasePath : null);
            return rInstallPath != null ? Path.Combine(rInstallPath, "library") : null;
        }
    }
}
