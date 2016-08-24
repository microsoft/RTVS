// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client.Test.Script {
    public class RHostClientTestApp : IRSessionCallback {
        public Func<LocatorResult> LocatorHandler { get; set; }
        public Func<Guid, PlotDeviceProperties> PlotDeviceCreateHandler { get; set; }
        public Action<Guid> PlotDeviceDestroyHandler { get; set; }
        public Action<PlotMessage> PlotHandler { get; set; }

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

        public virtual Task ShowErrorMessage(string message) {
            return Task.CompletedTask;
        }

        public virtual Task ShowHelp(string url) {
            return Task.CompletedTask;
        }

        public virtual Task<MessageButtons> ShowMessage(string message, MessageButtons buttons) {
            return Task.FromResult(MessageButtons.OK);
        }

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) {
            return Task.FromResult("\n");
        }

        public void ViewObject(string expression, string title) { }

        public Task ViewLibrary() {
            return Task.CompletedTask;
        }

        public Task ViewFile(string fileName, string tabName, bool deleteFile) {
            return Task.CompletedTask;
        }

        public Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct) {
            if (LocatorHandler != null) {
                return Task.FromResult(LocatorHandler());
            }
            throw new NotImplementedException();
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
