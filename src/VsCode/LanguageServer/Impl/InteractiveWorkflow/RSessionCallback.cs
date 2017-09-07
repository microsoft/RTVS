// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.UI;
using Microsoft.R.Host.Client;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    internal sealed class RSessionCallback : IRSessionCallback {
        public Task ShowErrorMessage(string message, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

        public Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons,
            CancellationToken cancellationToken = new CancellationToken()) => Task.FromResult(MessageButtons.OK);

        public Task ShowHelpAsync(string url, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

        public Task Plot(PlotMessage plot, CancellationToken ct) => Task.CompletedTask;

        public Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct) 
            => Task.FromResult(LocatorResult.CreateNotClicked());

        public Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct) 
            => Task.FromResult(new PlotDeviceProperties(640, 480, 96));

        public Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct) => Task.CompletedTask;

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) => Task.FromResult(string.Empty);

        public string CranUrlFromName(string name) => null;

        public Task ViewObjectAsync(string expression, string title, CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

        public Task ViewLibraryAsync(CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

        public Task ViewFile(string fileName, string tabName, bool deleteFile,
            CancellationToken cancellationToken = new CancellationToken()) => Task.CompletedTask;

        public Task<string> EditFileAsync(string content, string fileName, CancellationToken cancellationToken = new CancellationToken())
            => Task.FromResult(string.Empty);
        public Task<string> FetchFileAsync(string remoteFileName, ulong remoteBlobId, string localPath, CancellationToken cancellationToken)
            => Task.FromResult(string.Empty);

        public string GetLocalizedString(string id) => id;

        public Task BeforePackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AfterPackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
