// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Help;

namespace Microsoft.R.Host.Client.Test.Script {
    public class RHostClientTestApp : IRSessionCallback {
        public Func<LocatorResult> LocatorHandler { get; set; }
        public Func<Guid, PlotDeviceProperties> PlotDeviceCreateHandler { get; set; }
        public Action<Guid> PlotDeviceDestroyHandler { get; set; }
        public Action<PlotMessage> PlotHandler { get; set; }
        public IHelpVisualComponent HelpComponent { get; set; }

        public virtual string CranUrlFromName(string name) {
            return "https://cran.rstudio.com";
        }

        public virtual Task Plot(PlotMessage plot, CancellationToken ct) {
            if (PlotHandler != null) {
                PlotHandler(plot);
                return Task.CompletedTask;
            }
            throw new NotImplementedException();
        }

        public virtual Task ShowErrorMessage(string message, CancellationToken cancellationToken = default(CancellationToken)) {
            return Task.CompletedTask;
        }

        public virtual Task ShowHelpAsync(string url, CancellationToken cancellationToken = default(CancellationToken)) {
            HelpComponent.Navigate(url);
            return Task.CompletedTask;
        }

        public virtual Task<MessageButtons> ShowMessageAsync(string message, MessageButtons buttons, CancellationToken cancellationToken) {
            return Task.FromResult(MessageButtons.OK);
        }

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) {
            return Task.FromResult("\n");
        }

        public Task ViewObjectAsync(string expression, string title, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ViewLibraryAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ViewFile(string fileName, string tabName, bool deleteFile, CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        public Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct) {
            if (LocatorHandler != null) {
                return Task.FromResult(LocatorHandler());
            }
            throw new NotImplementedException();
        }

        public Task<string> FetchFileAsync(string remoteFileName, ulong remoteBlobId, string localPath, CancellationToken cancellationToken) {
            return Task.FromResult(localPath);
        }

        public Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct) {
            if (PlotDeviceCreateHandler != null) {
                return Task.FromResult(PlotDeviceCreateHandler(deviceId));
            }
            throw new NotImplementedException();
        }

        public Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct) {
            if (PlotDeviceDestroyHandler != null) {
                PlotDeviceDestroyHandler(deviceId);
                return Task.CompletedTask;
            }
            throw new NotImplementedException();
        }
    }
}
