using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Microsoft.Common.Core.Shell {
    [Export(typeof(IAppShellInitialization))]
    public sealed class AppShell: IAppShellInitialization {
        public void SetShell(object shell) {
            Current = shell as IApplicationShell;
            Debug.Assert(shell != null);
        }

        public static IApplicationShell Current { get; private set; }

        public static void SetShell(IApplicationShell shell) {
            Current = shell;
        }
    }
}
