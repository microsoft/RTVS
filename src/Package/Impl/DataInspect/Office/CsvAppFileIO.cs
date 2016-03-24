// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using static System.FormattableString;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Office {
    internal static class CsvAppFileIO {
        private static int _busy;

        public static async Task OpenDataCsvApp(DebugEvaluationResult result) {
            if (Interlocked.Exchange(ref _busy, 1) > 0) {
                return;
            }

            var workflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var workflow = workflowProvider.GetOrCreate();
            var session = workflow.RSession;

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folder = Path.Combine(folder, @"RTVS_CSV_Exports\");
            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            var variableName = result.Name ?? "variable";
            var csvFileName = MakeCsvFileName(variableName);
            var file = ProjectUtilities.GetUniqueFileName(folder, csvFileName, "csv", appendUnderscore: true);
            var rfile = file.Replace('\\', '/');

            string currentStatusText = string.Empty;
            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                var statusBar = VsAppShell.Current.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
                statusBar.GetText(out currentStatusText);
            });

            try {
                await SetStatusTextAsync(Resources.Status_WritingCSV);
                using (var e = await session.BeginEvaluationAsync()) {
                    await e.EvaluateAsync(Invariant($"write.csv({result.Expression}, file='{rfile}')") + Environment.NewLine);
                }

                if (File.Exists(file)) {
                    Task.Run(() => {
                        try {
                            Process.Start(file);
                        } catch (Win32Exception ex) {
                            ShowErrorMessage(ex.Message);
                        } catch (FileNotFoundException ex) {
                            ShowErrorMessage(ex.Message);
                        }
                    }).DoNotWait();
                }
            } finally {
                await SetStatusTextAsync(currentStatusText);
            }

            Interlocked.Exchange(ref _busy, 0);
        }

        private static void ShowErrorMessage(string message) {
            VsAppShell.Current.ShowErrorMessage(
                string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotOpenCsv, message));
        }

        private static async Task SetStatusTextAsync(string text) {
            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                var statusBar = VsAppShell.Current.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
                statusBar.SetText(text);
            });
        }

        private static string MakeCsvFileName(string variableName) {
            var project = ProjectUtilities.GetActiveProject();
            var projectName = project?.FileName;

            var contentTypeService = VsAppShell.Current.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
            var viewTracker = VsAppShell.Current.ExportProvider.GetExportedValue<IActiveWpfTextViewTracker>();

            var activeView = viewTracker.GetLastActiveTextView(contentTypeService.GetContentType(RContentTypeDefinition.ContentType));
            var filePath = activeView.GetFilePath();

            var csvFileName = string.Empty;

            if (!string.IsNullOrEmpty(projectName)) {
                csvFileName += Path.GetFileNameWithoutExtension(projectName);
                csvFileName += "_";
            }

            if (!string.IsNullOrEmpty(filePath)) {
                csvFileName += Path.GetFileNameWithoutExtension(filePath);
                csvFileName += "_";
            }

            if(variableName.StartsWith("$", StringComparison.Ordinal)) {
                variableName = variableName.Substring(1);
            }
            variableName = variableName.Replace('.', '_').Replace('@', '_').Replace('$', '_');
            if(variableName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
                variableName = "expression";
            }
            csvFileName += variableName;

            return csvFileName;
        }
    }
}
