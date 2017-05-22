// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Components.Application.Configuration;

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
        /// Gets the destination path for a project on the remote host.
        /// </summary>
        /// <remarks>
        /// Default destination is ~/RTVSProjects/.
        /// </remarks>
        Task<string> GetRemoteProjectPathAsync();

        /// <summary>
        /// Sets the destination path for a project on the remote host.
        /// </summary>
        /// <remarks>
        /// Default destination is ~/RTVSProjects/.
        /// </remarks>
        Task SetRemoteProjectPathAsync(string remoteProjectPath);

        /// <summary>
        /// Gets the filter string that selects files to be sent to remote host.
        /// </summary>
        /// <remarks>
        /// Default filter string is ".r;.rmd;.rsettings". This selects all R script files, R markdown, 
        /// and R Settings files.
        /// </remarks>
        Task<string> GetFileFilterAsync();

        /// <summary>
        /// Sets the filter string that selects files to be sent to remote host.
        /// </summary>
        /// <remarks>
        /// Default filter string is ".r;.rmd;.rsettings". This selects all R script files, R markdown, 
        /// and R Settings files.
        /// </remarks>
        Task SetFileFilterAsync(string fileTransferFilter);

        /// <summary>
        /// Defines if project has to be transfered to remote host during run.
        /// </summary>
        Task<bool> GetTransferProjectOnRunAsync();

        /// <summary>
        /// Defines if project has to be transfered to remote host during run.
        /// </summary>
        Task SetTransferProjectOnRunAsync(bool val);

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
