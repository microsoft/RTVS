// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Guid(RGuidList.ProjectFileGeneratorGuidString)]
    internal sealed class RProjectFileGenerator : FileSystemMirroringProjectFileGenerator {
        public RProjectFileGenerator()
            : base(RGuidList.CpsProjectFactoryGuid, null, RContentTypeDefinition.VsRProjectExtension, new[] { Constants.RtvsRulesPropsRelativePath }) {
        }
    }
}
