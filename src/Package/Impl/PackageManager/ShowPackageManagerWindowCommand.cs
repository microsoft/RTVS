// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.PackageManager {
    internal sealed class ShowPackageManagerWindowCommand : ShowToolWindowCommand<PackageManagerWindowPane> {
        public ShowPackageManagerWindowCommand() 
            : base(RPackageCommandId.icmdShowPackagesWindow) {}
    }
}