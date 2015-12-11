using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Composition;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    /// <summary>
    /// Replacement for VsAppShell in unit tests.
    /// Created via reflection by test code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class TestAppShell : TestEditorShell, IPackageShell {
        public static IApplicationShell Current { get; private set; }

        private IServiceProvider _sp;
        public TestAppShell() {
            CompositionService = TestCompositionCatalog.Current.CompositionService;
            ExportProvider = TestCompositionCatalog.Current.ExportProvider;
            _sp = new TestServiceProvider();

            Current = this;
            EditorShell.SetShell(this);
        }

        #region IApplicationShell
        public override T GetGlobalService<T>(Type type = null) {
            return _sp.GetService(type ?? typeof(T)) as T;
        }
        #endregion

        #region IVsApplicationShell
        public string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
        public string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
        #endregion
    }
}
