// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(ExportContractNames.Scopes.UnconfiguredProject, typeof(IProjectCapabilitiesProvider))]
    [SupportsFileExtension(RContentTypeDefinition.VsRProjectExtension)]
    [AppliesTo(ProjectCapabilities.AlwaysApplicable)]
    internal sealed class RProjectCapabilityProvider : ProjectCapabilitiesFromImportXmlProvider {
        [ImportingConstructor]
        public RProjectCapabilityProvider(UnconfiguredProject unconfiguredProject)
            : base(ProjectConstants.RtvsRulesPropsRelativePath, unconfiguredProject, 
                   ProjectConstants.RtvsRulesPropsRelativePath, ProjectConstants.RtvsProjectCapability) { }
    }
}