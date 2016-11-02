// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using EnvDTE;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Debugger;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Debugger.Commands {
    internal class ShowDotPrefixedVariablesCommand : PackageCommand {
        private readonly IRToolsSettings _settings;

        public ShowDotPrefixedVariablesCommand(IRToolsSettings settings)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowDotPrefixedVariables) {
            _settings = settings;
        }


        protected override void SetStatus() {
            Checked = _settings.ShowDotPrefixedVariables;

            // Only show it in the debugger context menu when debugging R code to avoid clutter.
            var debugger = VsAppShell.Current.GetGlobalService<DTE>().Debugger;
            Enabled = Visible = debugger.CurrentStackFrame?.Language == RContentTypeDefinition.LanguageName;
        }

        protected override void Handle() {
           _settings.ShowDotPrefixedVariables = !_settings.ShowDotPrefixedVariables;
            VsAppShell.Current.GetGlobalService<DTE>().Debugger.RefreshVariableViews();
        }
    }
}