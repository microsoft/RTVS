// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Settings.Mirrors;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Plots.Definitions;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.R.Components.InteractiveWorkflow.Implementation {
    internal sealed class RSessionCallback : IRSessionCallback {
        private readonly IInteractiveWindow _interactiveWindow;
        private readonly IRSession _session;
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;

        public object VsAppShell { get; private set; }

        public RSessionCallback(IInteractiveWindow interactiveWindow, IRSession session, IRSettings settings, ICoreShell coreShell) {
            _interactiveWindow = interactiveWindow;
            _session = session;
            _settings = settings;
            _coreShell = coreShell;
        }

        /// <summary>
        /// Displays error message in the host-specific UI
        /// </summary>
        public Task ShowErrorMessage(string message) {
            return _coreShell.DispatchOnMainThreadAsync(() => _coreShell.ShowErrorMessage(message));
        }

        /// <summary>
        /// Displays message with specified buttons in a host-specific UI
        /// </summary>
        public Task<MessageButtons> ShowMessage(string message, MessageButtons buttons) {
            return _coreShell.DispatchOnMainThreadAsync(() => _coreShell.ShowMessage(message, buttons));
        }

        /// <summary>
        /// Displays R help URL in a browser on in the host app-provided window
        /// </summary>
        public Task ShowHelp(string url) {
            return _coreShell.DispatchOnMainThreadAsync(() => {
                if (_settings.HelpBrowserType == HelpBrowserType.External) {
                    Process.Start(url);
                } else {
                    var container = _coreShell.ExportProvider.GetExportedValue<IHelpVisualComponentContainerFactory>().GetOrCreate();
                    container.Show(false);
                    container.Component.Navigate(url);
                }
            });
        }

        /// <summary>
        /// Displays R plot in the host app-provided window
        /// </summary>
        public Task Plot(string filePath, CancellationToken ct) {
            return _coreShell.DispatchOnMainThreadAsync(() => {
                var historyProvider = _coreShell.ExportProvider.GetExportedValue<IPlotHistoryProvider>();
                var history = historyProvider.GetPlotHistory(_session);
                history.PlotContentProvider.LoadFile(filePath);
            }, ct);
        }

        public Task<string> ReadUserInput(string prompt, int maximumLength, CancellationToken ct) {
            _coreShell.DispatchOnUIThread(() => _interactiveWindow.Write(prompt));
            return Task.Run(() => {
                using (var reader = _interactiveWindow.ReadStandardInput()) {
                    return Task.FromResult(reader.ReadToEnd());
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
            viewer?.ViewObjectDetails(expression, title);
        }

        public void ViewLibrary() {
            var viewer = _coreShell.ExportProvider.GetExportedValue<IPackageLibraryViewer>();
            viewer.ViewLibrary();
        }
    }
}
