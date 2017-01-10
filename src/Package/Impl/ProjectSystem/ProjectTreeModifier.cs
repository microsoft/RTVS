// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
#if VS14
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.Designers;
using Microsoft.VisualStudio.R.Package.Sql;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IProjectTreeModifier))]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class ProjectTreeModifier : IProjectTreeModifier {
        public IProjectTree ApplyModifications(IProjectTree tree, IProjectTreeProvider projectTreeProvider) {
            if (tree != null) {
                if (tree.Capabilities.Contains(ProjectTreeCapabilities.ProjectRoot)) {
                    tree = tree.SetIcon(ProjectIconProvider.ProjectNodeImage.ToProjectSystemType());
                } else if (tree.Capabilities.Contains(ProjectTreeCapabilities.FileOnDisk)) {
                    string ext = Path.GetExtension(tree.FilePath).ToLowerInvariant();
                    if (ext == ".r") {
                        tree = tree.SetIcon(ProjectIconProvider.RFileNodeImage.ToProjectSystemType());
                    } else if (ext == ".rdata" || ext == ".rhistory") {
                        tree = tree.SetIcon(ProjectIconProvider.RDataFileNodeImage.ToProjectSystemType());
                    } else if (ext == ".md" || ext == ".rmd") {
                        tree = tree.SetIcon(ProjectIconProvider.RMarkdownFileNodeImage.ToProjectSystemType());
                    } else if (ext == ".rd") {
                        tree = tree.SetIcon(ProjectIconProvider.RdFileNodeImage.ToProjectSystemType());
                    } else if (tree.FilePath.EndsWithIgnoreCase(SProcFileExtensions.QueryFileExtension)) {
                        tree = tree.SetIcon(ProjectIconProvider.SqlFileNodeImage.ToProjectSystemType());
                    } else if (tree.FilePath.EndsWithIgnoreCase(SProcFileExtensions.SProcFileExtension)) {
                        tree = tree.SetIcon(ProjectIconProvider.SqlProcFileNodeImage.ToProjectSystemType());
                    }
                }
            }
            return tree;
        }
    }
}
#endif
