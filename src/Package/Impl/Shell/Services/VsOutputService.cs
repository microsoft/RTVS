// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Common.Core.Output;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class VsOutputService : IOutputService {
        private readonly IServiceContainer _services;
        private readonly ConcurrentDictionary<string, IOutput> _outputs;

        public VsOutputService(IServiceContainer services) {
            _services = services;
            _outputs = new ConcurrentDictionary<string, IOutput>();
        }

        public IOutput Get(string name) => _outputs.GetOrAdd(name, CreateOutput);

        private IOutput CreateOutput(string name) => new LogWriterOutput(_services, name);

        private class LogWriterOutput : IOutput {
            private readonly IServiceContainer _services;
            private readonly string _name;
            private IVsOutputWindowPane _pane;

            public LogWriterOutput(IServiceContainer services, string name) {
                _services = services;
                _name = name;
            }

            public void Write(string text) {
                Dispatcher.CurrentDispatcher.VerifyAccess();
                EnsurePane();
                _pane?.OutputStringThreadSafe(text);
            }

            public void WriteError(string text) {
                Dispatcher.CurrentDispatcher.VerifyAccess();
                EnsurePane();
                // TODO: When IVsOutputWindow3.CreatePane2 is implemented, we should add colorization for errors
                // See Microsoft.VisualStudio.Editor.Implementation.OutputClassifier.OutputWindowStyleManager
                // For now, just set focus
                ActivateWindow();
                _pane?.OutputStringThreadSafe(text);
            }

            private void EnsurePane() {
                Dispatcher.CurrentDispatcher.VerifyAccess();
                if (_pane == null) {
                    // TODO: consider using IVsOutputWindow3.CreatePane2 and colorize the output
                    var outputWindow = _services.GetService<IVsOutputWindow>(typeof(SVsOutputWindow));
                    var paneGuid = _name.ToGuid();
                    outputWindow?.GetPane(ref paneGuid, out _pane);
                    if (_pane == null && outputWindow != null) {
                        outputWindow.CreatePane(ref paneGuid, _name, fInitVisible: 1, fClearWithSolution: 1);
                        outputWindow.GetPane(ref paneGuid, out _pane);

                        Debug.Assert(_pane != null, "Cannot create output window pane " + _name);
                    }

                    ActivateWindow();
                }
            }

            private void ActivateWindow() {
                Dispatcher.CurrentDispatcher.VerifyAccess();
                _pane?.Activate();

                var dte = _services.GetService<DTE>();
                var window = dte?.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                window?.Activate();
            }
        }
    }
}