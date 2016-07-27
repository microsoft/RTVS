// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Runtime.InteropServices;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Guid(RGuidList.ProjectFileGeneratorGuidString)]
    internal sealed class RProjectFileGenerator : FileSystemMirroringProjectFileGenerator {
        private static readonly string[] _imports = {
             ProjectConstants.RtvsRulesPropsRelativePath,
             ProjectConstants.RtvsTargetsRelativePath,
        };

        public RProjectFileGenerator()
            : base(RGuidList.CpsProjectFactoryGuid, null, RContentTypeDefinition.VsRProjectExtension, _imports) {
        }

        protected override XPropertyGroup CreateProjectSpecificPropertyGroup(string cpsProjFileName) {
            var scripts = Directory.GetFiles(Path.GetDirectoryName(cpsProjFileName), "*.R");
            if (scripts.Length > 0) {
                var startupFile = Path.GetFileName(scripts[0]);
                return new XPropertyGroup(new XProperty("StartupFile", startupFile));
            }
            return null;
        }
    }
}
