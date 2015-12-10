using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    [ExcludeFromCodeCoverage]
    public sealed class TestAppShell : TestEditorShell, IVsApplicationShell {
        private static Lazy<TestAppShell> _instance = Lazy.Create(() => new TestAppShell());
        public static IVsApplicationShell Current => _instance.Value;

        private IServiceProvider _sp;
        private TestAppShell() {
            CompositionService = RPackageTestCompositionCatalog.Current.CompositionService;
            ExportProvider = RPackageTestCompositionCatalog.Current.ExportProvider;
            _sp = new TestServiceProvider();

            EditorShell.SetShell(this);
            AppShell.SetShell(this);
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
