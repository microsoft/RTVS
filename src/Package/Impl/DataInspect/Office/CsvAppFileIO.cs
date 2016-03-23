// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Office {
    internal static class CsvAppFileIO {
        private static int _busy;

        public static async Task OpenDataCsvApp(string variableName, string csvFileName) {
            if (Interlocked.Exchange(ref _busy, 1) > 0) {
                return;
            }

            var workflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var workflow = workflowProvider.GetOrCreate();
            var session = workflow.RSession;

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folder = Path.Combine(folder, @"RTVS_CSV_Exports\");
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            var file = ProjectUtilities.GetUniqueFileName(folder, csvFileName, "csv", appendUnderscore: true);
            var rfile = file.Replace('\\', '/');

            string currentStatusText = string.Empty;
            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                var statusBar = VsAppShell.Current.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
                statusBar.GetText(out currentStatusText);
            });

            try {
                await SetStatusTextAsync(Resources.Status_WritingCSV);
                using (var e = await session.BeginInteractionAsync()) {
                    await e.RespondAsync(Invariant($"write.csv({variableName}, file='{rfile}')") + Environment.NewLine);
                }

                Task.Run(() => Process.Start(file)).DoNotWait();
            } finally {
                await SetStatusTextAsync(currentStatusText);
            }

            Interlocked.Exchange(ref _busy, 0);
        }

        private static async Task SetStatusTextAsync(string text) {
            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                var statusBar = VsAppShell.Current.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
                statusBar.SetText(text);
            });
        }
    }
}
