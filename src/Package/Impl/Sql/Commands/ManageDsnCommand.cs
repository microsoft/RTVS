// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
using Constants = Microsoft.VisualStudio.R.Package.ProjectSystem.Constants;

namespace Microsoft.VisualStudio.R.Package.Sql {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(Constants.RtvsProjectCapability)]
    internal sealed class ManageDsnCommand : ConfigurationSettingCommand {
        public ManageDsnCommand() : base(RPackageCommandId.icmdManageDsn) { }

        public override Task<bool> TryHandleCommandAsync() {
            IntPtr vsWindow;
            var shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            shell.GetDialogOwnerHwnd(out vsWindow);
            NativeMethods.SQLManageDataSources(vsWindow);
            return Task.FromResult(true);
        }
    }
}
