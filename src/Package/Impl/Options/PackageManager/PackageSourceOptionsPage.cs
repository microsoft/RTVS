using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.R.Components.PackageManager.Implementation.View;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.PackageManager {
    public class PackageSourceOptionsPage : DialogPage {
        public PackageSourceOptionsPage() {
            SettingsRegistryPath = @"UserSettings\R_Tools";
        }

        private PackageSourcesOptionsControl _optionsWindow;

        //protected override void OnActivate(CancelEventArgs e) {
        //    base.OnActivate(e);
        //    PackageSourcesControl.Font = VsShellUtilities.GetEnvironmentFont(this);
        //    PackageSourcesControl.InitializeOnActivated();
        //}

        //protected override void OnApply(PageApplyEventArgs e) {
        //    // Do not need to call base.OnApply() here.
        //    bool wasApplied = PackageSourcesControl.ApplyChangedSettings();
        //    if (!wasApplied) {
        //        e.ApplyBehavior = ApplyKind.CancelNoNavigate;
        //    }
        //}

        //protected override void OnClosed(EventArgs e) {
        //    PackageSourcesControl.ClearSettings();
        //    base.OnClosed(e);
        //}

        protected override IWin32Window Window => PackageSourcesControl;

        private PackageSourcesOptionsControl PackageSourcesControl => _optionsWindow ?? (_optionsWindow = new PackageSourcesOptionsControl {Location = new Point(0, 0)});
    }
}
