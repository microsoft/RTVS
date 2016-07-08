// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Host.Client.Extensions;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    internal sealed class SettingsPageViewModel: IDisposable {
        private readonly Dictionary<string, string> _filesMap = new Dictionary<string, string>();
        private readonly IProjectConfigurationSettingsProvider _settingProvider;
        private readonly IFileSystem _fileSystem;
        private readonly ICoreShell _coreShell;
        private IProjectConfigurationSettingsAccess _access;
        private IRProjectProperties[] _properties;
        private string _currentFile;
        private string _projectPath;

        public SettingsPageViewModel(IProjectConfigurationSettingsProvider settingsProvider, ICoreShell coreShell, IFileSystem fileSystem) {
            _settingProvider = settingsProvider;
            _coreShell = coreShell;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Files => _filesMap.Keys;
        public SettingsTypeDescriptor TypeDescriptor => new SettingsTypeDescriptor(_coreShell, _settings);

        public string CurrentFile {
            get { return _currentFile; }
            set {
                if(_access == null) {
                    throw new InvalidOperationException("Set path to the project first");
                }
                if (value != null && !value.EqualsIgnoreCase(_currentFile)) {
                    try {
                        var fullPath = GetFullPath(value);
                        if (!string.IsNullOrEmpty(fullPath)) {
                            _access.Settings.Load(fullPath);
                        }
                        _currentFile = value;
                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                        _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToReadSettings, ex.Message));
                    }
                }
            }
        }

        public async Task SetProjectAsync(ConfiguredProject project) {
            _projectPath = Path.GetDirectoryName(project.UnconfiguredProject.FullPath);
            _access = await _settingProvider.OpenProjectSettingsAccessAsync(project);
            try {
                EnumerateSettingFiles(_projectPath);
            } catch (COMException) { } catch (IOException) { } catch (AccessViolationException) { }

            if (_properties?.Length > 0) {
                var file = await _properties[0].GetSettingsFileAsync();
                if (_filesMap.ContainsKey(file)) {
                    CurrentFile = file;
                }
            }

            if (CurrentFile == null) {
                CurrentFile = _filesMap.Keys.FirstOrDefault();
            }
        }

        public void AddSetting(string name, string value, ConfigurationSettingValueType valueType) {
            _access.Settings.Add(new ConfigurationSetting(name, value, valueType));
        }

        public void RemoveSetting(IConfigurationSetting s) {
            _access.Settings.Remove(s);
        }

        public async Task<bool> SaveAsync() {
            var fullPath = GetFullPath(CurrentFile);
            if (!string.IsNullOrEmpty(fullPath)) {
                try {
                    _access.Settings.Save(fullPath);
                    await SaveSelectedSettingsFileNameAsync();
                    return true;
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToSaveSettings, fullPath, ex.Message));
                }
            }
            return false;
        }

        public async Task SaveSelectedSettingsFileNameAsync() {
            if (_properties != null) {
                foreach (var props in _properties) {
                    // Remember R path like ~/... so when project moves we can still find the file
                    await props.SetSettingsFileAsync(CurrentFile);
                }
            }
        }

        public void CreateNewSettingsFile() {
            if (!string.IsNullOrEmpty(_projectPath)) {
                var fullPath = Path.Combine(_projectPath, "Settings.R");
                _currentFile = fullPath.MakeRRelativePath(_projectPath);
                _filesMap[_currentFile] = fullPath;
            }
        }

        public void Dispose() {
            _access?.Dispose();
            _access = null;
        }

        private string GetFullPath(string rPath) {
            string fullPath = null;
            if (rPath != null) {
                _filesMap.TryGetValue(rPath, out fullPath);
            }
            return fullPath;
        }

        private void EnumerateSettingFiles(string directory) {
            var entries = _fileSystem.GetFileSystemEntries(directory, "*settings*.r", SearchOption.AllDirectories);
            foreach (var entry in entries) {
                var fileName = Path.GetFileName(entry);
                if (fileName.StartsWithIgnoreCase("settings") || fileName.EndsWithIgnoreCase(".settings.r")) {
                    var relativePath = entry.MakeRRelativePath(_projectPath);
                    _filesMap[relativePath] = entry;
                }
            }
        }
    }
}
