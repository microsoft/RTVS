using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class SequentialHostTestExecutor {
        private static IRSessionProvider _sessionProvider;
        private static IRSession _session;

        public static void ExecuteTest(Action action) {
            SequentialTestExecutor.ExecuteTest((evt) => {
                action();
                evt.Set();
            },
            () => {
                if (_sessionProvider == null) {
                    _sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                    _session = _sessionProvider.Create(0, new RHostClientApp());
                    _session.StartHostAsync(IntPtr.Zero).Wait();
                }
            },
            () => {
                if (_session != null) {
                    _session.StopHostAsync().Wait();
                }
            });
        }
    }
}
