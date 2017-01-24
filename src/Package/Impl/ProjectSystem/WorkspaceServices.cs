// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Workspace;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IWorkspaceServices))]
    internal sealed class WorkspaceServices : IWorkspaceServices {
        private readonly IProjectSystemServices _pss;

        [ImportingConstructor]
        public WorkspaceServices(IProjectSystemServices pss) {
            _pss = pss;
        }

        public string ActiveProjectPath {
            get {
                var projectFile = _pss.GetActiveProject()?.FullName;
                return !string.IsNullOrEmpty(projectFile) ? Path.GetDirectoryName(projectFile) : string.Empty;
            }
        }

        public bool IsRProjectActive {
            get {
                var projectFile = _pss.GetActiveProject()?.FullName;
                // Either there is no project or it is R project
                return string.IsNullOrEmpty(projectFile) || Path.GetExtension(projectFile).EqualsIgnoreCase(RContentTypeDefinition.VsRProjectExtension);
            }
        }
    }
}
