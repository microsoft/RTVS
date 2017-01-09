// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client {
    internal sealed class RSessionSimpleCallback : IRSessionCallback {
        private readonly IRHostSessionCallback _cb;

        public RSessionSimpleCallback(IRHostSessionCallback cb) {
            _cb = cb;
        }

        public string CranUrlFromName(string name) => "https://cran.rstudio.com";
        public Task<string> FetchFileAsync(string remoteFileName, ulong remoteBlobId, string localPath, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
        public Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct) => Task.FromResult(new LocatorResult());
        public Task Plot(PlotMessage plot, CancellationToken ct) {
            return _cb.PlotAsync(plot.Data);
        }

        public Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct) => Task.FromResult(_cb.PlotDeviceProperties);
        public Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct) => Task.CompletedTask;

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) => Task.FromResult(string.Empty);

        public Task ShowErrorMessage(string message, CancellationToken cancellationToken = default(CancellationToken)) 
            => _cb.ShowErrorMessage(message, cancellationToken);

        public Task ShowHelpAsync(string url, CancellationToken cancellationToken = default(CancellationToken)) 
            => Task.CompletedTask;

        public Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken))
            => _cb.ShowMessageAsync(message, buttons, cancellationToken);

        public Task ViewFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
        public Task ViewLibraryAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
        public Task ViewObjectAsync(string expression, string title, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
    }
}
