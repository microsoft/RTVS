// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using static System.FormattableString;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Services;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class OpenAdminCommandPromptCommand : CommandPromptCommand {
        [ImportingConstructor]
        public OpenAdminCommandPromptCommand(ICoreServices services) :
            base(RPackageCommandId.icmdOpenAdminCmdPromptHere, services.ProcessServices) { }

        protected override void SetFlags(ProcessStartInfo psi, string path) {
            psi.Verb = "runas";
            psi.UseShellExecute = true;
            var root = path.Substring(0, 2);
            var sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var cmd = Path.Combine(sys32, "cmd.exe");
            psi.FileName = Invariant($"{cmd} /k \"{root} & cd {path}\"");
        }
    }
}
