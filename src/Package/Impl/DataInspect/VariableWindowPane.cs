// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.DataInspect.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Guid("99d2ea62-72f2-33be-afc8-b8ce6e43b5d0")]
    internal sealed class VariableWindowPane : RToolWindowPane, IOleCommandTarget {
        private CommandTargetToOleShim _commandTarget;

        public VariableWindowPane() {
            Caption = Resources.VariableWindowCaption;
            Content = new VariableView(VsAppShell.Current.Services);

            // this value matches with icmdShowVariableExplorerWindow's Icon in VSCT file
            BitmapImageMoniker = KnownMonikers.VariableProperty;

            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.variableWindowToolBarId);
        }

        protected override void OnCreate() {
            var variableView = (VariableView) Content;
            var controller = new AsyncCommandController()
                .AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Copy, new CopyVariableCommand(variableView))
                .AddCommand(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.Delete, new DeleteVariableCommand(variableView))
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdCopyValue, new CopyVariableValueCommand(variableView))
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdShowDetails, new ShowVariableDetailsCommand(variableView))
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdOpenInCsvApp, new OpenVariableInCsvCommand(variableView));
              
            _commandTarget = new CommandTargetToOleShim(null, controller);
            base.OnCreate();
        }
        
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) 
            => _commandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) 
            => _commandTarget.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
        
        protected override void Dispose(bool disposing) {
            if (disposing) {
                _commandTarget = null;
            }
            base.Dispose(disposing);
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
