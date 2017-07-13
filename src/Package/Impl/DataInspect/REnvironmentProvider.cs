// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Host.Client;
using Microsoft.R.StackTracing;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironmentProvider : BindableBase, IREnvironmentProvider {
        private static readonly string[] _hiddenEnvironments = { "Autoloads", "rtvs::graphics::ide", ".rtvs" };
        private static readonly IReadOnlyList<REnvironment> _errorEnvironments = new[] { REnvironment.Error };

        private readonly DisposeToken _disposeToken = DisposeToken.Create<REnvironmentProvider>();
        private readonly IMainThread _mainThread;

        private volatile IRSession _rSession;
        private IREnvironment _selectedEnvironment;
        private BatchObservableCollection<IREnvironment> _environments = new BatchObservableCollection<IREnvironment>();

        public REnvironmentProvider(IRSession session, IMainThread mainThread) {
            _mainThread = mainThread;
            _rSession = session;
            _rSession.Mutated += RSession_Mutated;
        }

        public void Dispose() {
            if (!_disposeToken.TryMarkDisposed()) {
                return;
            }

            if (_rSession != null) {
                _rSession.Mutated -= RSession_Mutated;
                _rSession = null;
            }
        }

        public ObservableCollection<IREnvironment> Environments => _environments;

        public IREnvironment SelectedEnvironment {
            get { return _selectedEnvironment; }
            set {
                SetProperty(ref _selectedEnvironment, value);
            }
        }

        public async Task RefreshEnvironmentsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            using (_disposeToken.Link(ref cancellationToken)) {
                var session = _rSession;

                await TaskUtilities.SwitchToBackgroundThread();

                var envs = new List<REnvironment>();
                bool success = false;
                try {
                    var traceback = (await session.TracebackAsync(cancellationToken: cancellationToken))
                        .Skip(1) // skip global - it's in the search path already
                        .Select(frame => new REnvironment(frame))
                        .Reverse();
                    if (traceback.Any()) {
                        envs.AddRange(traceback);
                        envs.Add(null);
                    }

                    var searchPath = (await session.EvaluateAsync<string[]>("as.list(search())", REvaluationKind.BaseEnv, cancellationToken))
                        .Except(_hiddenEnvironments)
                        .Select(name => new REnvironment(name));
                    envs.AddRange(searchPath);

                    success = true;
                } catch (RException) {
                } catch (OperationCanceledException) {
                }

                await _mainThread.SwitchToAsync(cancellationToken);

                var oldSelection = _selectedEnvironment;
                _environments.ReplaceWith(success ? envs : _errorEnvironments);

                IREnvironment newSelection = null;
                if (oldSelection != null) {
                    newSelection = _environments?.FirstOrDefault(env => env?.Name == oldSelection.Name);
                }
                SelectedEnvironment = newSelection ?? _environments.FirstOrDefault();
            }
        }

        private void RSession_Mutated(object sender, EventArgs e) {
            RefreshEnvironmentsAsync().DoNotWait();
        }
    }
}
