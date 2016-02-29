// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using Microsoft.VisualStudio.R.Package.Shell;
using Excel = Microsoft.Office.Interop.Excel;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Office {
    internal static class ExcelInterop {
        private const string _workbookName = "RTVS_View.xlsx";
        private static bool _busy;
        private static Application _excel;

        class ExcelData {
            public object[] RowNames;
            public object[] ColNames;
            public object[,] CellData;
        }

        public static async Task OpenDataInExcel(string variableName, string expression, int rows, int cols) {
            if (_busy) {
                return;
            }

            _busy = true;

            try {
                await TaskUtilities.SwitchToBackgroundThread();

                ExcelData xlData = await GenerateExcelData(expression, rows, cols);
                if (xlData != null) {

                    VsAppShell.Current.DispatchOnUIThread(() => {
                        Workbook wb = RunExcel();
                        if (wb != null) {
                            // Try finding existing worksheet first
                            Worksheet ws = GetOrCreateWorksheet(wb, variableName);
                            PopulateWorksheet(ws, xlData.RowNames, xlData.ColNames, xlData.CellData);
                            _excel.Visible = true;
                        }
                    });
                }
            } finally {
                _busy = false;
            }
        }

        private static async Task<ExcelData> GenerateExcelData(string expression, int rows, int cols) {

            try {
                ExcelData xlData = new ExcelData();
                IGridData<string> data = await GridDataSource.GetGridDataAsync(expression,
                                         new GridRange(new Range(0, rows), new Range(0, cols)));
                if (data != null) {
                    if (data.RowHeader != null) {
                        xlData.RowNames = MakeRowNames(data, rows);
                    }

                    if (data.RowHeader != null) {
                        xlData.ColNames = MakeColumnNames(data, cols);
                    }

                    xlData.CellData = MakeExcelRange(data, rows, cols);
                    return xlData;
                }
            } catch (OperationCanceledException) { }

            return null;
        }

        private static Workbook RunExcel() {
            do {
                if (_excel == null) {
                    _excel = CreateExcel();
                }

                if (_excel != null) {
                    try {
                        return GetOrCreateWorkbook();
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

        private static Workbook GetOrCreateWorkbook() {
            Workbook wb = null;
            try {
                wb = _excel.Workbooks[_workbookName];
            } catch (COMException) { }

            if (wb == null) {
                wb = _excel.Workbooks.Add();
                string wbName = null;
                try {
                    string myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    wbName = Path.Combine(myDocPath, _workbookName);
                    if (File.Exists(wbName)) {
                        File.Delete(wbName);
                    }
                } catch (IOException) { }

                try {
                    if (wbName != null) {
                        wb.SaveAs(wbName);
                    }
                } catch (COMException) { }
            }

            return wb;
        }

        private static Worksheet GetOrCreateWorksheet(Workbook wb, string name) {
            Worksheet ws = null;
            try {
                ws = (Worksheet)wb.Sheets[name];
            } catch (COMException) { }

            if (ws == null) {
                ws = (Worksheet)wb.Sheets.Add();
                ws.Name = name;
            }
            return ws;
        }

        private static void PopulateWorksheet(Worksheet ws, object[] rowNames, object[] colNames, object[,] cellData) {
            int startRow = colNames != null ? 2 : 1;
            int startCol = rowNames != null ? 2 : 1;
            Excel.Range c1, c2;

            int rows = rowNames.Length;
            int cols = colNames.Length;

            c1 = (Excel.Range)ws.Cells[startRow, startCol];
            c2 = (Excel.Range)ws.Cells[rows + startRow - 1, cols + startCol - 1];
            Excel.Range dataRange = ws.get_Range(c1, c2);
            dataRange.Value = cellData;

            if (rowNames != null) {
                for (int r = 0; r < rows; r++) {
                    ws.Cells[r + startRow, 1] = rowNames[r];
                }

                c1 = (Excel.Range)ws.Cells[startRow, 1];
                c2 = (Excel.Range)ws.Cells[rows + startRow - 1, 1];
                Excel.Range rowNamesRange = ws.get_Range(c1, c2);
                rowNamesRange.Columns.AutoFit();
            }

            if (colNames != null) {
                c1 = (Excel.Range)ws.Cells[1, startCol];
                c2 = (Excel.Range)ws.Cells[1, cols + startCol - 1];
                Excel.Range colRange = ws.get_Range(c1, c2);
                colRange.Value = colNames;
                colRange.HorizontalAlignment = XlHAlign.xlHAlignRight;
            }
        }

        private static object[] MakeRowNames(IGridData<string> data, int rows) {
            object[] arr = new object[rows];
            for (int r = 0; r < rows; r++) {
                arr[r] = data.RowHeader[r];
            }
            return arr;
        }

        private static object[] MakeColumnNames(IGridData<string> data, int cols) {
            object[] arr = new object[cols];
            for (int c = 0; c < cols; c++) {
                arr[c] = data.ColumnHeader[c];
            }
            return arr;
        }

        private static object[,] MakeExcelRange(IGridData<string> data, int rows, int cols) {
            object[,] arr = new object[rows, cols];
            for (int r = 0; r < rows; r++) {
                for (int c = 0; c < cols; c++) {
                    arr[r, c] = data.Grid[r, c];
                }
            }
            return arr;
        }
    }
}
