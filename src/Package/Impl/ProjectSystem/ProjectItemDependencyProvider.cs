// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IProjectItemDependencyProvider))]
    internal sealed class ProjectItemDependencyProvider : IProjectItemDependencyProvider {
        public string GetMasterFile(string childFilePath) {
            if(childFilePath.EndsWithIgnoreCase(".R.sql")) {
                return childFilePath.Substring(0, childFilePath.Length - 4);
            }
            else if (childFilePath.EndsWithIgnoreCase(".R.SProc.sql")) {
                return childFilePath.Substring(0, childFilePath.Length - 10);
            }
            return string.Empty;
        }
    }
}
