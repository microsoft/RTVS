// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Guid("99d2ea62-72f2-33be-afc8-b8ce6e43b5d0")]
    internal sealed class VariableWindowPane : RToolWindowPane {
        public VariableWindowPane() {
            Caption = Resources.VariableWindowCaption;
            Content = new VariableView(VsAppShell.Current.Services);

            // this value matches with icmdShowVariableExplorerWindow's Icon in VSCT file
            BitmapImageMoniker = KnownMonikers.VariableProperty;

            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.variableWindowToolBarId);
        }

        public bool IsGlobalREnvironment() {
            var varView = Content as VariableView;
            return varView.IsGlobalREnvironment();
        }

        public override bool SearchEnabled {
            get {
                var grid = Content as VariableView;
                return grid?.RootTreeGrid != null;
            }
        }

        public override void ProvideSearchSettings(IVsUIDataSource pSearchSettings) {
            dynamic settings = pSearchSettings;
            settings.SearchStartType = VSSEARCHSTARTTYPE.SST_INSTANT;
            base.ProvideSearchSettings(pSearchSettings);
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback) {
            if (SearchEnabled) {
                var grid = Content as VariableView;
                return new VariableSearchTask(grid.RootTreeGrid, dwCookie, pSearchQuery, pSearchCallback);
            }
            return null;
        }
    }
}
