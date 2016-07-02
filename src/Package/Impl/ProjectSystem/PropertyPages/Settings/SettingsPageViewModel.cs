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
        private readonly IConfigurationSettingsService _css;
        private readonly IFileSystem _fileSystem;
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private readonly EnvDTE.Project _activeProject;
        private string _currentFile;

        public SettingsPageViewModel(IConfigurationSettingsService css, ICoreShell coreShell, IFileSystem fileSystem, IProjectSystemServices pss) {
            _css = css;
            _coreShell = coreShell;
            _fileSystem = fileSystem;
            _pss = pss;

            _activeProject = _pss.GetActiveProject();
            try {
                EnumerateSettingFiles();
            } catch (COMException) { } catch (IOException) { } catch (AccessViolationException) { }
        }

        public IEnumerable<string> Files => _filesMap.Keys;
        public SettingsTypeDescriptor TypeDescriptor => new SettingsTypeDescriptor(_css.Settings);

        public string CurrentFile {
            get {
                return _currentFile;
            }
            set {
                if (!value.EqualsIgnoreCase(_currentFile)) {
                    _currentFile = value;
                    try {
                        _css.Load(_currentFile);
                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                        _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToReadSettings, ex.Message));
                    }
                }
            }
        }

        public void AddSetting(string name, string value, ConfigurationSettingValueType valueType) {
            _css.Settings.Add(new ConfigurationSetting(name, value, valueType));
        }

        public void RemoveSetting(IConfigurationSetting s) {
            _css.Settings.Remove(s);
        }

        public bool Save() {
            if (!string.IsNullOrEmpty(_currentFile)) {
                try {
                    _css.Save(_currentFile);
                    return true;
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToSaveSettings, _currentFile, ex.Message));
                }
            }
            return false;
        }

        private void EnumerateSettingFiles() {
            var projectFiles = _pss.GetProjectFiles(_activeProject);
            foreach (var fullPath in projectFiles) {
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
}
