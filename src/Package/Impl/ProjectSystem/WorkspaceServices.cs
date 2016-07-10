// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IWorkspaceServices))]
    internal sealed class WorkspaceServices : IWorkspaceServices {
        [Import]
        private IProjectSystemServices ProjectSystemServices { get; set; }

        public string ActiveProjectPath {
            get {
                var projectFile = ProjectSystemServices.GetActiveProject()?.FullName;
                return !string.IsNullOrEmpty(projectFile) ? Path.GetDirectoryName(projectFile) : string.Empty;
            }
        }
    }
}
