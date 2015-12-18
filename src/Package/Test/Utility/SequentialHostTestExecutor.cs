using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class SequentialHostTestExecutor {
        public static IRSessionProvider SessionProvider { get; private set; }
        public static IRSession Session { get; private set; }

        public static void ExecuteTest(Action action) {
            SequentialTestExecutor.ExecuteTest((evt) => {
                action();
                evt.Set();
            },
            () => {
                if (SessionProvider == null) {
                    SessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                    Session = SessionProvider.Create(0, new RHostClientApp());
                    Session.StartHostAsync(IntPtr.Zero).Wait();
                }
            },
            () => {
                if (Session != null) {
                    Session.StopHostAsync().Wait();
                }
            });
        }

        public static void DoIdle() {
            TestScript.DoEvents();
            VsAppShell.Current.DoIdle();
            EditorShell.Current.DoIdle();
        }
    }
}
