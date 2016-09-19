// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    /// <summary>
    /// R application project properties
    /// </summary>
    public interface IRProjectProperties {
        /// <summary>
        /// Defines if REPL is to be reset before starting the application.
        /// </summary>
        Task<bool> GetResetReplOnRunAsync();

        /// <summary>
        /// Defines if REPL is to be reset before starting the application.
        /// </summary>
        Task SetResetReplOnRunAsync(bool val);

        /// <summary>
        /// Gets command line arguments of the application.
        /// </summary>
        Task<string> GetCommandLineArgsAsync();

        /// <summary>
        /// Sets command line arguments of the application.
        /// </summary>
        Task SetCommandLineArgsAsync(string val);

        /// <summary>
        /// Defines which file contains the application entry point.
        /// </summary>
        Task<string> GetStartupFileAsync();

        /// <summary>
        /// Defines which file contains the application entry point.
        /// </summary>
        Task SetStartupFileAsync(string val);

        /// <summary>
        /// Sets R path to the settings file. 
        /// </summary>
        /// <remarks>
        /// Settings file contains project settings as R code.
        /// The file is to be sourced before running the application. Null means no settings are defined. 
        /// In order to read the actual settings, use <see cref="ConfigurationSettingCollection"/>
        /// </remarks>
        Task<string> GetSettingsFileAsync();

        /// <summary>
        /// Gets R path to the settings file. 
        /// </summary>
        /// <remarks>
        /// Settings file contains project settings as R code.
        /// The file is to be sourced before running the application. Null means no settings are defined. 
        /// In order to read the actual settings, use <see cref="ConfigurationSettingCollection"/>
        /// </remarks>
        Task SetSettingsFileAsync(string rFilePath);

        /// <summary>
        /// Gets all the R script files in the current project.
        /// </summary>
        IEnumerable<string> GetRFilePaths();

        /// <summary>
        /// Gets the current project name.
        /// </summary>
        string GetProjectName();
    }
}
