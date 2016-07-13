// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Workspace;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IWorkspaceServices))]
    internal sealed class WorkspaceServices : IWorkspaceServices {
        private readonly IRInteractiveWorkflowProvider _provider;
        private readonly IProjectSystemServices _pss;

        [ImportingConstructor]
        public WorkspaceServices(IProjectSystemServices pss, IRInteractiveWorkflowProvider provider) {
            _pss = pss;
            _provider = provider;
        }

        public string ActiveProjectPath {
            get {
                var projectFile = _pss.GetActiveProject()?.FullName;
                return !string.IsNullOrEmpty(projectFile) ? Path.GetDirectoryName(projectFile) : string.Empty;
            }
        }

        public Task<string> GetRUserFolder() {
            var session = _provider.GetOrCreate()?.RSession;
            return session?.GetRUserDirectoryAsync();
        }
    }
}
