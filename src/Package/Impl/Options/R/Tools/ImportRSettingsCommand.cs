using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    public sealed class ImportRSettingsCommand : MenuCommand {
        public ImportRSettingsCommand() :
            base(OnCommand, new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportRSettings)) { }

        public static void OnCommand(object sender, EventArgs args) {
            if (MessageButtons.Yes == VsAppShell.Current.ShowMessage(Resources.Warning_SettingsReset, MessageButtons.YesNo)) {
                IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                Guid group = VSConstants.CMDSETID.StandardCommandSet2K_guid;

                string asmDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
                string settingsFilePath = Path.Combine(asmDirectory, "R.vssettings");
                object arguments = string.Format(CultureInfo.InvariantCulture, "-import:\"{0}\"", settingsFilePath);
                shell.PostExecCommand(ref group, (uint)VSConstants.VSStd2KCmdID.ManageUserSettings, 0, ref arguments);

                if (MessageButtons.Yes == VsAppShell.Current.ShowMessage(Resources.Warning_RStudioKeyboardShortcuts, MessageButtons.YesNo)) {
                    settingsFilePath = Path.Combine(asmDirectory, "RStudioKeyboard.vssettings");
                    arguments = string.Format(CultureInfo.InvariantCulture, "-import:\"{0}\"", settingsFilePath);
                    shell.PostExecCommand(ref group, (uint)VSConstants.VSStd2KCmdID.ManageUserSettings, 0, ref arguments);
                }
            }
        }
    }
}
