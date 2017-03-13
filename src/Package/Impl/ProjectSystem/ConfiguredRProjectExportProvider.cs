// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Export(typeof(IConfiguredRProjectExportProvider))]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal class ConfiguredRProjectExportProvider : IConfiguredRProjectExportProvider {
        public async Task<T> GetExportAsync<T>(UnconfiguredProject unconfigProject, string configurationName) {
            configurationName = configurationName.Replace("Any CPU", "AnyCPU");
            var configurationProps = ConfigurationPropertiesFromConfigurationName(configurationName);
            var configProject = await unconfigProject.LoadConfiguredProjectAsync(configurationName, configurationProps);
            if (configProject == null) {
                throw new ArgumentException();
            }
            return configProject.Services.ExportProvider.GetExportedValue<T>();
        }

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
