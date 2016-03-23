// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Excel;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Office {
    internal static class ExcelIO {
        public const string CsvName = "RTVS_View.csv";
        private static Application _excel;
        private static int _busy;

        public static async System.Threading.Tasks.Task OpenDataInExcel(string variableName) {
            if(Interlocked.Exchange(ref _busy, 1) > 0) {
                return;
            }

            var workflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            var workflow = workflowProvider.GetOrCreate();
            var session = workflow.RSession;

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var file = Path.Combine(folder, CsvName);
            var rfile = file.Replace('\\', '/');

            if(File.Exists(file)) {
                try {
                    File.Delete(file);
                } catch(IOException) { }
            }

            using (var e = await session.BeginInteractionAsync()) {
                await e.RespondAsync(Invariant($"write.csv({variableName}, file='{rfile}')") + Environment.NewLine);
            }

            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                RunExcel(file);
            });

            Interlocked.Exchange(ref _busy, 0);
        }

        private static Workbook RunExcel(string file) {
            do {
                if (_excel == null) {
                    _excel = CreateExcel();
                }

                if (_excel != null) {
                    try {
                        _excel.Visible = true;
                        _excel.Workbooks.Open(file);
                    } catch (COMException) {
                        _excel = null;
                    }
                }
            } while (_excel == null);

            return null;
        }

        private static Application CreateExcel() {
            Application xlApp = null;
            try {
                xlApp = (Application)Marshal.GetActiveObject("Excel.Application");
            } catch (COMException) { }

            if (xlApp == null) {
                try {
                    xlApp = new Application();
                } catch (COMException) { }
            }
            return xlApp;
        }
    }
}
