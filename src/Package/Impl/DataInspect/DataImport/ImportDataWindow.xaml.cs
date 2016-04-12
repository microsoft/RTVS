// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataImport {
    /// <summary>
    /// Interaction logic for ImportDataWindow.xaml
    /// </summary>
    public partial class ImportDataWindow : DialogWindow {
        private const int MaxPreviewLines = 20;
        private string _tempFilePath;

        public IDictionary<string, string> Separators { get; } = new Dictionary<string, string> {
            [Package.Resources.ImportData_Whitespace] = "",
            [Package.Resources.ImportData_Comma] = ",",
            [Package.Resources.ImportData_Semicolon] = ";",
            [Package.Resources.ImportData_Tab] = "\t"
        };

        public IDictionary<string, string> Decimals { get; } = new Dictionary<string, string> {
            [Package.Resources.ImportData_Period] = ".",
            [Package.Resources.ImportData_Comma] = ","
        };

        public IDictionary<string, string> Quotes { get; } = new Dictionary<string, string> {
            [Package.Resources.ImportData_DoubleQuote] = "\"",
            [Package.Resources.ImportData_SingleQuote] = "'",
            [Package.Resources.ImportData_None] = ""
        };

        public IDictionary<string, string> Comments { get; } = new Dictionary<string, string> {
            [Package.Resources.ImportData_None] = "",
            ["#"] = "#",
            ["!"] = "!",
            ["%"] = "%",
            ["@"] = "@",
            ["/"] = "/",
            ["~"] = "~"
        };

        public IDictionary<string, int> Encodings { get; } = new Dictionary<string, int>();

        public IDictionary<string, string> RowNames { get; } = new Dictionary<string, string> {
            [Package.Resources.AutomaticValue] = null,
            [Package.Resources.ImportData_UseFirstColumn] = "1",
            [Package.Resources.ImportData_UseNumber] = null
        };

        public ImportDataWindow() {
            InitializeComponent();
            PopulateEncodingList();
        }

        public ImportDataWindow(string filePath, string name)
            : this() {
            SetFilePath(filePath, name);
        }

        private IRSession GetRSession() {
            IRSession rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().RSession;
            if (rSession == null) {
                throw new InvalidOperationException(Invariant($"{nameof(IRSessionProvider)} failed to return RSession for importing data set"));
            }
            return rSession;
        }

        private string BuildCommandLine(bool preview) {
            if (string.IsNullOrEmpty(_tempFilePath)) {
                return null;
            }

            var encoding = GetSelectedString(EncodingComboBox);
            var input = new Dictionary<string, string> {
                ["file"] = _tempFilePath.ToRPath().ToRStringLiteral(),
                ["header"] = (HeaderCheckBox.IsChecked != null && HeaderCheckBox.IsChecked.Value).ToString().ToUpperInvariant(),
                ["row.names"] = GetSelectedValue(RowNamesComboBox),
                ["encoding"] = "UTF-8".ToRStringLiteral(),
                ["sep"] = GetSelectedValue(SeparatorComboBox),
                ["dec"] = GetSelectedValue(DecimalComboBox),
                ["quote"] = GetSelectedValue(QuoteComboBox),
                ["comment.char"] = GetSelectedValue(CommentComboBox),
                ["na.strings"] = string.IsNullOrEmpty(NaStringTextBox.Text) ? null : NaStringTextBox.Text.ToRStringLiteral()
            };

            var inputString = string.Join(", ", input
                .Where(kvp => kvp.Value != null)
                .Select(kvp => $"{kvp.Key}={kvp.Value}"));

            return preview
                ? Invariant($"read.csv({inputString}, nrows=20)")
                : Invariant($"`{VariableNameBox.Text}` <- read.csv({inputString})");
        }

        private static string GetSelectedValue(ComboBox comboBox) {
            return comboBox.SelectedItem != null ? ((KeyValuePair<string, string>)comboBox.SelectedItem).Value.ToRStringLiteral() : null;
        }

        private static int GetSelectedValueAsInt(ComboBox comboBox) {
            return comboBox.SelectedItem != null ? ((KeyValuePair<string, int>)comboBox.SelectedItem).Value : -1;
        }

        private static string GetSelectedString(ComboBox comboBox) {
            var s = comboBox.SelectedItem as string;
            if (s != null && s.EqualsOrdinal(Package.Resources.AutomaticValue)) {
                s = null;
            }
            return s;
        }

        private void FileOpenButton_Click(object sender, RoutedEventArgs e) {
            string filePath = VsAppShell.Current.BrowseForFileOpen(IntPtr.Zero, Package.Resources.CsvFileFilter);
            if (!string.IsNullOrEmpty(filePath)) {
                SetFilePath(filePath);
            }
        }

        private void SetFilePath(string filePath, string name = null) {
            FilePathBox.Text = filePath;
            FilePathBox.CaretIndex = FilePathBox.Text.Length;
            FilePathBox.ScrollToEnd();

            VariableNameBox.Text = name ?? Path.GetFileNameWithoutExtension(filePath);
            PreviewContent();
        }

        private async void PreviewContent() {
            if (string.IsNullOrEmpty(FilePathBox.Text)) {
                return;
            }

            int cp = GetSelectedValueAsInt(EncodingComboBox);
            PreviewFileContent(FilePathBox.Text, cp);
            await ConvertToUtf8(FilePathBox.Text, cp, MaxPreviewLines);

            if (!string.IsNullOrEmpty(_tempFilePath)) {
                var expression = BuildCommandLine(preview: true);
                if (expression != null) {
                    try {
                        var grid = await GridDataSource.GetGridDataAsync(expression, null);

                        PopulateDataFramePreview(grid);
                        ErrorBlock.Visibility = Visibility.Collapsed;
                        DataFramePreview.Visibility = Visibility.Visible;
                    } catch (Exception ex) {
                        OnError(ex.Message);
                    }
                }
            }
        }

        private async Task RunAsync(string expression) {
            try {
                REvaluationResult result = await EvaluateAsync(expression);
                if (result.ParseStatus == RParseStatus.OK && result.Error == null) {
                    Close();
                } else {
                    OnError(result.ToString());
                }
            } catch (Exception ex) {
                OnError(ex.Message);
            }
        }

        private void OnError(string errorText) {
            ErrorText.Text = errorText;
            ErrorBlock.Visibility = Visibility.Visible;
            DataFramePreview.Visibility = Visibility.Collapsed;
            ProgressBarText.Text = string.Empty;
            ProgressBar.Value = -10;
        }

        private async Task<REvaluationResult> EvaluateAsync(string expression) {
            await TaskUtilities.SwitchToBackgroundThread();

            IRSession rSession = GetRSession();
            REvaluationResult result;
            using (var evaluator = await rSession.BeginEvaluationAsync()) {
                result = await evaluator.EvaluateAsync(expression, REvaluationKind.Mutating);
            }

            return result;
        }

        private void PopulateEncodingList() {
            var encodings = Encoding.GetEncodings().OrderBy(x => x.DisplayName);
            foreach (var enc in encodings) {
                string item = Invariant($"{enc.DisplayName} (CP {enc.CodePage})");
                Encodings[item] = enc.CodePage;
            }

            EncodingComboBox.ItemsSource = Encodings;
            EncodingComboBox.SelectedIndex = Encodings.IndexWhere((kvp) => kvp.Value == Encoding.Default.CodePage).FirstOrDefault();
        }

        private void PopulateDataFramePreview(IGridData<string> gridData) {
            var dg = DataFramePreview;
            dg.Columns.Clear();

            for (int i = 0; i < gridData.ColumnHeader.Range.Count; i++) {
                dg.Columns.Add(new DataGridTextColumn() {
                    Header = gridData.ColumnHeader[gridData.ColumnHeader.Range.Start + i],
                    Binding = new Binding(Invariant($"Values[{i}]")),
                });
            }

            var rows = new List<DataFramePreviewRowItem>();
            for (var r = 0; r < gridData.Grid.Range.Rows.Count; r++) {
                var row = new DataFramePreviewRowItem {
                    RowName = gridData.RowHeader[gridData.RowHeader.Range.Start + r]
                };

                for (int c = 0; c < gridData.Grid.Range.Columns.Count; c++) {
                    row.Values.Add(gridData.Grid[gridData.Grid.Range.Rows.Start + r, gridData.Grid.Range.Columns.Start + c].ToUnicodeQuotes());
                }

                rows.Add(row);
            }
            dg.ItemsSource = rows;
        }

        private string ReadFilePreview(string filePath, Encoding enc) {
            StringBuilder sb = new StringBuilder();
            using (var sr = new StreamReader(filePath, enc, detectEncodingFromByteOrderMarks: true)) {
                int readCount = 0;
                while (readCount < MaxPreviewLines) {
                    string read = sr.ReadLine();
                    if (read == null) {
                        break;
                    }
                    sb.AppendLine(read);
                    readCount++;
                }
            }
            return sb.ToString();
        }

        class DataFramePreviewRowItem {
            public string RowName { get; set; }
            public List<string> Values { get; } = new List<string>();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            PreviewContent();
        }

        private void HeaderCheckBox_Changed(object sender, RoutedEventArgs e) {
            PreviewContent();
        }

        protected override void OnClosed(EventArgs e) {
            DeleteTempFile();
            base.OnClosed(e);
        }

        private void PreviewFileContent(string file, int codePage) {
            Encoding encoding = Encoding.GetEncoding(codePage);
            string text = ReadFilePreview(file, encoding);
            InputFilePreview.Text = text;
        }

        private async Task ConvertToUtf8(string file, int codePage, int maxLines = Int32.MaxValue) {
            try {
                DeleteTempFile();
                await ConvertToUtf8Worker(file, codePage, maxLines);
            } catch (IOException ex) {
                OnError(ex.Message);
            } catch (UnauthorizedAccessException ex) {
                OnError(ex.Message);
            }
        }

        private async Task ConvertToUtf8Worker(string file, int codePage, int maxLines = Int32.MaxValue) {
            await TaskUtilities.SwitchToBackgroundThread();

            Encoding encoding = Encoding.GetEncoding(codePage);
            _tempFilePath = Path.GetTempFileName();

            int lineCount = 0;
            double progressValue = 0;
            bool reportProgress = maxLines == Int32.MaxValue;

            using (var sr = new StreamReader(file, encoding, detectEncodingFromByteOrderMarks: true)) {
                if (reportProgress) {
                    await StartReportProgress(Package.Resources.Converting);
                }
                long read = 0;
                using (var sw = new StreamWriter(_tempFilePath, append: false, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))) {
                    string line;
                    while (true) {
                        line = sr.ReadLine();
                        lineCount++;
                        if (line == null || lineCount >= maxLines) {
                            break;
                        }

                        read += line.Length;
                        sw.WriteLine(line);

                        if (reportProgress) {
                            var newProgressValue = 90 * (double)read / (double)sr.BaseStream.Length;
                            if (newProgressValue - progressValue >= 10) {
                                progressValue = newProgressValue;
                                await ReportProgress(progressValue);
                            }
                        }
                    }
                }
            }
            if (reportProgress) {
                await ReportProgress(90);
            }
        }

        private void DeleteTempFile() {
            try {
                if (!string.IsNullOrEmpty(_tempFilePath) && File.Exists(_tempFilePath)) {
                    File.Delete(_tempFilePath);
                }
            } catch (UnauthorizedAccessException) { }
        }

        private async Task DoDefaultAction() {
            await VsAppShell.Current.DispatchOnMainThreadAsync(async () => {
                RunButton.IsEnabled = CancelButton.IsEnabled = false;

                try {
                    int cp = GetSelectedValueAsInt(EncodingComboBox);
                    await ConvertToUtf8(FilePathBox.Text, cp);
                    await SetProgressMessage(Package.Resources.Importing);

                    var expression = BuildCommandLine(false);
                    if (expression != null) {
                        // TODO: this may take a while and must be cancellable
                        await RunAsync(expression);
                    }
                } finally {
                    RunButton.IsEnabled = CancelButton.IsEnabled = true;
                }
            });
        }

        private async Task StartReportProgress(string message) {
            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                ProgressBarText.Text = message;
                ProgressBar.Value = 0;
            });
        }

        private async Task ReportProgress(double value) {
            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                ProgressBar.Value = value;
            });
        }

        private async Task SetProgressMessage(string message) {
            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                ProgressBarText.Text = message;
            });
        }

        private void RunButton_Click(object sender, RoutedEventArgs e) {
            DoDefaultAction().DoNotWait();
        }

        private void RunButton_PreviewKeyUp(object sender, KeyEventArgs e) {
            DoDefaultAction().DoNotWait();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void CancelButton_PreviewKeyUp(object sender, KeyEventArgs e) {
            Close();
        }
    }
}
