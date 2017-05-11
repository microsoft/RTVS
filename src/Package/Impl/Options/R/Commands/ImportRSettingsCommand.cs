// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.R.Commands {
    public sealed class ImportRSettingsCommand : System.ComponentModel.Design.MenuCommand {
        private const string _settingsFileName = "R.vssettings";
        private const string _profilesFolder = @"Profiles\";
        private static IServiceContainer _services;

        public ImportRSettingsCommand(IServiceContainer services) :
            base(OnCommand, new System.ComponentModel.Design.CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportRSettings)) {
            _services = _services ?? services;
        }

        public static void OnCommand(object sender, EventArgs args) {
            if (MessageButtons.Yes == _services.UI().ShowMessage(Resources.Warning_SettingsReset, MessageButtons.YesNo)) {
                var shell = _services.GetService<IVsUIShell>(typeof(SVsUIShell));
                var group = VSConstants.CMDSETID.StandardCommandSet2K_guid;

                var asmDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
                // Non-versioned setup
                var settingsFilePath = Path.Combine(asmDirectory, _profilesFolder, _settingsFileName);
                if (!File.Exists(settingsFilePath)) {
                    // Typically debug setup launched via F5 with profiles under 14.0/15.0 folder
                    settingsFilePath = Path.Combine(asmDirectory, _profilesFolder, Toolset.Version, _settingsFileName);
                    if (!File.Exists(settingsFilePath)) {
                        // Release setup with settings in the IDE profiles folder
                        string ideFolder = asmDirectory.Substring(0, asmDirectory.IndexOf(@"\Extensions", StringComparison.OrdinalIgnoreCase));
                        settingsFilePath = Path.Combine(ideFolder, _profilesFolder, _settingsFileName);
                    }
                }

                object arguments = string.Format(CultureInfo.InvariantCulture, "-import:\"{0}\"", settingsFilePath);
                shell.PostExecCommand(ref group, (uint)VSConstants.VSStd2KCmdID.ManageUserSettings, 0, ref arguments);
            }
        }
    }
}
