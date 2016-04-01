// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.R.Package.Windows;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal class ShowToolWindowCommand<T> : PackageCommand 
        where T : RToolWindowPane {

        public ShowToolWindowCommand(int id)
            : base(RGuidList.RCmdSetGuid, id) {}


        protected override void SetStatus() {
            Visible = true;
            Enabled = true;
        }

        protected override void Handle() {
            ToolWindowUtilities.ShowWindowPane<T>(0, true);
        }
    }
}