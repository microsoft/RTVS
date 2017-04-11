// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Core.UI;
using static Microsoft.R.Host.Client.YesNoCancel;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class NullRCallbacks : IRCallbacks {
        private volatile string _input = Environment.NewLine;
        private readonly AsyncManualResetEvent _mrs = new AsyncManualResetEvent();

        public void SetReadConsoleInput(string input) {
            _input = input;
            _mrs.Set();
        }

        public Task Connected(string rVersion) => Task.CompletedTask;
        public Task Disconnected() => Task.CompletedTask;
        public Task Shutdown(bool savedRData) => Task.CompletedTask;
        public Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, CancellationToken ct) => Task.FromResult(Cancel);
        public Task<MessageButtons> ShowDialog(IReadOnlyList<IRContext> contexts, string s, MessageButtons buttons, CancellationToken ct) => Task.FromResult(MessageButtons.Cancel);
        public Task WriteConsoleEx(string buf, OutputType otype, CancellationToken ct) => Task.CompletedTask;
        public Task ShowMessage(string s, CancellationToken ct) => Task.CompletedTask;
        public Task Busy(bool which, CancellationToken ct) => Task.CompletedTask;
        public Task Plot(PlotMessage plot, CancellationToken ct) => Task.CompletedTask;
        public Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct) => Task.FromResult(LocatorResult.CreateNotClicked());
        public Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct) => Task.FromResult(PlotDeviceProperties.Default);
        public Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct) => Task.CompletedTask;
        public Task WebBrowser(string url, CancellationToken ct) => Task.CompletedTask;
        public Task ViewLibrary(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task ShowFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<string> EditFileAsync(string expression, string fileName, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
        public void DirectoryChanged() { }
        public Task ViewObject(string expression, string title, CancellationToken cancellationToken) => Task.CompletedTask;
        public void PackagesRemoved() {}
        public Task<string> FetchFileAsync(string remoteFileName, ulong remoteBlocbId, string localPath, CancellationToken cancellationToken) => Task.FromResult(string.Empty);
        public string GetLocalizedString(string id) => null;
        public Task BeforePackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AfterPackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task<string> ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, int len, bool addToHistory, CancellationToken ct) {
            await _mrs.WaitAsync(ct);
            _mrs.Reset();
            return _input;
        }
    }
}