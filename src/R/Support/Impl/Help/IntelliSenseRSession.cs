// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help {
    [Export(typeof(IIntellisenseRSession))]
    public sealed class IntelliSenseRSession : IIntellisenseRSession {
        private static readonly Guid SessionId = new Guid("8BEF9C06-39DC-4A64-B7F3-0C68353362C9");
        private readonly ICoreShell _coreShell;
        private readonly IRSessionProvider _sessionProvider;
        private readonly BinaryAsyncLock _lock = new BinaryAsyncLock();

        [ImportingConstructor]
        public IntelliSenseRSession(ICoreShell coreShell, IRInteractiveWorkflowProvider workflowProvider) {
            _coreShell = coreShell;
            _sessionProvider = workflowProvider.GetOrCreate().RSessions;
        }

        /// <summary>
        /// Timeout to allow R-Host to start. Typically only needs
        /// different value in tests or code coverage runs.
        /// </summary>
        public static int HostStartTimeout { get; set; } = 3000;

        public IRSession Session { get; private set; }

        public void Dispose() {
            Session?.Dispose();
            Session = null;
        }

        public async Task CreateSessionAsync() {
            await _lock.WaitAsync();
            try {
                if (Session == null) {
                    Session = _sessionProvider.GetOrCreate(SessionId);
                }

                if (!Session.IsHostRunning) {
                    int timeout = _coreShell.IsUnitTestEnvironment ? 10000 : 3000;
                    await Session.EnsureHostStartedAsync(new RHostStartupInfo {
                        Name = "IntelliSense",
                        CranMirrorName = RToolsSettings.Current.CranMirror,
                        CodePage = RToolsSettings.Current.RCodePage
                    }, null, timeout);
                }
            } finally {
                _lock.Release();
            }
        }
    }
}
