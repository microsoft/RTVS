// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Testing;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using static System.FormattableString;

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Represents R session(s) for intellisense and signatures. The interactive session is used
    /// to determine list of loaded packages and separate session is used to fetch RD data 
    /// for function descriptions and signatures.
    /// </summary>
    public sealed class IntelliSenseRSession : IIntellisenseRSession {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly BinaryAsyncLock _lock = new BinaryAsyncLock();
        private readonly bool _unitTests;
        private volatile IEnumerable<string> _loadedPackages;
        private volatile Task _updateTask;

        public IntelliSenseRSession(IServiceContainer services) {
            Services = services;
            _workflow = services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            _workflow.RSession.Mutated += OnInteractiveSessionMutated;
            _sessionProvider = _workflow.RSessions;
            _unitTests = TestEnvironment.Current != null;
        }

        /// <summary>
        /// Timeout to allow R-Host to start. Typically only needs
        /// different value in tests or code coverage runs.
        /// </summary>
        public static int HostStartTimeout { get; set; } = 3000;

        public IServiceContainer Services { get; }

        public IRSession Session { get; private set; }

        public void Dispose() {
            _workflow.RSession.Mutated -= OnInteractiveSessionMutated;
            Session?.Dispose();
            Session = null;
        }

        /// <summary>
        /// Given function name returns package the function belongs to.
        /// The package is determined from the interactive R session since
        /// there may be functions with the same name but from different packages.
        /// Most recently loaded package typically wins.
        /// </summary>
        /// <param name="functionName">R function name</param>
        /// <returns>Function package or null if undefined</returns>
        public async Task<string> GetFunctionPackageNameAsync(string functionName) {
            var session = GetLoadedPackagesInspectionSession();
            string packageName = null;

            if (session != null && session.IsHostRunning) {
                try {
                    var candidate = await session.EvaluateAsync<string>(
                        Invariant(
                            $"as.list(find('{functionName}', mode='function')[1])[[1]]"
                        ), REvaluationKind.Normal);
                    if (candidate != null && candidate.StartsWithOrdinal("package:")) {
                        packageName = candidate.Substring(8);
                    }
                } catch (REvaluationException) { } catch(OperationCanceledException) { }
            }

            return packageName;
        }

        /// <summary>
        /// Starts intellisense session.
        /// </summary>
        public async Task StartSessionAsync(CancellationToken ct = default(CancellationToken)) {
            var token = await _lock.ResetAsync(ct);
            try {
                if (!_sessionProvider.HasBroker) {
                    throw new RHostDisconnectedException();
                }

                if (Session == null) {
                    Session = _sessionProvider.GetOrCreate(SessionNames.Intellisense);
                }

                if (!Session.IsHostRunning) {
                    int timeout = _unitTests ? 10000 : 3000;
                    var settings = Services.GetService<IRSettings>();
                    await Session.EnsureHostStartedAsync(new RHostStartupInfo(settings.CranMirror, codePage: settings.RCodePage), null, timeout, ct);
                }
            } finally {
                token.Set();
            }
        }

        public async Task StopSessionAsync(CancellationToken ct = default(CancellationToken)) {
            var token = await _lock.ResetAsync(ct);
            try {
                if (Session.IsHostRunning) {
                    await Session.StopHostAsync(waitForShutdown: true, cancellationToken: ct);
                }
            } finally {
                token.Set();
            }
        }

        /// <summary>
        /// Retrieves names of packages loaded into the interactive session.
        /// Cached list of packages may not be up to date.
        /// </summary>
        public IEnumerable<string> LoadedPackageNames {
            get {
                if (_loadedPackages == null && _workflow.RSession != null) {
                    _updateTask = UpdateListOfLoadedPackagesAsync();
                }
                return _loadedPackages ?? Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Retrieves names of packages loaded into the interactive session.
        /// </summary>
        public async Task<IEnumerable<string>> GetLoadedPackageNamesAsync(CancellationToken ct = default(CancellationToken)) {
            if (_loadedPackages == null && _updateTask == null) {
                _updateTask = UpdateListOfLoadedPackagesAsync(ct);
            }

            Debug.Assert(_updateTask != null);
            await _updateTask;

            return _loadedPackages ?? Enumerable.Empty<string>();
        }

        private void OnInteractiveSessionMutated(object sender, EventArgs e)
             => _updateTask = UpdateListOfLoadedPackagesAsync();

        private async Task UpdateListOfLoadedPackagesAsync(CancellationToken ct = default(CancellationToken)) {
            try {
                await StartSessionAsync(ct);
                var session = GetLoadedPackagesInspectionSession();
                if (session != null) {
                    _loadedPackages = await session.EvaluateAsync<string[]>("as.list(.packages())", REvaluationKind.Normal);
                }
            } catch (RHostDisconnectedException) { } catch (RException) { }
        }

        private IRSession GetLoadedPackagesInspectionSession() {
            IRSession session = null;
            // Normal case is to use the interacive session.
            if (_workflow.RSession.IsHostRunning) {
                session = _workflow.RSession;
            } else if (_unitTests) {
                // For tests that only employ standard packages we can reuse the same session.
                // This improves test performance and makes test code simpler.
                session = Session;
            }
            return session;
        }
    }
}
