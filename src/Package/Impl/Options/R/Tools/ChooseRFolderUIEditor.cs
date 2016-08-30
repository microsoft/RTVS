// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudioTools;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class ChooseRFolderUIEditor : UITypeEditor {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            string latestRPath = Environment.SystemDirectory;
            try {
                latestRPath = new RInstallation().GetCompatibleEnginePathFromRegistry();
                if (string.IsNullOrEmpty(latestRPath) || !Directory.Exists(latestRPath)) {
                    // Force 64-bit PF
                    latestRPath = Environment.GetEnvironmentVariable("ProgramFiles");
                }
            } catch (ArgumentException) { } catch (IOException) { }

            return Dialogs.BrowseForDirectory(VsAppShell.Current.GetDialogOwnerWindow(), latestRPath, Resources.ChooseRInstallFolder);
        }
    }
}
