using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.R.Editor.Completion.Definitions;

namespace Microsoft.VisualStudio.R.Package.Editors {
    [Export(typeof(IREditorWorkspaceProvider))]
    internal sealed class REditorWorkspaceProvider : IREditorWorkspaceProvider {
        private Lazy<REditorWorkspace> _instance = Lazy.Create(() => new REditorWorkspace());
        public IREditorWorkspace GetWorkspace() {
            return _instance.Value;
        }
    }
}
