// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Actions.Logging;

namespace Microsoft.VisualStudio.R.Package.Logging {
    public sealed class PackagesInstallLog : LinesLog {
        private static readonly Guid WindowPaneGuid = new Guid("8DF051DA-60DE-4E2B-AB90-E440EE089A7F");
        private static readonly Lazy<PackagesInstallLog> Instance = new Lazy<PackagesInstallLog>(() => new PackagesInstallLog());

        public static IActionLog Current => Instance.Value;

        private PackagesInstallLog() :
            base(new OutputWindowLogWriter(WindowPaneGuid, Resources.OutputWindowName_InstallPackages)) {
        }
    }
}
