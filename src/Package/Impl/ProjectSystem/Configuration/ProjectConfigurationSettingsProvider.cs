// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Host.Client.Extensions;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration {
    /// <summary>
    /// Provides access to the current project configuration settings collection.
    /// The collection is shared and there may be multiple concurrent users 
    /// such as when project property page is open and user also invokes 
    /// 'Add Database Connection' command. Dispose object to release access
    /// to the collection. All access is read/write. Do not cache the object.
    /// </summary>
    [Export(typeof(IProjectConfigurationSettingsProvider))]
    internal sealed class ProjectConfigurationSettingsProvider : IProjectConfigurationSettingsProvider {
        private readonly Dictionary<string, SettingsAccess> _settings = new Dictionary<string, SettingsAccess>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<IProjectConfigurationSettingsAccess> OpenProjectSettingsAccessAsync(ConfiguredProject configuredProject) {
            Check.ArgumentNull(nameof(configuredProject), configuredProject);
            SettingsAccess access = null;

            await _semaphore.WaitAsync();
            try {
                string projectFile = configuredProject.UnconfiguredProject.FullPath;
                if (_settings.TryGetValue(projectFile, out access)) {
                    access.UseCount++;
                } else {
                    var settings = await OpenCollectionAsync(configuredProject);
                    if (settings != null) {
                        access = new SettingsAccess(this, projectFile, settings);
                        _settings[projectFile] = access;
                    }
                }

            } finally {
                _semaphore.Release();
            }

            return access;
        }

        private async Task<ConfigurationSettingCollection> OpenCollectionAsync(ConfiguredProject configuredProject) {
            var settings = new ConfigurationSettingCollection();
            var projectFolder = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
            var settingsFilePath = await GetSettingsFilePathAsync(configuredProject);
            if (string.IsNullOrEmpty(settingsFilePath)) {
                settings.Load(settingsFilePath);
            }
            return settings;
        }

        private async Task<string> GetSettingsFilePathAsync(ConfiguredProject configuredProject) {
            var projectFolder = Path.GetDirectoryName(configuredProject.UnconfiguredProject.FullPath);
            var props = configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
            if (props != null) {
                var conf = await props.GetConfigurationSettingsPropertiesAsync();
                var settingsFile = await conf.SettingsFile.GetEvaluatedValueAsync();
                if (string.IsNullOrEmpty(settingsFile)) {
                    return settingsFile.MakeAbsolutePathFromRRelative(projectFolder);
                }
            }
            return null;
        }

        private void ReleaseSettings(string projectPath, bool save) {
            _semaphore.Wait();
            try {
                var access = _settings[projectPath];
                string settingsFile = null;
                if (string.IsNullOrEmpty(access.Settings.SourceFile)) {
                    settingsFile = Path.Combine(Path.GetDirectoryName(projectPath), "Settings.R");
                }
                access.Settings.Save(settingsFile);
            } finally {
                _semaphore.Release();
            }
        }

        class SettingsAccess : IProjectConfigurationSettingsAccess {
            private readonly ProjectConfigurationSettingsProvider _provider;
            private readonly string _projectPath;
            private bool _changed;

            public SettingsAccess(ProjectConfigurationSettingsProvider provider, string projectPath, ConfigurationSettingCollection settings) {
                _provider = provider;
                _projectPath = projectPath;
                Settings = settings;
                settings.CollectionChanged += OnCollectionChanged;
            }

            private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
                _changed = true;
            }

            #region IProjectConfigurationSettingsAccess
            public ConfigurationSettingCollection Settings { get; }
            public void Dispose() {
                if (UseCount > 0) {
                    UseCount--;
                }
                if (UseCount == 0) {
                    _provider.ReleaseSettings(_projectPath, _changed);
                }
            }
            #endregion

            public int UseCount { get; set; } = 1;
        }
    }
}
