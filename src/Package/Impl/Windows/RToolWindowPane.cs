// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Windows {
    internal abstract class RToolWindowPane : ToolWindowPane {
        public override void OnToolWindowCreated() {
            // Binds all tool windows to the same set of keyboard bindings
            ((IVsWindowFrame)Frame).SetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, RGuidList.REditorFactoryGuid);
            base.OnToolWindowCreated();
        }
    }
}
