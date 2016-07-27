// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif
#if VS15
using Microsoft.VisualStudio.ProjectSystem.Properties;
#endif
using Microsoft.VisualStudio.R.Package.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package {
    [Export]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal partial class ProjectProperties : StronglyTypedPropertyAccess, IRProjectProperties {
        [ImportingConstructor]
        public ProjectProperties(ConfiguredProject configuredProject)
            : base(configuredProject) {
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

        private static bool ParseBooleanProperty(string propertyText, bool defaultVal) {
            bool result;
            if (bool.TryParse(propertyText, out result)) {
                return result;
            }
            return defaultVal;
        }
    }
}
