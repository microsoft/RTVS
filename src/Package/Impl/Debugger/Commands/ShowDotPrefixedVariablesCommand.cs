// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using EnvDTE;
using Microsoft.R.Debugger.Engine;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Debugger.Commands {
    internal class ShowDotPrefixedVariablesCommand : PackageCommand {
        private readonly IRToolsSettings _settings;

        public ShowDotPrefixedVariablesCommand(IRToolsSettings settings)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowDotPrefixedVariables) {
            _settings = settings;
        }


        internal override void SetStatus() {
            Checked = _settings.ShowDotPrefixedVariables;

            // Only show it in the debugger context menu when debugging R code to avoid clutter.
            var debugger = VsAppShell.Current.GetGlobalService<DTE>().Debugger;
            Enabled = Visible = debugger.CurrentStackFrame?.Language == RContentTypeDefinition.LanguageName;
        }

        internal override void Handle() {
           _settings.ShowDotPrefixedVariables = !_settings.ShowDotPrefixedVariables;
            VsAppShell.Current.GetGlobalService<DTE>().Debugger.RefreshVariableViews();
        }
    }
}