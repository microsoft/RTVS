// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IProjectTreePropertiesProvider))]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class ProjectTreePropertiesProvider : IProjectTreePropertiesProvider {
        public void CalculatePropertyValues(IProjectTreeCustomizablePropertyContext propertyContext,
                                            IProjectTreeCustomizablePropertyValues propertyValues) {
            if (propertyValues.Flags.Contains(ProjectTreeFlags.Common.ProjectRoot)) {
                propertyValues.Icon = ProjectIconProvider.ProjectNodeImage.ToProjectSystemType();
            } else if (propertyValues.Flags.Contains(ProjectTreeFlags.Common.FileOnDisk)) {
                string ext = Path.GetExtension(propertyContext.ItemName).ToLowerInvariant();
                if (ext == ".r") {
                    propertyValues.Icon = ProjectIconProvider.RFileNodeImage.ToProjectSystemType();
                } else if (ext == ".rdata" || ext == ".rhistory") {
                    propertyValues.Icon = ProjectIconProvider.RDataFileNodeImage.ToProjectSystemType();
                } else if (ext == ".md" || ext == ".rmd") {
                    propertyValues.Icon = KnownMonikers.MarkdownFile.ToProjectSystemType();
                } else if (propertyContext.ItemName.EndsWithIgnoreCase(SProcFileExtensions.QueryFileExtension)) {
                    propertyValues.Icon = KnownMonikers.DatabaseColumn.ToProjectSystemType();
                } else if (propertyContext.ItemName.EndsWithIgnoreCase(SProcFileExtensions.SProcFileExtension)) {
                    propertyValues.Icon = KnownMonikers.DatabaseStoredProcedures.ToProjectSystemType();
                }
            }
        }
    }
}
