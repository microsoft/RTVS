using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class RHostScript : IDisposable {
        public IRSessionProvider SessionProvider { get; private set; }
        public IRSession Session { get; private set; }

        public RHostScript() {
            SessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            Session = SessionProvider.Create(0, new RHostClientApp());
            Session.StartHostAsync("RHostScript", RToolsSettings.Current.RBasePath, RToolsSettings.Current.RCommandLineArguments, RToolsSettings.Current.CranMirror, IntPtr.Zero).Wait();
        }

        public void Dispose() {
            if (Session != null) {
                Session.StopHostAsync().Wait();
                Session.Dispose();
                Session = null;
            }

            if(SessionProvider != null) {
                SessionProvider.Dispose();
                SessionProvider = null;
            }
        }
    }
}
