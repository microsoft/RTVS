using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class VsRHostScript : RHostScript {
        public VsRHostScript(IRHostClientApp clientApp = null) : 
            base(VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>(), clientApp) {
        }

        public static void DoIdle(int ms) {
            UIThreadHelper.Instance.Invoke(() => {
                int time = 0;
                while (time < ms) {
                    TestScript.DoEvents();
                    VsAppShell.Current.DoIdle();
                    EditorShell.Current.DoIdle();

                    Thread.Sleep(20);
                    time += 20;
                }
            });
        }
    }
}
