// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IConfiguredRProjectExportProvider))]
    [AppliesTo("RTools")]
    internal class ConfiguredRProjectExportProvider : IConfiguredRProjectExportProvider {
        public T GetExport<T>(UnconfiguredProject unconfigProject, string configurationName) {
            configurationName = configurationName.Replace("Any CPU", "AnyCPU");
            var configProject = unconfigProject.LoadedConfiguredProjects.SingleOrDefault(cp => cp.ProjectConfiguration.Name.Equals(configurationName, StringComparison.OrdinalIgnoreCase));
            if (configProject == null) {
                throw new ArgumentException();
            }
            return configProject.Services.ExportProvider.GetExportedValue<T>();
        }

        public T GetExport<T>(IVsHierarchy projectHierarchy, string configurationName) {
            var unconfigProject = projectHierarchy.GetUnconfiguredProject();
            return GetExport<T>(unconfigProject, configurationName);
        }
    }
}
