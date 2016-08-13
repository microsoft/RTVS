// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using System.Collections.Generic;
#endif
#if VS15
using Microsoft.VisualStudio.ProjectSystem.Properties;
#endif
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.R.Package.ProjectSystem;


namespace Microsoft.VisualStudio.R.Package {
    [Export(typeof(ProjectProperties))]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal partial class ProjectProperties : StronglyTypedPropertyAccess, IRProjectProperties {
        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public ProjectProperties(ConfiguredProject configuredProject)
            : base(configuredProject) {
            _fileSystem = new FileSystem();
        }

        /// <summary>
        /// Defines if REPL is to be reset before starting the application.
        /// </summary>
        public async Task<bool> GetResetReplOnRunAsync() {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            var val = await runProps.ResetReplOnRun.GetEvaluatedValueAsync();
            return ParseBooleanProperty(val, false);
        }

        /// <summary>
        /// Defines if REPL is to be reset before starting the application.
        /// </summary>
        public async Task SetResetReplOnRunAsync(bool val) {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            await runProps.ResetReplOnRun.SetValueAsync(val);
        }

        /// <summary>
        /// Gets command line arguments of the application.
        /// </summary>
        public async Task<string> GetCommandLineArgsAsync() {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            return await runProps.CommandLineArgs.GetEvaluatedValueAsync();
        }

        /// <summary>
        /// Sets command line arguments of the application.
        /// </summary>
        public async Task SetCommandLineArgsAsync(string val) {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            await runProps.CommandLineArgs.SetValueAsync(val);
        }

        /// <summary>
        /// Defines which file contains the application entry point.
        /// </summary>
        public async Task<string> GetStartupFileAsync() {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            return await runProps.StartupFile.GetEvaluatedValueAsync();
        }

        /// <summary>
        /// Defines which file contains the application entry point.
        /// </summary>
        public async Task SetStartupFileAsync(string val) {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            await runProps.StartupFile.SetValueAsync(val);
        }

        /// <summary>
        /// Sets R path to the settings file. 
        /// </summary>
        /// <remarks>
        /// Settings file contains project settings as R code.
        /// The file is to be sourced before running the application. Null means no settings are defined. 
        /// In order to read the actual settings, use <see cref="ConfigurationSettingCollection"/>
        /// </remarks>
        public async Task<string> GetSettingsFileAsync() {
            var runProps = await this.GetConfigurationSettingsPropertiesAsync();
            return await runProps.SettingsFile.GetEvaluatedValueAsync();
        }

        /// <summary>
        /// Gets R path to the settings file. 
        /// </summary>
        /// <remarks>
        /// Settings file contains project settings as R code.
        /// The file is to be sourced before running the application. Null means no settings are defined. 
        /// In order to read the actual settings, use <see cref="ConfigurationSettingCollection"/>
        /// </remarks>
        public async Task SetSettingsFileAsync(string rFilePath) {
            var runProps = await this.GetConfigurationSettingsPropertiesAsync();
            await runProps.SettingsFile.SetValueAsync(rFilePath);
        }

        /// <summary>
        /// Gets the destination path for a project on the remote host.
        /// </summary>
        /// <remarks>
        /// Default destination is ~/RTVSProjects/.
        /// </remarks>
        public async Task<string> GetRemoteProjectPathAsync() {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            var remotePath = await runProps.RemoteProjectPath.GetEvaluatedValueAsync();
            if (string.IsNullOrWhiteSpace(remotePath)) {
                return "~/RTVSProjects/";
            }
            return remotePath;
        }

        /// <summary>
        /// Gets the destination path for a project on the remote host.
        /// </summary>
        /// <remarks>
        /// Default destination is ~/RTVSProjects/.
        /// </remarks>
        public async Task SetRemoteProjectPathAsync(string remoteProjectPath) {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            await runProps.RemoteProjectPath.SetValueAsync(remoteProjectPath);
        }

        /// <summary>
        /// Gets the filter string that selects files to be sent to remote host.
        /// </summary>
        /// <remarks>
        /// Default filter string is ".r;.rmd;.rsettings". This selects all R script files, R markdown, 
        /// and R Settings files.
        /// </remarks>
        public async Task<string> GetFileFilterAsync() {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            var filter = await runProps.TransferFilesFilter.GetEvaluatedValueAsync();
            if (string.IsNullOrWhiteSpace(filter)) {
                return ".r;.rmd;.rsettings";
            }
            return filter;
        }

        /// <summary>
        /// Gets the filter string that selects files to be sent to remote host.
        /// </summary>
        /// <remarks>
        /// Default filter string is ".r;.rmd;.rsettings". This selects all R script files, R markdown, 
        /// and R Settings files.
        /// </remarks>
        public async Task SetFileFilterAsync(string fileTransferFilter) {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            await runProps.TransferFilesFilter.SetValueAsync(fileTransferFilter);
        }

        /// <summary>
        /// Defines if project has to be transfered to remote host during run.
        /// </summary>
        public async Task<bool> GetTransferProjectOnRunAsync() {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            var val = await runProps.TransferProjectOnRun.GetEvaluatedValueAsync();
            return ParseBooleanProperty(val, false);
        }

        /// <summary>
        /// Defines if project has to be transfered to remote host during run.
        /// </summary>
        public async Task SetTransferProjectAsync(bool val) {
            var runProps = await this.GetConfigurationRunPropertiesAsync();
            await runProps.TransferProjectOnRun.SetValueAsync(val);
        }

        /// <summary>
        /// Gets all the R script files in the current project.
        /// </summary>
        public IEnumerable<string> GetRFilePaths() {
            IList<string> files = new List<string>();
            string projectDir = Path.GetDirectoryName(this.ConfiguredProject.UnconfiguredProject.FullPath);
            var projDir = _fileSystem.GetDirectoryInfo(projectDir);

            Stack<IDirectoryInfo> dirs = new Stack<IDirectoryInfo>();
            dirs.Push(projDir);

            while (dirs.Count > 0) {
                var dir = dirs.Pop();
                foreach (var info in dir.EnumerateFileSystemInfos()) {
                    var subdir = info as IDirectoryInfo;
                    if (subdir != null) {
                        dirs.Push(subdir);
                    } else if (info.FullName.EndsWithIgnoreCase(".R")) {
                        files.Add(info.FullName.Remove(0, projDir.FullName.Length + 1));
                    }
                }
            }

            return (IEnumerable<string>)files;
        }

        /// <summary>
        /// Gets the current project name.
        /// </summary>
        public string GetProjectName() {
            var projectName = Path.GetFileNameWithoutExtension(this.ConfiguredProject.UnconfiguredProject.FullPath);
            return projectName;
        }

        private static bool ParseBooleanProperty(string propertyText, bool defaultVal) {
            bool result;
            if (bool.TryParse(propertyText, out result)) {
                return result;
            }
            return defaultVal;
        }
    }
}
