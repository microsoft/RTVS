using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    /// <summary>
    /// Replacement for VsAppShell in unit tests.
    /// Created via reflection by test code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed class TestAppShell : TestEditorShell, IApplicationShell {
        private IServiceProvider _sp;
        private static TestAppShell _instance;

        private TestAppShell() {
            CompositionService = TestCompositionCatalog.Current.CompositionService;
            ExportProvider = TestCompositionCatalog.Current.ExportProvider;
            _sp = new TestServiceProvider();
        }

        public static void Create() {
            _instance = new TestAppShell();
            VsAppShell.Current = _instance;
            RToolsSettings.Current = new TestRToolsSettings();
        }

        #region IApplicationShell
        public override T GetGlobalService<T>(Type type = null) {
            return _sp.GetService(type ?? typeof(T)) as T;
        }
        #endregion

        #region IPackageShell
        public string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
        public string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
        #endregion
    }
}
