using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.Threading;

namespace Microsoft.R.Host.Client.Test.Script {
    [ExcludeFromCodeCoverage]
    public class RHostScript : IDisposable {
        private bool disposed = false;

        public IRSessionProvider SessionProvider { get; private set; }
        public IRSession Session { get; private set; }

        public RHostScript(IRSessionProvider sessionProvider, IRHostClientApp clientApp = null) {
            SessionProvider = sessionProvider;
            Session = SessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, clientApp ?? new RHostClientTestApp());
            Session.StartHostAsync(new RHostStartupInfo {
                Name = "RHostScript",
                RBasePath = RToolsSettings.Current.RBasePath,
                RCommandLineArguments = RToolsSettings.Current.RCommandLineArguments,
                CranMirrorName = RToolsSettings.Current.CranMirror
            }, 10000).Wait();
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
                    Session.StopHostAsync().Wait();
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
