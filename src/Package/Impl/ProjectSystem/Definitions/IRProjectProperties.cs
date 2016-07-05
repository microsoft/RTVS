// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    /// <summary>
    /// R application project properties
    /// </summary>
    internal interface IRProjectProperties {
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
        /// R file that contains project settings as R code.
        /// the file is to be sourced before running the application.
        /// Null if no settings are defined. In order to read actual
        /// settings, use <see cref="ConfigurationSettingCollection"/>
        /// </summary>
        string SettingsFile { get; set; }
    }
}
