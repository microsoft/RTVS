using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using Excel = Microsoft.Office.Interop.Excel;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Office {
    internal sealed class ExcelInterop {
        private static bool _busy;
        private static Application _excel;
        private static Timer _timer;

        class ExcelData {
            public object[] RowNames;
            public object[] ColNames;
            public object[,] CellData;
        }

        public static async Task OpenDataInExcel(string expression, int rows, int cols) {
            if (_busy) {
                return;
            }

            _busy = true;

            ExcelData xlData = await GenerateExcelData(expression, rows, cols);
            try {
                Workbook wb = RunExcel();
                if (wb != null) {
                    // Try finding existing worksheet first
                    Worksheet ws = null;
                    try {
                        ws = (Worksheet)wb.Sheets[expression];
                    }
                    catch (COMException) { }

                    if (ws == null) {
                        ws = (Worksheet)wb.Sheets.Add();
                    }

                    PopulateWorksheet(ws, xlData.RowNames, xlData.ColNames, xlData.CellData);
                    _excel.Visible = true;
                }
            } finally {
                _busy = false;
            }
        }

        private static async Task<ExcelData> GenerateExcelData(string expression, int rows, int cols) {
            ExcelData xlData = new ExcelData();

            IGridData<string> data = await GridDataSource.GetGridDataAsync(expression,
                                     new GridRange(new Range(0, rows), new Range(0, cols)));
            if (data.RowHeader != null) {
                xlData.RowNames = await MakeRowNames(data, rows);
            }

            if (data.RowHeader != null) {
                xlData.ColNames = await MakeColumnNames(data, cols);
            }

            xlData.CellData = await MakeExcelRange(data, rows, cols);
            return xlData;
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

        private static Workbook RunExcel() {
            int retries = 0;
            do {
                if (_excel == null) {
                    _excel = new Application();

                    _timer = new Timer(10 * 60 * 1000); // 10 minutes
                    _timer.AutoReset = true;
                    _timer.Elapsed += OnTimer;
                    _timer.Start();
                }

                try {
                    return _excel.Workbooks.Add();
                } catch (COMException) {
                    _excel = null;
                    retries++;
                }
            } while (_excel == null && retries < 5);

            return null;
        }

        private static void OnTimer(object sender, ElapsedEventArgs e) {
            if(_excel != null && !_excel.Visible) {
                _excel.Quit();
                _excel = null;

                _timer.Stop();
                _timer.Dispose();
            }
        }

        private static Task<object[]> MakeRowNames(IGridData<string> data, int rows) {
            return Task.Run(() => {
                object[] arr = new object[rows];
                for (int r = 0; r < rows; r++) {
                    arr[r] = data.RowHeader[r];
                }
                return arr;
            });
        }

        private static Task<object[]> MakeColumnNames(IGridData<string> data, int cols) {
            return Task.Run(() => {
                object[] arr = new object[cols];
                for (int c = 0; c < cols; c++) {
                    arr[c] = data.ColumnHeader[c];
                }
                return arr;
            });
        }

        private static Task<object[,]> MakeExcelRange(IGridData<string> data, int rows, int cols) {
            return Task.Run(() => {
                object[,] arr = new object[rows, cols];
                for (int r = 0; r < rows; r++) {
                    for (int c = 0; c < cols; c++) {
                        arr[r, c] = data.Grid[r, c];
                    }
                }
                return arr;
            });
        }
    }
}
