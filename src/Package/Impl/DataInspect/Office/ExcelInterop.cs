#if _NOT_
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Office.Interop.Excel;
using Microsoft.R.Actions.Logging;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using Excel = Microsoft.Office.Interop.Excel;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Office {
    internal sealed class ExcelData {
        public object[] RowNames;
        public object[] ColNames;
        public object[,] CellData;
    }

    internal static class ExcelInterop {
        public const string WorkbookName = "RTVS_View.xlsx";
        private static Application _excel;

        public static void OpenDataInExcel(string variableName, string expression, int rows, int cols) {
            ExcelData xlData = GenerateExcelData(expression, Math.Min(10000, rows), Math.Min(10000, cols));
            if (xlData != null) {
                VsAppShell.Current.DispatchOnUIThread(() => {
                    Workbook wb = RunExcel();
                    if (wb != null) {
                        // Try finding existing worksheet first
                        Worksheet ws = GetOrCreateWorksheet(wb, variableName);
                        try {
                            PopulateWorksheet(ws, xlData.RowNames, xlData.ColNames, xlData.CellData);
                            _excel.Visible = true;
                        }
                        catch(COMException) { }
                    }
                });
            }
        }

        public static ExcelData GenerateExcelData(string expression, int rows, int cols) {
            if (rows <= 0 || cols <= 0) {
                return null;
            }

            try {
                ExcelData xlData = new ExcelData();
                xlData.CellData = new object[rows, cols];
                int chunkSize = 1000;

                int steps = (rows + chunkSize - 1) / chunkSize;
                List<LongAction> actions = new List<LongAction>();

                for (int i = 0; i < steps; i++) {
                    actions.Add(
                        new LongAction() {
                            Data = i,
                            Name = Resources.Progress_PreparingExcelData,
                            Action = (o) => FetchChunk(expression, ((int)o) * chunkSize, chunkSize, xlData, rows, cols)
                        }
                    );
                }

                if (LongOperationNotification.ShowWaitingPopup(Resources.Progress_PreparingExcelData, actions)) {
                    if (xlData.CellData[0, 0] == null) {
                        return null;
                    }
                    return xlData;
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                VsAppShell.Current.ShowErrorMessage(Resources.Error_ExcelCannotEvaluateExpression);
                GeneralLog.Write(ex);
            }
            return null;
        }

        private static void FetchChunk(string expression, int start, int chunkSize, ExcelData xlData, int totalRows, int totalCols) {
            IGridData<string> data =
                GridDataSource.GetGridDataAsync(expression,
                                new GridRange(new Range(start, Math.Min(chunkSize, totalRows - start)),
                                new Range(0, totalCols))).Result;

            if (data != null) {
                if (data.RowHeader != null) {
                    if (xlData.RowNames == null) {
                        xlData.RowNames = new object[totalRows];
                    }
                    for (int r = 0; r < chunkSize && r < totalRows - start; r++) {
                        xlData.RowNames[r + start] = data.RowHeader[r + start];
                    }
                }

                if (data.ColumnHeader != null && xlData.ColNames == null) {
                    xlData.ColNames = new object[totalCols];
                    for (int c = 0; c < totalCols; c++) {
                        xlData.ColNames[c] = data.ColumnHeader[c];
                    }
                }

                for (int r = 0; r < chunkSize && r < totalRows - start; r++) {
                    for (int c = 0; c < totalCols; c++) {
                        xlData.CellData[r + start, c] = data.Grid[r + start, c];
                    }
                }
            }
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
                wb = _excel.Workbooks[WorkbookName];
            } catch (COMException) { }

            if (wb == null) {
                wb = _excel.Workbooks.Add();
                string wbName = null;
                try {
                    string myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    wbName = Path.Combine(myDocPath, WorkbookName);
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

        internal static void PopulateWorksheet(Worksheet ws, object[] rowNames, object[] colNames, object[,] cellData) {
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
    }
}
#endif