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
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Host.Client.Extensions;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    internal sealed class SettingsPageViewModel {
        private readonly Dictionary<string, string> _filesMap = new Dictionary<string, string>();
        private readonly IConfigurationSettingCollection _settings;
        private readonly IFileSystem _fileSystem;
        private readonly ICoreShell _coreShell;
        private IRProjectProperties _properties;
        private string _currentFile;
        private string _projectPath;

        public SettingsPageViewModel(IConfigurationSettingCollection settings, ICoreShell coreShell, IFileSystem fileSystem) {
            Check.ArgumentNull(nameof(settings), settings);
            Check.ArgumentNull(nameof(coreShell), coreShell);
            Check.ArgumentNull(nameof(fileSystem), fileSystem);

            _settings = settings;
            _coreShell = coreShell;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Files => _filesMap.Keys;
        public SettingsTypeDescriptor TypeDescriptor => new SettingsTypeDescriptor(_coreShell, _settings);

        public string CurrentFile {
            get => _currentFile;
            set {
                if (value != null && !value.EqualsIgnoreCase(_currentFile)) {
                    try {
                        var fullPath = GetFullPath(value);
                        if (!string.IsNullOrEmpty(fullPath)) {
                            _settings.Load(fullPath);
                        }
                        _currentFile = value;
                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                        _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToReadSettings, ex.Message));
                    }
                }
            }
        }

        public async Task SetProjectPathAsync(string projectPath, IRProjectProperties properties) {
            _projectPath = projectPath;
            _properties = properties;
            try {
                EnumerateSettingFiles(projectPath);
            } catch (COMException) { } catch (IOException) { } catch (UnauthorizedAccessException) { }

            if (_properties != null) {
                var file = await _properties.GetSettingsFileAsync();
                if (!string.IsNullOrEmpty(file) && _filesMap.ContainsKey(file)) {
                    CurrentFile = file;
                }
            }

            if (CurrentFile == null) {
                CurrentFile = _filesMap.Keys.FirstOrDefault();
            }
        }

        public void AddSetting(string name, string value, ConfigurationSettingValueType valueType) {
            _settings.Add(new ConfigurationSetting(name, value, valueType));
        }

        public void RemoveSetting(IConfigurationSetting s) {
            _settings.Remove(s);
        }

        public async Task<bool> SaveAsync() {
            var fullPath = GetFullPath(CurrentFile);
            if (!string.IsNullOrEmpty(fullPath)) {
                try {
                    _settings.Save(fullPath);
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
                // Remember R path like ~/... so when project moves we can still find the file
                await _properties.SetSettingsFileAsync(CurrentFile);
            }
        }

        public void CreateNewSettingsFile() {
            if (!string.IsNullOrEmpty(_projectPath)) {
                var fullPath = Path.Combine(_projectPath, "Settings.R");
                _currentFile = fullPath.MakeRRelativePath(_projectPath);
                _filesMap[_currentFile] = fullPath;
            }
        }

        private string GetFullPath(string rPath) {
            string fullPath = null;
            if (rPath != null) {
                _filesMap.TryGetValue(rPath, out fullPath);
            }
            return fullPath;
        }

        private void EnumerateSettingFiles(string directory) {
            var entries = _fileSystem.GetFileSystemEntries(directory, ProjectSettingsFiles.SettingsFilePattern, SearchOption.AllDirectories);
            foreach (var entry in entries) {
                var fileName = Path.GetFileName(entry);
                if (ProjectSettingsFiles.IsProjectSettingFile(fileName)) {
                    var relativePath = entry.MakeRRelativePath(_projectPath);
                    _filesMap[relativePath] = entry;
                }
            }
        }
    }
}
