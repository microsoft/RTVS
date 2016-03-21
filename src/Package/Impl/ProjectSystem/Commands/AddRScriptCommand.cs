// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo("RTools")]
    [OrderPrecedence(200)]
    internal sealed class AddRScriptCommand : AddItemCommand {

        [ImportingConstructor]
        public AddRScriptCommand(UnconfiguredProject unconfiguredProject):
            base(unconfiguredProject, RPackageCommandId.icmdAddRScript, "rscript", "Script", "r") {
        }
    }
}
