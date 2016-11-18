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
using Microsoft.Common.Wpf;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Host.Client;
using Microsoft.R.StackTracing;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironmentProvider : BindableBase, IREnvironmentProvider {
        private static readonly string[] _hiddenEnvironments = { "Autoloads", "rtvs::graphics::ide", ".rtvs" };
        private static readonly IReadOnlyList<REnvironment> _errorEnvironments = new[] { REnvironment.Error };

        private readonly DisposeToken _disposeToken = DisposeToken.Create<REnvironmentProvider>();
        private volatile IRSession _rSession;
        private readonly CancellationTokenSource _refreshCts = new CancellationTokenSource();
        private IREnvironment _selectedEnvironment;
        private BatchObservableCollection<IREnvironment> _environments = new BatchObservableCollection<IREnvironment>();

        public REnvironmentProvider(IRSession session) {
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

                cancellationToken.ThrowIfCancellationRequested();

                var envs = new List<REnvironment>();
                bool success = false;
                try {
                    var traceback = (await session.TracebackAsync())
                        .Skip(1) // skip global - it's in the search path already
                        .Select(frame => new REnvironment(frame))
                        .Reverse();
                    if (traceback.Any()) {
                        envs.AddRange(traceback);
                        envs.Add(null);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    var searchPath = (await session.EvaluateAsync<string[]>("as.list(search())", REvaluationKind.BaseEnv))
                        .Except(_hiddenEnvironments)
                        .Select(name => new REnvironment(name));
                    envs.AddRange(searchPath);

                    cancellationToken.ThrowIfCancellationRequested();

                    success = true;
                } catch (RException) {
                } catch (OperationCanceledException) {
                }

                VsAppShell.Current.DispatchOnUIThread(() => {
                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    var oldSelection = _selectedEnvironment;
                    _environments.ReplaceWith(success ? envs : _errorEnvironments);

                    IREnvironment newSelection = null;
                    if (oldSelection != null) {
                        newSelection = _environments?.FirstOrDefault(env => env?.Name == oldSelection.Name);
                    }
                    SelectedEnvironment = newSelection ?? _environments.FirstOrDefault();
                });
            }
        }

        private void RSession_Mutated(object sender, EventArgs e) {
            RefreshEnvironmentsAsync().DoNotWait();
        }
    }
}
