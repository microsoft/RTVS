// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.R.Package.Sql;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IProjectItemDependencyProvider))]
    internal sealed class ProjectItemDependencyProvider : IProjectItemDependencyProvider {
        public string GetMasterFile(string childFilePath) {
            if(childFilePath.EndsWithIgnoreCase(SProcFileExtensions.QueryFileExtension)) {
                return childFilePath.Substring(0, childFilePath.Length - SProcFileExtensions.QueryFileExtension.Length) + ".R";
            }
            else if (childFilePath.EndsWithIgnoreCase(SProcFileExtensions.SProcFileExtension)) {
                return childFilePath.Substring(0, childFilePath.Length - SProcFileExtensions.SProcFileExtension.Length) + ".R";
            }
            return string.Empty;
        }
    }
}
