using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;

namespace Microsoft.Languages.Editor.Tests.Shell {
    [ExcludeFromCodeCoverage]
    public sealed class TestAppShell : IApplicationShell {
        private static Lazy<TestAppShell> _instance = Lazy.Create(() => new TestAppShell());

        public static IApplicationShell Current { get; } = _instance.Value;

        private TestAppShell() {
            CompositionService = RPackageTestCompositionCatalog.Current.CompositionService;
            ExportProvider = RPackageTestCompositionCatalog.Current.ExportProvider;

            var sp = new TestServiceProvider();
            GlobalServiceProvider = sp;
            OleServiceProvider = sp;
        }

        public ICompositionService CompositionService { get; private set; }

        public ExportProvider ExportProvider { get; private set; }

        public System.IServiceProvider GlobalServiceProvider { get; private set; }

        public bool IsTestEnvironment { get; } = true;

        public VisualStudio.OLE.Interop.IServiceProvider OleServiceProvider { get; private set; }

        public void Dispose() {
        }

        public T GetGlobalService<T>(Type type = null) where T : class {
            return GlobalServiceProvider.GetService(type ?? typeof(T)) as T;
        }
    }
}
