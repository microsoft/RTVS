using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.Languages.Editor.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public static class SequentialEditorTestExecutor
    {
        [ExcludeFromCodeCoverage]
        class ExecutionRequest
        {
            public ManualResetEventSlim Event { get; private set; }
            public Action<ManualResetEventSlim> Action { get; private set; }

            public ExecutionRequest(Action<ManualResetEventSlim> action)
            {
                Action = action;
                Event = new ManualResetEventSlim();
            }
        }

        private static IEditorShell _shell;
        private static object _creatorLock = new object();
        private static Action _disposeAction;

        public static void ExecuteTest(Action<ManualResetEventSlim> action, ITestCompositionCatalog catalog = null)
        {
            ExecuteTest(action, null, null, catalog);
        }

        public static void ExecuteTest(Action<ManualResetEventSlim> action, Action initAction, Action disposeAction, ITestCompositionCatalog catalog = null)
        {
            if (catalog == null)
                catalog = EditorTestCompositionCatalog.Current;

            lock (_creatorLock)
            {
                _disposeAction = disposeAction;

                PrepareShell(catalog);

                if (initAction != null)
                {
                    initAction();
                }

                using (var evt = new ManualResetEventSlim())
                {
                    action(evt);
                    evt.Wait();
                }
            }
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            if (_disposeAction != null)
            {
                _disposeAction();
                _disposeAction = null;

                AppDomain.CurrentDomain.DomainUnload -= CurrentDomain_DomainUnload;

                _shell = null;
            }
        }

        private static void PrepareShell(ITestCompositionCatalog catalog)
        {
            lock (_creatorLock)
            {
                if (_shell == null || catalog.ExportProvider != _shell.ExportProvider)
                {
                    AppDomain.CurrentDomain.DomainUnload -= CurrentDomain_DomainUnload;
                    AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

                    _shell = TestEditorShell.Create(catalog);
                    EditorShell.SetShell(_shell);
                }
            }
        }
    }
}
