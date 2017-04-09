// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.UI;

namespace Microsoft.R.Host.Client {
    public interface IRCallbacks {
        Task Connected(string rVersion);
        Task Disconnected();

        Task Shutdown(bool rDataSaved);

        /// <summary>
        /// Called as a result of R calling R API 'YesNoCancel' callback
        /// </summary>
        /// <returns>Codes that match constants in RApi.h</returns>
        Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, CancellationToken ct);

        /// <summary>
        /// Called when R wants to display generic Windows MessageBox. 
        /// Graph app may call Win32 API directly rather than going via R API callbacks.
        /// </summary>
        /// <returns>Pressed button code</returns>
        Task<MessageButtons> ShowDialog(IReadOnlyList<IRContext> contexts, string s, MessageButtons buttons,
            CancellationToken ct);

        Task<string> ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, int len, bool addToHistory,
            CancellationToken ct);

        Task WriteConsoleEx(string buf, OutputType otype, CancellationToken ct);

        /// <summary>
        /// Displays error message
        /// </summary>
        Task ShowMessage(string s, CancellationToken ct);

        Task Busy(bool which, CancellationToken ct);

        /// <summary>
        /// Graphics device sends new plot information.
        /// </summary>
        Task Plot(PlotMessage plot, CancellationToken ct);

        /// <summary>
        /// Set locator mode in the plot window.
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
        /// Asks VS to open specified URL in the help window browser
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task WebBrowser(string url, CancellationToken ct);

        /// <summary>
        /// Invoked in response of parameter-less library call
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task ViewLibrary(CancellationToken cancellationToken);

        /// <summary>
        /// Invoked when R calls 'pager'
        /// </summary>
        Task ShowFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken);

        /// <summary>
        /// Invoked when R calls 'edit()'
        /// </summary>
        Task<string> EditFileAsync(string content, string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Called when working directory has changed in R.
        /// </summary>
        void DirectoryChanged();

        /// <summary>
        /// Called when used invoked View(obj) in R.
        /// </summary>
        /// <returns></returns>
        Task ViewObject(string expression, string title, CancellationToken cancellationToken);

        Task BeforePackagesInstalledAsync(CancellationToken cancellationToken);
        Task AfterPackagesInstalledAsync(CancellationToken cancellationToken);
        void PackagesRemoved();

        /// <summary>
        /// Called when user invokes rtvs:::fetch_file() in R.
        /// </summary>
        Task<string> FetchFileAsync(string remoteFileName, ulong remoteFileBlobId, string localPath,
            CancellationToken cancellationToken);

        /// <summary>
        /// Implements rtvs:::locstr().
        /// </summary>
        string GetLocalizedString(string id);
    }
}
