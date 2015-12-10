using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using Microsoft.R.Actions.Utility;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.VisualStudio.R.Package.Options.R.Tools {
    internal sealed class ChooseRFolderUIEditor : UITypeEditor {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            IVsUIShell uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IntPtr dialogOwner;
            uiShell.GetDialogOwnerHwnd(out dialogOwner);

            string latestRPath = Environment.SystemDirectory;
            try {
                latestRPath = RInstallation.GetLatestEnginePathFromRegistry();
                if (string.IsNullOrEmpty(latestRPath) || !Directory.Exists(latestRPath)) {
                    // Force 64-bit PF
                    latestRPath = Environment.GetEnvironmentVariable("ProgramFiles");
                }
            } catch (ArgumentException) { } catch (IOException) { }

            return Dialogs.BrowseForDirectory(dialogOwner, latestRPath, Resources.ChooseRInstallFolder);
        }
    }
}
