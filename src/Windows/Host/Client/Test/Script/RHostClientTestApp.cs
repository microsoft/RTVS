// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.Help;

namespace Microsoft.R.Host.Client.Test.Script {
    public class RHostClientTestApp : IRSessionCallback {
        public Func<LocatorResult> LocatorHandler { get; set; }
        public Func<Guid, PlotDeviceProperties> PlotDeviceCreateHandler { get; set; }
        public Action<Guid> PlotDeviceDestroyHandler { get; set; }
        public Action<PlotMessage> PlotHandler { get; set; }
        public IHelpVisualComponent HelpComponent { get; set; }

        public virtual string CranUrlFromName(string name) => "https://cran.rstudio.com";

        public virtual Task Plot(PlotMessage plot, CancellationToken ct) {
            if (PlotHandler != null) {
                PlotHandler(plot);
                return Task.CompletedTask;
            }
            throw new NotImplementedException();
        }

        public virtual Task ShowErrorMessage(string message, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;

        public virtual Task ShowHelpAsync(string url, CancellationToken cancellationToken = default(CancellationToken)) {
            HelpComponent.Navigate(url);
            return Task.CompletedTask;
        }

        public virtual Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken) => Task.FromResult(MessageButtons.OK);
        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) => Task.FromResult("\n");
        public Task ViewObjectAsync(string expression, string title, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task ViewLibraryAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task ViewFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<string> EditFileAsync(string expression, string fileName, CancellationToken cancellationToken) => Task.FromResult(string.Empty);

        public Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct)
            =>  LocatorHandler != null ? Task.FromResult(LocatorHandler()) : throw new NotImplementedException();

        public Task<string> FetchFileAsync(string remoteFileName, ulong remoteBlobId, string localPath, CancellationToken cancellationToken)=> Task.FromResult(localPath);

        public Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct) 
            => PlotDeviceCreateHandler != null ? Task.FromResult(PlotDeviceCreateHandler(deviceId)): throw new NotImplementedException();

        public Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct) {
            if (PlotDeviceDestroyHandler != null) {
                PlotDeviceDestroyHandler(deviceId);
                return Task.CompletedTask;
            }
            throw new NotImplementedException();
        }

        public Task BeforePackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AfterPackagesInstalledAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public string GetLocalizedString(string id) => throw new NotImplementedException();
    }
}
