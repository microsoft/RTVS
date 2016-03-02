// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Host.Client.Test.Script {
    [ExcludeFromCodeCoverage]
    public class RHostScript : IDisposable {
        private bool disposed = false;

        public IRSessionProvider SessionProvider { get; private set; }
        public IRSession Session { get; private set; }

        public RHostScript(IRSessionProvider sessionProvider, IRHostClientApp clientApp = null) {
            SessionProvider = sessionProvider;

            Session = SessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, clientApp ?? new RHostClientTestApp());
            Session.IsHostRunning.Should().BeFalse();

            Session.StartHostAsync(new RHostStartupInfo {
                Name = "RHostScript",
                RBasePath = RToolsSettings.Current.RBasePath,
                RCommandLineArguments = RToolsSettings.Current.RCommandLineArguments,
                CranMirrorName = RToolsSettings.Current.CranMirror
            }, 50000).Wait();
        }

        public void Dispose() {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposed) {
                return;
            }

            if (disposing) {
                if (Session != null) {
                    Session.StopHostAsync().Wait(5000);
                    Session.Dispose();
                    Session = null;
                }

                if (SessionProvider != null) {
                    SessionProvider.Dispose();
                    SessionProvider = null;
                }
            }

            disposed = true;
        }
    }
}
