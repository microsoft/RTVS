// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Repl.Commands {
    internal static class SourceFileHelper {
        public static void SourceFile(string filePath) {
            var debugger = VsAppShell.Current.GetGlobalService<IVsDebugger>(typeof(SVsShellDebugger));
            if (debugger != null) {
                var mode = new DBGMODE[1];
                if (debugger.GetMode(mode) >= 0) {
                    bool debugging = mode[0] != DBGMODE.DBGMODE_Design;
                    var interactiveWorkflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
                    var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
                    var operations = interactiveWorkflow.Operations;

                    // TODO: need make it not to wait.
                    filePath = SessionUtilities.GetRShortenedPathNameAsync(filePath).Result;
                    operations.ExecuteExpression($"{(debugging ? "rtvs::debug_source" : "source")}({filePath.ToRStringLiteral()})");
                }
            }
        }
    }
}