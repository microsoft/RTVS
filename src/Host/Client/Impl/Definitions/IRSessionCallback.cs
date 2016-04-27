// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Implemented by the application that uses Microsoft.R.Host.Client.
    /// Provides services for plotting, help display, etc.
    /// </summary>
    public interface IRSessionCallback {
        /// <summary>
        /// Displays error message in the host-specific UI
        /// </summary>
        Task ShowErrorMessage(string message);

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        Task<MessageButtons> ShowMessage(string message, MessageButtons buttons);

        /// <summary>
        /// Displays R help URL in a browser or in the host provided window
        /// </summary>
        Task ShowHelp(string url);

        /// <summary>
        /// Displays R plot in the host app-provided window
        /// </summary>
        Task Plot(string filePath, CancellationToken ct);

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
        void ViewObject(string expression, string title);

        /// <summary>
        /// Present package list or package manager
        /// </summary>
        void ViewLibrary();
    }
}
