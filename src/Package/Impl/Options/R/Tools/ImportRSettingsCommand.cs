// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        private const string _settingsFileName = "R.vssettings";
        private const string _profilesFolder = @"Profiles\" + Toolset.Version;

        public ImportRSettingsCommand() :
            base(OnCommand, new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportRSettings)) { }

        public static void OnCommand(object sender, EventArgs args) {
            if (MessageButtons.Yes == VsAppShell.Current.ShowMessage(Resources.Warning_SettingsReset, MessageButtons.YesNo)) {
                IVsUIShell shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
                Guid group = VSConstants.CMDSETID.StandardCommandSet2K_guid;

                string asmDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
                string settingsFilePath = Path.Combine(asmDirectory, _profilesFolder, _settingsFileName);
                if (!File.Exists(settingsFilePath)) {
                    string ideFolder = asmDirectory.Substring(0, asmDirectory.IndexOf(@"\Extensions", StringComparison.OrdinalIgnoreCase));
                    settingsFilePath = Path.Combine(ideFolder, _profilesFolder, _settingsFileName);
                }

                object arguments = string.Format(CultureInfo.InvariantCulture, "-import:\"{0}\"", settingsFilePath);
                shell.PostExecCommand(ref group, (uint)VSConstants.VSStd2KCmdID.ManageUserSettings, 0, ref arguments);
            }
        }
    }
}
