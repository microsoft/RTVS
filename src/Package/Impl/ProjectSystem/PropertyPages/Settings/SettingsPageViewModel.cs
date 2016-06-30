// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Host.Client.Extensions;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings {
    internal sealed class SettingsPageViewModel {
        private readonly Dictionary<string, string> _filesMap = new Dictionary<string, string>();
        private readonly IFileSystem _fileSystem;
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private EnvDTE.Project _activeProject;
        private string _currentFile;
        private List<IConfigurationSetting> _settings;

        public SettingsPageViewModel(ICoreShell coreShell, IFileSystem fileSystem, IProjectSystemServices pss) {
            _coreShell = coreShell;
            _fileSystem = fileSystem;
            _pss = pss;

            Initialize();
        }

        public IEnumerable<string> Files => _filesMap.Keys;

        public string CurrentFile {
            get {
                return _currentFile;
            }
            set {
                _currentFile = value;
                LoadSettingsFromFile(_currentFile);
            }
        }

        public SettingsTypeDescriptor TypeDescriptor => _settings != null ? new SettingsTypeDescriptor(_settings) : null;

        /// <summary>
        /// Retrieves all files with settings.r tail
        /// </summary>
        /// <returns></returns>
        public void Initialize() {
            _activeProject = _pss.GetActiveProject();
            _filesMap.Clear();
            try {
                EnumerateSettingFiles(_activeProject?.ProjectItems);
            } catch (COMException) { } catch (IOException) { } catch (AccessViolationException) { }
        }

        public void AddSetting(string name, string value, ConfigurationSettingValueType valueType) {
            _settings.Add(new ConfigurationSetting(name, value, valueType));
        }

        public void RemoveSetting(IConfigurationSetting s) {
            _settings.Remove(s);
        }

        public bool Save() {
            var fullPath = _filesMap[CurrentFile];
            try {
                using (var sw = new StreamWriter(fullPath)) {
                    using (var csw = new ConfigurationSettingsWriter(sw)) {
                        csw.SaveSettings(_settings);
                        return true;
                    }
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToSaveSettings, fullPath, ex.Message));
            }
            return false;
        }


        private void EnumerateSettingFiles(EnvDTE.ProjectItems items) {
            if (items == null) {
                return;
            }
            foreach (var item in items) {
                var pi = item as EnvDTE.ProjectItem;
                if (pi.ProjectItems?.Count != 0) {
                    EnumerateSettingFiles(pi.ProjectItems);
                } else {
                    var fullPath = (item as EnvDTE.ProjectItem)?.Properties?.Item("FullPath")?.Value as string;
                    if (!string.IsNullOrEmpty(fullPath)) {
                        var fileName = Path.GetFileName(fullPath);
                        if (fileName.EqualsIgnoreCase("settings.r") || fileName.EndsWithIgnoreCase(".settings.r")) {
                            var relativePath = fullPath.MakeRRelativePath(Path.GetDirectoryName(_activeProject.FileName));
                            _filesMap[relativePath] = fullPath;
                        }
                    }
                }
            }
        }

        private IReadOnlyList<IConfigurationSetting> LoadSettingsFromFile(string filePath) {
            _settings = new List<IConfigurationSetting>();
            var fullPath = _filesMap[filePath];
            if (_fileSystem.FileExists(fullPath)) {
                try {
                    using (var sr = new StreamReader(fullPath)) {
                        using (var csr = new ConfigurationSettingsReader(sr)) {
                            _settings.AddRange(csr.LoadSettings());
                            return _settings;
                        }
                    }
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToReadSettings, ex.Message));
                }
            } else {
                _coreShell.ShowErrorMessage(Resources.Error_SettingFileNoLongerExists);
            }
            return null;
        }
    }
}
