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
        private EnvDTE.Project _activeProject;
        private string _currentFile;

        public SettingsPageViewModel(IConfigurationSettingsService css, ICoreShell coreShell, IFileSystem fileSystem, IProjectSystemServices pss) {
            _css = css;
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
                _css.Load(_currentFile);
            }
        }

        public SettingsTypeDescriptor TypeDescriptor {
            get {
                try {
                    return new SettingsTypeDescriptor(_css.Settings);
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToReadSettings, ex.Message));
                }
                return null;
            }
        }

        /// <summary>
        /// Retrieves all files with settings.r tail
        /// </summary>
        /// <returns></returns>
        public void Initialize() {
            _activeProject = _pss.GetActiveProject();
            _filesMap.Clear();
            try {
                EnumerateSettingFiles();
            } catch (COMException) { } catch (IOException) { } catch (AccessViolationException) { }
        }

        public void AddSetting(string name, string value, ConfigurationSettingValueType valueType) {
            var setting = _css.AddSetting(name);
            setting.Value = value;
            setting.ValueType = valueType;
        }

        public void RemoveSetting(IConfigurationSetting s) {
            _css.RemoveSetting(s);
        }

        public bool Save() {
            try {
                _css.Save(_currentFile);
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToSaveSettings, _currentFile, ex.Message));
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
