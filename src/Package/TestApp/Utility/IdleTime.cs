using Microsoft.Common.Core.Tests.Script;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Interactive.Test.Utility {
    public static class IdleTime {
        public static void DoIdle() {
            TestScript.DoEvents();
            VsAppShell.Current.DoIdle();
            EditorShell.Current.DoIdle();
        }
    }
}
