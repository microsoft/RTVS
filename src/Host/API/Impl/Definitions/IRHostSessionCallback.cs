// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Implemented by the application that uses Microsoft.R.Host.Client.API
    /// Provides facilities to respond to the R engine requests, if any.
    /// Stub class <see cref="RHostSessionCallback"/> provides basic implementation.
    /// </summary>
    public interface IRHostSessionCallback {
        /// <summary>
        /// Displays error message in the application-specific UI
        /// </summary>
        Task ShowErrorMessage(string message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Displays user prompt with specified buttons in the application-specific UI
        /// </summary>
        Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Invoked as a result of R plot command
        /// </summary>
        /// <param name="imageBytes">
        /// Plot image data. Can be converted to WPF <see cref="BitmapImage"/> via
        /// <code>
        /// var image = new BitmapImage();
        ///  image.BeginInit();
        ///  image.StreamSource = new MemoryStream(imageBytes);
        ///  image.CacheOption = BitmapCacheOption.OnLoad;
        ///  image.EndInit();
        /// </code>
        /// </param>
        /// <returns>Awaitable task</returns>
        Task PlotAsync(byte[] imageBytes);

        /// <summary>
        /// Provides R with device properties such as width, height and resolution.
        /// </summary>
        /// <returns>Plot device properties</returns>
        PlotDeviceProperties PlotDeviceProperties { get; }

        /// <summary>
        /// Called when session produces output (as in the REPL output).
        /// </summary>
        void Output(string message, bool error);
    }
}
