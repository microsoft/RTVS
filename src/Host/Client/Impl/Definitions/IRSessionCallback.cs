// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.UI;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Implemented by the application that uses Microsoft.R.Host.Client.
    /// Provides services for plotting, help display, etc.
    /// </summary>
    public interface IRSessionCallback {
        /// <summary>
        /// Displays error message in the host-specific UI
        /// </summary>
        Task ShowErrorMessage(string message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Displays R help URL in a browser or in the host provided window
        /// </summary>
        Task ShowHelpAsync(string url, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Displays R plot in the host app-provided window
        /// </summary>
        Task Plot(PlotMessage plot, CancellationToken ct);

        /// <summary>
        /// Set locator mode in the plot window
        /// </summary>
        /// <returns>Location where the user clicked.</returns>
        Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct);

        /// <summary>
        /// Device is being created, so create/assign a plot window for it.
        /// </summary>
        /// <returns>Properties for the plot device window, such as width, height and resolution.</returns>
        Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct);

        /// <summary>
        /// Device is being destroyed, so recycle its plot window.
        /// </summary>
        Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct);

        /// <summary>
        /// Requests user input from UI
        /// </summary>
        Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct);

        /// <summary>
        /// Given CRAN mirror name returns server URL
        /// </summary>
        string CranUrlFromName(string name);

        /// <summary>
        /// Opens viewer for the given object
        /// </summary>
        /// <returns></returns>
        Task ViewObjectAsync(string expression, string title, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Present package list or package manager
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task ViewLibraryAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Presents file content
        /// </summary>
        Task ViewFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Edits file or deparsed object
        /// </summary>
        Task<string> EditFileAsync(string content, string fileName, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Saves data to file sent from RHost.
        /// </summary>
        Task<string> FetchFileAsync(string remoteFileName, ulong remoteBlobId, string localPath, CancellationToken cancellationToken);

        string GetLocalizedString(string id);
        Task BeforePackagesInstalledAsync(CancellationToken cancellationToken);
        Task AfterPackagesInstalledAsync(CancellationToken cancellationToken);
    }
}
