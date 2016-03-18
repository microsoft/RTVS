// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal static class SourceFileHelper {
        public static void SourceFiles(IEnumerable<string> files) {
            VsAppShell.Current.AssertIsOnMainThread();

            bool debugging = IsDebugging();
            var operations = GetOperations();
            var list = new List<string>();

            Task.Run(async () => {
                foreach (var f in files) {
                    list.Add(await SessionUtilities.GetRShortenedPathNameAsync(f));
                }
                VsAppShell.Current.DispatchOnUIThread(() => {
                    foreach (var filePath in list) {
                        operations.EnqueueExpression($"{(debugging ? "rtvs::debug_source" : "source")}({filePath.ToRStringLiteral()})", addNewLine: true);
                     }
                });
            });
        }

        public static void SourceFile(string file) {
            VsAppShell.Current.AssertIsOnMainThread();

            bool debugging = IsDebugging();
            var operations = GetOperations();

            Task.Run(async () => {
                file = await SessionUtilities.GetRShortenedPathNameAsync(file);
                VsAppShell.Current.DispatchOnUIThread(() => {
                    operations.ExecuteExpression($"{(debugging ? "rtvs::debug_source" : "source")}({file.ToRStringLiteral()})");
                });
            });
        }

        private static bool IsDebugging() {
            var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger>(typeof(SVsShellDebugger));
            if (debugger != null) {
                var mode = new DBGMODE[1];
                if (debugger.GetMode(mode) >= 0) {
                    return mode[0] != DBGMODE.DBGMODE_Design;
                }
            }
            return false;
        }

        private static IRInteractiveWorkflowOperations GetOperations() {
            var interactiveWorkflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
            return interactiveWorkflow.Operations;
        }
    }
}