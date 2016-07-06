// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Common.Core;
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
        private string _currentFile;
        private string _projectPath;

        public SettingsPageViewModel(IConfigurationSettingCollection settings, ICoreShell coreShell, IFileSystem fileSystem) {
            _settings = settings;
            _coreShell = coreShell;
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Files => _filesMap.Keys;
        public SettingsTypeDescriptor TypeDescriptor => new SettingsTypeDescriptor(_settings);

        public string CurrentFile {
            get {
                return _currentFile;
            }
            set {
                if (!value.EqualsIgnoreCase(_currentFile)) {
                    _currentFile = value;
                    try {
                        var fullPath = GetFullPath(_currentFile);
                        if (!string.IsNullOrEmpty(fullPath)) {
                            _settings.Load(fullPath);
                        }
                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                        _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToReadSettings, ex.Message));
                    }
                }
            }
        }

        public void SetProjectPath(string projectPath) {
            _projectPath = projectPath;
            try {
                EnumerateSettingFiles(projectPath);
            } catch (COMException) { } catch (IOException) { } catch (AccessViolationException) { }
        }

        public void AddSetting(string name, string value, ConfigurationSettingValueType valueType) {
            _settings.Add(new ConfigurationSetting(name, value, valueType));
        }

        public void RemoveSetting(IConfigurationSetting s) {
            _settings.Remove(s);
        }

        public async Task<bool> SaveAsync(IRProjectProperties[] configuredProjectsProperties) {
            var fullPath = GetFullPath(CurrentFile);
            if (!string.IsNullOrEmpty(fullPath)) {
                try {
                    _settings.Save(fullPath);
                    if (configuredProjectsProperties != null) {
                        await SaveProjectPropertiesAsync(configuredProjectsProperties);
                    }
                    return true;
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToSaveSettings, fullPath, ex.Message));
                }
            }
            return false;
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
            foreach (var entry in _fileSystem.GetFileSystemEntries(directory)) {
                if (_fileSystem.DirectoryExists(entry)) {
                    EnumerateSettingFiles(entry);
                } else {
                    var fileName = Path.GetFileName(entry);
                    if (fileName.EqualsIgnoreCase("settings.r") || fileName.EndsWithIgnoreCase(".settings.r")) {
                        var relativePath = entry.MakeRRelativePath(_projectPath);
                        _filesMap[relativePath] = entry;
                    }
                }
            }
        }

        private async Task SaveProjectPropertiesAsync(IRProjectProperties[] configuredProjectsProperties) {
            if (configuredProjectsProperties != null) {
                foreach (var props in configuredProjectsProperties) {
                    // Remember R path like ~/... so when project moves we can still find the file
                    await props.SetSettingsFileAsync(CurrentFile);
                }
            }
        }
    }
}
