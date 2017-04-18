// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.UI;

namespace Microsoft.R.Host.Client.Test.Stubs {
    public class RSessionCallbackStub : IRSessionCallback {
        public IList<string> ShowErrorMessageCalls { get; } = new List<string>();
        public IList<Tuple<string, MessageButtons>> ShowMessageCalls { get; } = new List<Tuple<string, MessageButtons>>();
        public IList<string> HelpUrlCalls { get; } = new List<string>();
        public IList<Tuple<PlotMessage, CancellationToken>> PlotCalls { get; } = new List<Tuple<PlotMessage, CancellationToken>>();
        public IList<CancellationToken> LocatorCalls { get; } = new List<CancellationToken>();
        public IList<Tuple<Guid, CancellationToken>> PlotDeviceCreateCalls { get; } = new List<Tuple<Guid, CancellationToken>>();
        public IList<Tuple<Guid, CancellationToken>> PlotDeviceDestroyCalls { get; } = new List<Tuple<Guid, CancellationToken>>();
        public IList<Tuple<string, int, CancellationToken>> ReadUserInputCalls { get; } = new List<Tuple<string, int, CancellationToken>>();
        public IList<string> CranUrlFromNameCalls { get; } = new List<string>();
        public IList<Tuple<string, string>> ViewObjectCalls { get; } = new List<Tuple<string, string>>();
        public IList<int> ViewLibraryCalls { get; } = new List<int>();
        public IList<Tuple<string, string, bool>> ShowFileCalls { get; } = new List<Tuple<string, string, bool>>();
        public IList<Tuple<string, string>> EditFileCalls { get; } = new List<Tuple<string, string>>();
        public IList<Tuple<string, string>> SaveFileCalls { get; } = new List<Tuple<string, string>>();
        public IList<string> GetLocalizedStringCalls { get; } = new List<string>();

        public Func<string, MessageButtons, Task<MessageButtons>> ShowMessageCallsHandler { get; set; } = 
            (m, b) => Task.FromResult(b.HasFlag(MessageButtons.Yes) ? MessageButtons.Yes : MessageButtons.OK);

        public Func<string, int, CancellationToken, Task<string>> ReadUserInputHandler { get; set; } = (m, l, ct) => Task.FromResult("\n");
        public Func<PlotMessage, CancellationToken, Task> PlotHandler { get; set; } = (deviceId, ct) => Task.CompletedTask;
        public Func<Guid, CancellationToken, Task<LocatorResult>> LocatorHandler { get; set; } = (deviceId, ct) => Task.FromResult(LocatorResult.CreateNotClicked());
        public Func<Guid, CancellationToken, Task<PlotDeviceProperties>> PlotDeviceCreateHandler { get; set; } = (deviceId, ct) => Task.FromResult(PlotDeviceProperties.Default);
        public Func<Guid, CancellationToken, Task> PlotDeviceDestroyHandler { get; set; } = (deviceId, ct) => Task.CompletedTask;

        public Func<string, string> CranUrlFromNameHandler { get; set; } = s => "https://cran.rstudio.com";
        public Func<string, string, Task> ViewObjectHandler { get; set; } = (x, t) => Task.CompletedTask;
        public Action ViewLibraryHandler { get; set; } = () => { };
        public Func<string, string, bool, Task> ShowFileHandler { get; set; } = (f, t, d) => Task.CompletedTask;
        public Func<string, string, Task<string>> EditFileHandler { get; set; } = (e, f) => Task.FromResult(string.Empty);
        public Func<string, ulong, string, Task<string>> SaveFileHandler { get; set; } = (r, b, l) => Task.FromResult(string.Empty);

        public Task ShowErrorMessage(string message, CancellationToken cancellationToken = default(CancellationToken)) {
            ShowErrorMessageCalls.Add(message);
            return Task.CompletedTask;
        }

        public Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken = default(CancellationToken)) {
            ShowMessageCalls.Add(new Tuple<string, MessageButtons>(message, buttons));
            return ShowMessageCallsHandler != null ? ShowMessageCallsHandler(message, buttons) : Task.FromResult(default(MessageButtons));
        }

        public Task ShowHelpAsync(string url, CancellationToken cancellationToken) {
            HelpUrlCalls.Add(url);
            return Task.CompletedTask;
        }

        public Task Plot(PlotMessage plot, CancellationToken ct) {
            PlotCalls.Add(new Tuple<PlotMessage, CancellationToken>(plot, ct));
            return PlotHandler != null ? PlotHandler(plot, ct) : Task.CompletedTask;
        }

        public Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct) {
            LocatorCalls.Add(ct);
            return LocatorHandler != null ? LocatorHandler(deviceId, ct) : Task.FromResult(LocatorResult.CreateNotClicked());
        }

        public Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct) {
            PlotDeviceCreateCalls.Add(new Tuple<Guid, CancellationToken>(deviceId, ct));
            return PlotDeviceCreateHandler != null ? PlotDeviceCreateHandler(deviceId, ct) : Task.FromResult(PlotDeviceProperties.Default);
        }

        public Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct) {
            PlotDeviceDestroyCalls.Add(new Tuple<Guid, CancellationToken>(deviceId, ct));
            return PlotDeviceDestroyHandler != null ? PlotDeviceDestroyHandler(deviceId, ct) : Task.CompletedTask;
        }

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) {
            ReadUserInputCalls.Add(new Tuple<string, int, CancellationToken>(prompt, maximumLength, ct));
            return ReadUserInputHandler != null ? ReadUserInputHandler(prompt, maximumLength, ct) : Task.FromResult(string.Empty);
        }

        public string CranUrlFromName(string name) {
            CranUrlFromNameCalls.Add(name);
            return CranUrlFromNameHandler != null ? CranUrlFromNameHandler(name) : string.Empty;
        }

        public Task ViewObjectAsync(string expression, string title, CancellationToken cancellationToken) {
            ViewObjectCalls.Add(new Tuple<string, string>(expression, title));
            ViewObjectHandler?.Invoke(expression, title);
            return Task.CompletedTask;
        }

        public Task ViewLibraryAsync(CancellationToken cancellationToken) {
            ViewLibraryCalls.Add(0);
            ViewLibraryHandler?.Invoke();
            return Task.CompletedTask;
        }

        public Task ViewFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken) {
            ShowFileCalls.Add(new Tuple<string, string, bool>(fileName, tabName, deleteFile));
            ShowFileHandler?.Invoke(fileName, tabName, deleteFile);
            return Task.CompletedTask;
        }

        public Task<string> EditFileAsync(string expression, string fileName, CancellationToken cancellationToken) {
            EditFileCalls.Add(new Tuple<string, string>(expression, fileName));
            EditFileHandler?.Invoke(expression, fileName);
            return Task.FromResult(string.Empty);
        }

        public Task<string> FetchFileAsync(string remotePath, ulong remoteBlobId, string localPath, CancellationToken cancellationToken) {
            SaveFileCalls.Add(new Tuple<string, string>(remotePath, localPath));
            SaveFileHandler?.Invoke(remotePath, remoteBlobId, localPath);
            return Task.FromResult(string.Empty);
        }

        public string GetLocalizedString(string id) {
            GetLocalizedStringCalls.Add(id);
            return null;
        }

        public Task BeforePackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AfterPackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
