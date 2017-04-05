// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client {
    internal sealed class RSessionCallback : IRSessionCallback {
        private readonly IRHostSessionCallback _cb;

        public RSessionCallback(IRHostSessionCallback cb) {
            _cb = cb;
        }

        internal byte[] PlotResult { get; private set; } = new byte[0];
        internal PlotDeviceProperties PlotDeviceProperties { get; set; } = new PlotDeviceProperties(1024, 1024, 96);

        public string CranUrlFromName(string name) => "https://cran.rstudio.com";

        public Task<string> FetchFileAsync(string remoteFileName, ulong remoteBlobId, string localPath, CancellationToken cancellationToken)
            => Task.FromResult(string.Empty);

        public Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct)
            => Task.FromResult(new LocatorResult());

        public Task Plot(PlotMessage plot, CancellationToken ct) {
            PlotResult = plot.Data;
            return Task.CompletedTask;
        }

        public Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct)
            => Task.FromResult(PlotDeviceProperties);

        public Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct)
            => Task.CompletedTask;

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct)
            => Task.FromResult(string.Empty);

        public Task ShowErrorMessage(string message, CancellationToken cancellationToken = default(CancellationToken))
            => _cb.ShowErrorMessageAsync(message, cancellationToken);

        public Task ShowHelpAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
            => Task.CompletedTask;

        public Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken))
            => _cb.ShowMessageAsync(message, buttons, cancellationToken);

        public Task ViewFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
        public Task<string> EditFileAsync(string expression, string fileName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(string.Empty);
        public Task ViewLibraryAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
        public Task ViewObjectAsync(string expression, string title, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;

        public string GetLocalizedString(string id) => null;
        public Task BeforePackagesInstalledAsync(CancellationToken ct) => Task.CompletedTask;
        public Task AfterPackagesInstalledAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
