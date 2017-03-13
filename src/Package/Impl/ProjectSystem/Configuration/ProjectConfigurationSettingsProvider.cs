// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Host.Client.Extensions;
using Microsoft.VisualStudio.ProjectSystem;
using IThreadHandling = Microsoft.VisualStudio.ProjectSystem.IProjectThreadingService;

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

        public Task<IProjectConfigurationSettingsAccess> OpenProjectSettingsAccessAsync(ConfiguredProject configuredProject) {
            var props = configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
            return OpenProjectSettingsAccessAsync(configuredProject.UnconfiguredProject, props);
        }

        public async Task<IProjectConfigurationSettingsAccess> OpenProjectSettingsAccessAsync(
            UnconfiguredProject unconfiguredProject, IRProjectProperties propertes) {

            Check.ArgumentNull(nameof(unconfiguredProject), unconfiguredProject);
            Check.ArgumentNull(nameof(propertes), propertes);
            SettingsAccess access = null;

            await _semaphore.WaitAsync();
            try {
                string projectFolder = Path.GetDirectoryName(unconfiguredProject.FullPath);
                if (!_settings.TryGetValue(projectFolder, out access)) {
                    var settings = await OpenCollectionAsync(projectFolder, propertes);
                    var th = unconfiguredProject.Services?.ExportProvider?.GetExportedValue<IThreadHandling>();
                    access = new SettingsAccess(this, th, projectFolder, propertes, settings);
                    _settings[projectFolder] = access;
                }
                access.Counter.Increment();
            } finally {
                _semaphore.Release();
            }
            return access;
        }

        private async Task<ConfigurationSettingCollection> OpenCollectionAsync(string projectFolder, IRProjectProperties properties) {
            var settings = new ConfigurationSettingCollection();
            var settingsFilePath = await GetSettingsFilePathAsync(projectFolder, properties);
            if (!string.IsNullOrEmpty(settingsFilePath)) {
                settings.Load(settingsFilePath.MakeAbsolutePathFromRRelative(projectFolder));
            }
            return settings;
        }

        private async Task<string> GetSettingsFilePathAsync(string projectFolder, IRProjectProperties properties) {
            var settingsFile = await properties.GetSettingsFileAsync();
            if (!string.IsNullOrEmpty(settingsFile)) {
                return settingsFile.MakeAbsolutePathFromRRelative(projectFolder);
            }
            return null;
        }

        private void ReleaseSettings(string projectPath, IThreadHandling threadHandling, IRProjectProperties propertes, bool save) {
            _semaphore.Wait();
            try {
                var access = _settings[projectPath];
                string settingsFile = null;
                if (string.IsNullOrEmpty(access.Settings.SourceFile)) {
                    settingsFile = Path.Combine(projectPath, "Settings.R");
                }
                if (save) {
                    access.Settings.Save(settingsFile);
                }

                threadHandling?.ExecuteSynchronously(async () => {
                    var currentSettingsFile = await propertes.GetSettingsFileAsync();
                    if (string.IsNullOrEmpty(currentSettingsFile)) {
                        settingsFile = settingsFile.MakeRRelativePath(projectPath);
                        await propertes.SetSettingsFileAsync(settingsFile);
                    }
                });
                _settings.Remove(projectPath);
            } finally {
                _semaphore.Release();
            }
        }

        class SettingsAccess : IProjectConfigurationSettingsAccess {
            private readonly IThreadHandling _threadHandling;
            private readonly ProjectConfigurationSettingsProvider _provider;
            private readonly CountdownDisposable _counter;
            private readonly string _projectPath;
            private readonly IRProjectProperties _properties;
            private bool _changed;

            public SettingsAccess(ProjectConfigurationSettingsProvider provider, IThreadHandling threadHandling,
                                  string projectPath, IRProjectProperties propertes, ConfigurationSettingCollection settings) {
                _provider = provider;
                _threadHandling = threadHandling;
                _projectPath = projectPath;
                _properties = propertes;

                _counter = new CountdownDisposable(Release);

                Settings = settings;
                Settings.CollectionChanged += OnCollectionChanged;
            }

            private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
                _changed = true;
            }

            #region IProjectConfigurationSettingsAccess
            public IConfigurationSettingCollection Settings { get; }

            public void Dispose() {
                _counter.Decrement();
            }
            #endregion

            public CountdownDisposable Counter => _counter;

            private void Release() {
                Settings.CollectionChanged -= OnCollectionChanged;
                _provider.ReleaseSettings(_projectPath, _threadHandling, _properties, _changed);
            }
        }
    }
}
