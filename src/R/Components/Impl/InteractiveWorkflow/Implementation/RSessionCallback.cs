// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Settings.Mirrors;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    internal sealed class RSessionCallback : IRSessionCallback {
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IInteractiveWindow _interactiveWindow;
        private readonly IRSession _session;
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly IFileSystem _fileSystem;

        public RSessionCallback(IInteractiveWindow interactiveWindow, IRSession session, IRSettings settings, ICoreShell coreShell, IFileSystem fileSystem) {
            _interactiveWindow = interactiveWindow;
            _session = session;
            _settings = settings;
            _coreShell = coreShell;
            _fileSystem = fileSystem;
            _workflowProvider = _coreShell.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
        }

        /// <summary>
        /// Displays error message in the host-specific UI
        /// </summary>
        public Task ShowErrorMessage(string message) => _coreShell.ShowErrorMessageAsync(message);

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        public Task<MessageButtons> ShowMessage(string message, MessageButtons buttons) => _coreShell.ShowMessageAsync(message, buttons);
            
        /// <summary>
        /// Displays R help URL in a browser on in the host app-provided window
        /// </summary>
        public async Task ShowHelp(string url) {
            await _coreShell.SwitchToMainThreadAsync();
            if (_settings.HelpBrowserType == HelpBrowserType.External) {
                Process.Start(url);
            } else {
                var container = _coreShell.ExportProvider.GetExportedValue<IHelpVisualComponentContainerFactory>().GetOrCreate();
                container.Show(focus: false, immediate: false);
                container.Component.Navigate(url);
            }
        }

        /// <summary>
        /// Displays R plot in the host app-provided window
        /// </summary>
        public async Task Plot(PlotMessage plot, CancellationToken ct) {
            var workflow = _workflowProvider.GetOrCreate();

            await _coreShell.SwitchToMainThreadAsync();
            await workflow.Plots.LoadPlotAsync(plot);
        }

        public async Task<LocatorResult> Locator(Guid deviceId, CancellationToken ct) {
            var workflow = _workflowProvider.GetOrCreate();

            await _coreShell.SwitchToMainThreadAsync();
            return await workflow.Plots.StartLocatorModeAsync(deviceId, ct);
        }

        public async Task<PlotDeviceProperties> PlotDeviceCreate(Guid deviceId, CancellationToken ct) {
            var workflow = _workflowProvider.GetOrCreate();

            await _coreShell.SwitchToMainThreadAsync();
            return await workflow.Plots.DeviceCreatedAsync(deviceId);
        }

        public async Task PlotDeviceDestroy(Guid deviceId, CancellationToken ct) {
            var workflow = _workflowProvider.GetOrCreate();

            await _coreShell.SwitchToMainThreadAsync();
            await workflow.Plots.DeviceDestroyedAsync(deviceId);
        }

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) {
            _coreShell.DispatchOnUIThread(() => _interactiveWindow.Write(prompt));
            return Task.Run(() => {
                using (var reader = _interactiveWindow.ReadStandardInput()) {
                    return reader != null ? Task.FromResult(reader.ReadToEnd()) : Task.FromResult("\n");
                }
            }, ct);
        }

        /// <summary>
        /// Given CRAN mirror name returns URL
        /// </summary>
        public string CranUrlFromName(string mirrorName) {
            return CranMirrorList.UrlFromName(mirrorName);
        }

        public void ViewObject(string expression, string title) {
            var viewer = _coreShell.ExportProvider.GetExportedValue<IObjectViewer>();
            viewer?.ViewObjectDetails(_session, REnvironments.GlobalEnv, expression, title);
        }

        public async Task ViewLibrary() {
            var containerFactory = _coreShell.ExportProvider.GetExportedValue<IRPackageManagerVisualComponentContainerFactory>();
            var workflow = _workflowProvider.GetOrCreate();

            await _coreShell.SwitchToMainThreadAsync();
            workflow.Packages.GetOrCreateVisualComponent(containerFactory, 0).Container.Show(focus: true, immediate: false);
        }

        public Task ViewFile(string fileName, string tabName, bool deleteFile) {
            var viewer = _coreShell.ExportProvider.GetExportedValue<IObjectViewer>();
            return viewer?.ViewFile(fileName, tabName, deleteFile);
        }

        public Task<string> SaveFileAsync(string filename, byte[] data) {
            return Task.Run(() => {
                string destPath = _fileSystem.GetDownloadsPath(filename);
                _fileSystem.FileWriteAllBytes(destPath, data);
                return destPath;
            });
        }
    }
}
