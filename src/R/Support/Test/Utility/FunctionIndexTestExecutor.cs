using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public static class FunctionIndexTestExecutor
    {
        private static object _lockObject = new object();
        private static bool _closed = true;

        public static void ExecuteTest(Action<ManualResetEventSlim> action)
        {
            SequentialEditorTestExecutor.ExecuteTest(action, InitFunctionIndex, DisposeFunctionIndex);
        }

        private static void InitFunctionIndex()
        {
            lock (_lockObject)
            {
                if (_closed)
                {
                    RToolsSettings.ToolsSettings = new TestRToolsSettings();

                    FunctionIndex.Initialize();
                    FunctionIndex.BuildIndexAsync().Wait();
                }
            }
        }

        private static void DisposeFunctionIndex()
        {
            lock (_lockObject)
            {
                FunctionIndex.Terminate();
                _closed = true;
            }
        }
    }
}
