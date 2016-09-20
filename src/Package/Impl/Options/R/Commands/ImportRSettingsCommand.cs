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

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    public sealed class ImportRSettingsCommand : MenuCommand {
        public ImportRSettingsCommand() :
            base(OnCommand, new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.icmdImportRSettings)) { }

        public static void OnCommand(object sender, EventArgs args) {
            if (MessageButtons.Yes == VsAppShell.Current.ShowMessage(Resources.Warning_SettingsReset, MessageButtons.YesNo)) {
                Guid group = VSConstants.CMDSETID.StandardCommandSet2K_guid;

                string asmDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
                string settingsFilePath1 = Path.Combine(asmDirectory, @"Profiles\", "R.vssettings");
                //string settingsFilePath2 = Path.Combine(asmDirectory, @"Profiles\", "RStudioKeyboard.vssettings");
                if (!File.Exists(settingsFilePath1)) {
                    string ideFolder = asmDirectory.Substring(0, asmDirectory.IndexOf(@"\Extensions", StringComparison.OrdinalIgnoreCase));
                    settingsFilePath1 = Path.Combine(ideFolder, @"Profiles\", "R.vssettings");
                }

                object arguments = string.Format(CultureInfo.InvariantCulture, "-import:\"{0}\"", settingsFilePath1);
                VsAppShell.Current.PostCommand(group, (int)VSConstants.VSStd2KCmdID.ManageUserSettings);
            }
        }
    }
}
