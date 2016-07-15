// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help {
    [Export(typeof(IIntellisenseRHost))]
    public sealed class IntelliSenseRHost : IIntellisenseRHost {
        private static readonly Guid SessionId = new Guid("8BEF9C06-39DC-4A64-B7F3-0C68353362C9");
        private readonly ICoreShell _coreShell;
        private readonly IRSessionProvider _sessionProvider;
        private readonly SemaphoreSlim _sessionSemaphore = new SemaphoreSlim(1, 1);

        [ImportingConstructor]
        public IntelliSenseRHost(ICoreShell coreShell, IRSessionProvider sessionProvider) {
            _coreShell = coreShell;
            _sessionProvider = sessionProvider;
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
            await _sessionSemaphore.WaitAsync();
            try {
                if (Session == null) {
                    Session = _sessionProvider.GetOrCreate(SessionId);
                }

                if (!Session.IsHostRunning) {
                    int timeout = _coreShell.IsUnitTestEnvironment ? 10000 : 3000;
                    await Session.StartHostAsync(new RHostStartupInfo {
                        Name = "IntelliSense",
                        RBasePath = RToolsSettings.Current.RBasePath,
                        CranMirrorName = RToolsSettings.Current.CranMirror,
                        CodePage = RToolsSettings.Current.RCodePage
                    }, null, timeout);
                }
            } finally {
                _sessionSemaphore.Release();
            }
        }
    }
}
