// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages {
    internal class RunPageViewModel : PropertyPageViewModel {
        private IRProjectProperties[] _configuredProjectsProperties;
        private string _startupFile;
        private string _commandLineArgs;
        private bool? _resetReplOnRun;
        private IEnumerable<string> _rFilePaths;
        private string _projectName;

        public RunPageViewModel(IRProjectProperties[] configuredProjectsProperties) {
            _configuredProjectsProperties = configuredProjectsProperties;
        }

        public string StartupFile {
            get { return _startupFile; }
            set { OnPropertyChanged(ref _startupFile, value); }
        }

        public string CommandLineArgs {
            get { return _commandLineArgs; }
            set { OnPropertyChanged(ref _commandLineArgs, value); }
        }

        public bool? ResetReplOnRun {
            get { return _resetReplOnRun; }
            set { OnPropertyChanged(ref _resetReplOnRun, value); }
        }

        public IEnumerable<string> RFilePaths {
            get { return _rFilePaths; }
            private set { OnPropertyChanged(ref _rFilePaths, value); }
        }

        public string ProjectName {
            get { return _projectName; }
            private set { OnPropertyChanged(ref _projectName, value); }
        }

        public async override Task Initialize() {
            ResetReplOnRun = await GetPropertyValueForSelectedConfigurationsAsync(
                async (props) => await props.GetResetReplOnRunAsync());

            StartupFile = await GetPropertyValueForSelectedConfigurationsAsync(
                async (props) => await props.GetStartupFileAsync());

            CommandLineArgs = await GetPropertyValueForSelectedConfigurationsAsync(
                async (props) => await props.GetCommandLineArgsAsync());

            RFilePaths = _configuredProjectsProperties.Length > 0 ? _configuredProjectsProperties[0].GetRFilePaths() : new List<string>();
            ProjectName = _configuredProjectsProperties.Length > 0 ? _configuredProjectsProperties[0].GetProjectName() : "<project>";
        }

        public async override Task<int> Save() {
            try {
                PushIgnoreEvents();

                foreach (var props in _configuredProjectsProperties) {
                    if (ResetReplOnRun.HasValue) {
                        await props.SetResetReplOnRunAsync(ResetReplOnRun.Value);
                    }

                    if (StartupFile != DifferentStringOptions) {
                        await props.SetStartupFileAsync(StartupFile);
                    }

                    if (CommandLineArgs != DifferentStringOptions) {
                        await props.SetCommandLineArgsAsync(CommandLineArgs);
                    }
                }

                return VSConstants.S_OK;
            } finally {
                PopIgnoreEvents();
            }
        }

        /// <summary>
        /// Get the common value for the selected configurations for a specific property.
        /// If there are multiple conflicting values, then a predefined string is returned.
        /// </summary>
        private async Task<string> GetPropertyValueForSelectedConfigurationsAsync(Func<IRProjectProperties, Task<string>> getter) {
            var all = new HashSet<string>();

            foreach (var props in _configuredProjectsProperties) {
                all.Add(await getter(props));
            }

            return all.Count == 1 ? all.First() : DifferentStringOptions;
        }

        /// <summary>
        /// Get the common value for the selected configurations for a specific property.
        /// If there are multiple conflicting values, then <c>null</c> is returned.
        /// </summary>
        private async Task<bool?> GetPropertyValueForSelectedConfigurationsAsync(Func<IRProjectProperties, Task<bool>> getter) {
            var all = new HashSet<bool>();

            foreach (var props in _configuredProjectsProperties) {
                all.Add(await getter(props));
            }

            return all.Count == 1 ? all.First() : DifferentBoolOptions;
        }
    }
}
