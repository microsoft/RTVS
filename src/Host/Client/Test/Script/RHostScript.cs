using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Host.Client.Test.Script {
    [ExcludeFromCodeCoverage]
    public class RHostScript : IDisposable {
        private static readonly Guid InteractiveWindowRSessionGuid = new Guid("77E2BCD9-BEED-47EF-B51E-2B892260ECA7");
        private bool disposed = false;

        public IRSessionProvider SessionProvider { get; private set; }
        public IRSession Session { get; private set; }

        public RHostScript(IRSessionProvider sessionProvider) {
            SessionProvider = sessionProvider;
            Session = SessionProvider.GetOrCreate(InteractiveWindowRSessionGuid, new RHostClientTestApp());
            Session.StartHostAsync(new RHostStartupInfo {
                Name = "RHostScript",
                RBasePath = RToolsSettings.Current.RBasePath,
                RCommandLineArguments = RToolsSettings.Current.RCommandLineArguments,
                CranMirrorName = RToolsSettings.Current.CranMirror
            }).Wait();
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
