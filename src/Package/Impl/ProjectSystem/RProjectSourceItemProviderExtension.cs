// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IProjectSourceItemProviderExtension))]
    [Export(typeof(IProjectFolderItemProviderExtension))]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class RProjectSourceItemProviderExtension : FileSystemMirroringProjectSourceItemProviderExtensionBase {
        [ImportingConstructor]
        public RProjectSourceItemProviderExtension(UnconfiguredProject unconfiguredProject, ConfiguredProject configuredProject, IProjectLockService projectLockService, IFileSystemMirroringProjectTemporaryItems temporaryItems)
            : base(unconfiguredProject, configuredProject, projectLockService, temporaryItems) {
        }
    }
}