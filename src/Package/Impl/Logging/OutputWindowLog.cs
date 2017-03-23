// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Logging {
    internal sealed class OutputWindowLogWriter {
        private readonly IServiceContainer _services;
        private readonly string _windowName;
        private IVsOutputWindowPane _pane;
        private Guid _paneGuid;

        public OutputWindowLogWriter(IServiceContainer services, Guid paneGuid, string windowName) {
            _services = services;
            _paneGuid = paneGuid;
            _windowName = windowName;
        }

        private void EnsurePaneVisible() {
            if (_pane == null) {
                // TODO: consider using IVsOutputWindow3.CreatePane2 and colorize the output
                var outputWindow = _services.GetService<IVsOutputWindow>(typeof(SVsOutputWindow));
                outputWindow?.GetPane(ref _paneGuid, out _pane);
                if (_pane == null && outputWindow != null) {
                    outputWindow.CreatePane(ref _paneGuid, _windowName, fInitVisible: 1, fClearWithSolution: 1);
                    outputWindow.GetPane(ref _paneGuid, out _pane);

                    Debug.Assert(_pane != null, "Cannot create output window pane " + _windowName);
                }
            }

            _pane?.Activate();

            var dte = _services.GetService<DTE>();
            var window = dte?.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            window?.Activate();
        }

        public Task WriteAsync(MessageCategory category, string message) {
            EnsurePaneVisible();
            _pane?.OutputStringThreadSafe(message);
            return Task.CompletedTask;
        }

        public void Flush() { }
    }
}
