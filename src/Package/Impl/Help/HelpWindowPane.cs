// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.R.Components.Help;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    [Guid(WindowGuidString)]
    internal class HelpWindowPane : VisualComponentToolWindow<IHelpVisualComponent> {
        public const string WindowGuidString = "9E909526-A616-43B2-A82B-FD639DCD40CB";
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        public HelpWindowPane() {
            Caption = Resources.HelpWindowCaption;
            BitmapImageMoniker = KnownMonikers.StatusHelp;
            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.helpWindowToolBarId);
        }

        protected override void OnCreate() {
            Component = new HelpVisualComponent { Container = this };
            ToolBarCommandTarget = new CommandTargetToOleShim(null, Component.Controller);
            base.OnCreate();
        }
    }
}
