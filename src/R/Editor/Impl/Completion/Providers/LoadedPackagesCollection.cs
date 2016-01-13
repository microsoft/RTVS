using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Completion.Definitions;

namespace Microsoft.R.Editor.Completion.Providers {
    internal sealed class LoadedPackagesCollection {
        IEnumerable<string> _loadedPackages = Enumerable.Empty<string>();

        public LoadedPackagesCollection() {
            IREditorWorkspaceProvider provider = EditorShell.Current.ExportProvider.GetExportedValue<IREditorWorkspaceProvider>();
            IREditorWorkspace workspace = provider.GetWorkspace();
            workspace.Changed += OnWorkspaceChanged;
        }

        private void OnWorkspaceChanged(object sender, EventArgs e) {
            var workspace = sender as IREditorWorkspace;
            workspace.EvaluateExpression("paste0(.packages(), collapse = ' ')", ParseSearchResponse, null);
        }

        private void ParseSearchResponse(string response, object parameter) {
            _loadedPackages = response.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> LoadedPackages => _loadedPackages;
    }
}
