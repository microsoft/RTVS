// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Implements callback for R session. Client code can derive
    /// from this class and override methods allowing to receive
    /// additional information and handle requsts from the R session.
    /// </summary>
    public class RHostSessionCallback: IRHostSessionCallback {
        /// <summary>
        /// Called when R session wants to display error message box.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Awaitable task</returns>
        public virtual Task ShowErrorMessageAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
            => Task.CompletedTask;

        /// <summary>
        /// Called when R session wants to display a message box with Yes/No/Ok/Cancel buttons.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="buttons">Message box buttons</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Button pressed</returns>
        public virtual Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(MessageButtons.OK);

        /// <summary>
        /// Called when R sessions needs to display a plot
        /// </summary>
        /// <param name="image">Image data</param>
        /// <returns>Awaitable task</returns>
        public virtual Task PlotAsync(byte[] image)
            => Task.CompletedTask;

        /// <summary>
        /// Called by R before plotting to get information on the image dimensions and resolution
        /// </summary>
        public virtual PlotDeviceProperties PlotDeviceProperties
            => new PlotDeviceProperties(1024, 1024, 96);

        /// <summary>
        /// Called by R to output text or error message to console
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="error">Indicates if message is an error message</param>
        public virtual void Output(string message, bool error) { }
    }
}
