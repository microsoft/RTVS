// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    [Guid(WindowGuid)]
    internal class HelpWindowPane : VisualComponentToolWindow<IHelpWindowVisualComponent> {
        internal const string WindowGuid = "9E909526-A616-43B2-A82B-FD639DCD40CB";

        public HelpWindowPane() {

            Caption = Resources.HelpWindowCaption;
            BitmapImageMoniker = KnownMonikers.StatusHelp;

            Component = new HelpWindowVisualComponent {
                Container = this
            };

            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.helpWindowToolBarId);
            ToolBarCommandTarget = new CommandTargetToOleShim(null, Component.Controller);
        }

        protected override void Dispose(bool disposing) {
            if (disposing && Component != null) {
                Component.Dispose();
                Component = null;
            }
            base.Dispose(disposing);
        }
    }
}
