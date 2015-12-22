using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class FunctionIndexTestExecutor {
        private static object _lockObject = new object();
        private static bool _closed = true;

        public static void ExecuteTest(Action<ManualResetEventSlim> action) {
            SequentialEditorTestExecutor.ExecuteTest(action, InitFunctionIndex, DisposeFunctionIndex);
        }

        private static void InitFunctionIndex() {
            lock (_lockObject) {
                if (_closed) {
                    RToolsSettings.Current = new TestRToolsSettings();

                    FunctionIndex.Initialize();
                    FunctionIndex.BuildIndexAsync().Wait();
                }
            }
        }

        private static void DisposeFunctionIndex() {
            lock (_lockObject) {
                IRSessionProvider sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
                if (sessionProvider != null) {
                    foreach (var s in sessionProvider.GetSessions()) {
                        if (s.Value.IsHostRunning) {
                            s.Value.StopHostAsync().Wait();
                        }
                    }
                }

                FunctionIndex.Terminate();
                _closed = true;
            }
        }
    }
}
