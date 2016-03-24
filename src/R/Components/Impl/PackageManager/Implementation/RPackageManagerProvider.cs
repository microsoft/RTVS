// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.PackageManager.Implementation {
    [Export(typeof(IRPackageManagerProvider))]
    internal class RPackageManagerProvider : IRPackageManagerProvider {

        public IRPackageManager CreateRPackageManager(IRInteractiveWorkflow interactiveWorkflow) {
            var pm = new RPackageManager(interactiveWorkflow, () => {});
            return pm;
        }
    }
}