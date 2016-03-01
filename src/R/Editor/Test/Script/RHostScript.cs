using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Host.Client.Test.Script {
    [ExcludeFromCodeCoverage]
    public class RHostScript : IDisposable {
        private bool _disposed = false;

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
            if (_disposed) {
                return;
            }

            if (disposing) {
                if (Session != null) {
                    Session.StopHostAsync().Wait(15000);
                    Debug.Assert(!Session.IsHostRunning);
                    Session.Dispose();
                    Session = null;
                }

                if (SessionProvider != null) {
                    SessionProvider.Dispose();
                    SessionProvider = null;
                }
            }

            _disposed = true;
        }
    }
}
