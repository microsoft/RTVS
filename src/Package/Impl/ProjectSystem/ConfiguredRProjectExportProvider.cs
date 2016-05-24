// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IConfiguredRProjectExportProvider))]
    [AppliesTo("RTools")]
    internal class ConfiguredRProjectExportProvider : IConfiguredRProjectExportProvider {
#if VS14
        public Task<T> GetExportAsync<T>(UnconfiguredProject unconfigProject, string configurationName)
        {
            configurationName = configurationName.Replace("Any CPU", "AnyCPU");
            var configProject = unconfigProject.LoadedConfiguredProjects.SingleOrDefault(cp => cp.ProjectConfiguration.Name.Equals(configurationName, StringComparison.OrdinalIgnoreCase));
            if (configProject == null) {
                throw new ArgumentException();
            }
            var exportedValue = configProject.Services.ExportProvider.GetExportedValue<T>();
            Task.FromResult(exportedValue);
        }
#endif

#if VS15
        public async Task<T> GetExportAsync<T>(UnconfiguredProject unconfigProject, string configurationName) {
            configurationName = configurationName.Replace("Any CPU", "AnyCPU");
            var configurationProps = ConfigurationPropertiesFromConfigurationName(configurationName);
            var configProject = await unconfigProject.LoadConfiguredProjectAsync(configurationName, configurationProps);
            if (configProject == null) {
                throw new ArgumentException();
            }
            return configProject.Services.ExportProvider.GetExportedValue<T>();
        }
#endif

        public async Task<T> GetExportAsync<T>(IVsHierarchy projectHierarchy, string configurationName) {
            var unconfigProject = projectHierarchy.GetUnconfiguredProject();
            return await GetExportAsync<T>(unconfigProject, configurationName);
        }

        private IImmutableDictionary<string, string> ConfigurationPropertiesFromConfigurationName(string configurationName) {
            var names = configurationName.Split('|');
            if (names.Length != 2) {
                throw new ArgumentOutOfRangeException(nameof(configurationName));
            }

            var dims = new Dictionary<string, string>();
            dims.Add("Configuration", names[0]);
            dims.Add("Platform", names[1]);
            return ImmutableDictionary.ToImmutableDictionary(dims);
        }
    }
}
