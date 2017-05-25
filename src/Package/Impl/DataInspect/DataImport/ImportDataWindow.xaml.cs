// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using Microsoft.VisualStudio.R.Package.Wpf;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataImport
{
    /// <summary>
    /// Interaction logic for ImportDataWindow.xaml
    /// </summary>
    public partial class ImportDataWindow : PlatformDialogWindow {
        private const int MaxPreviewLines = 20;
        private readonly IServiceContainer _services;
        private string _utf8FilePath;

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

        public IDictionary<string, int?> RowNames { get; } = new Dictionary<string, int?> {
            [Package.Resources.AutomaticValue] = null,
            [Package.Resources.ImportData_UseFirstColumn] = 1,
            [Package.Resources.ImportData_UseNumber] = null
        };

        public ImportDataWindow() {
            InitializeComponent();
            PopulateEncodingList();
        }

        public ImportDataWindow(IServiceContainer services, string filePath, string name)
            : this() {
            _services = services;
            SetFilePath(filePath, name);
        }

        private string BuildCommandLine(bool preview) {
            if (string.IsNullOrEmpty(_utf8FilePath)) {
                return null;
            }

            var encoding = GetSelectedString(EncodingComboBox);
            var input = new Dictionary<string, string> {
                ["file"] = _utf8FilePath.ToRPath().ToRStringLiteral(),
                ["header"] = (HeaderCheckBox.IsChecked != null && HeaderCheckBox.IsChecked.Value).ToString().ToUpperInvariant(),
                ["row.names"] = GetSelectedNullableIntValueAsString(RowNamesComboBox),
                ["encoding"] = "UTF-8".ToRStringLiteral(),
                ["sep"] = GetSelectedValue(SeparatorComboBox),
                ["dec"] = GetSelectedValue(DecimalComboBox),
                ["quote"] = GetSelectedValue(QuoteComboBox),
                ["comment.char"] = GetSelectedValue(CommentComboBox),
                ["na.strings"] = string.IsNullOrEmpty(NaStringTextBox.Text) ? null : NaStringTextBox.Text.ToRStringLiteral()
            };

            var inputString = string.Join(", ", input
                .Where(kvp => kvp.Value != null)
                .Select(kvp => Invariant($"{kvp.Key}={kvp.Value}")));

            return preview
                ? Invariant($"read.csv({inputString}, nrows=20)")
                : Invariant($"`{VariableNameBox.Text}` <- read.csv({inputString})");
        }

        private static string GetSelectedValue(ComboBox comboBox) {
            return comboBox.SelectedItem != null ? ((KeyValuePair<string, string>)comboBox.SelectedItem).Value.ToRStringLiteral() : null;
        }

        private static string GetSelectedNullableIntValueAsString(ComboBox comboBox) {
            if (comboBox.SelectedItem == null) {
                return null;
            }

            var val = ((KeyValuePair<string, int?>)comboBox.SelectedItem).Value;
            return val?.ToString(CultureInfo.InvariantCulture) ?? "NULL";
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
            string filePath = _services.FileDialog().ShowOpenFileDialog(Package.Resources.CsvFileFilter);
            if (!string.IsNullOrEmpty(filePath)) {
                SetFilePath(filePath);
            }
        }

        private void SetFilePath(string filePath, string name = null) {
            FilePathBox.Text = filePath;
            FilePathBox.CaretIndex = FilePathBox.Text.Length;
            FilePathBox.ScrollToEnd();

            VariableNameBox.Text = name ?? Path.GetFileNameWithoutExtension(filePath);
            PreviewContentAsync().DoNotWait();
        }

        private async Task PreviewContentAsync() {
            if (string.IsNullOrEmpty(FilePathBox.Text)) {
                return;
            }

            int cp = GetSelectedValueAsInt(EncodingComboBox);
            PreviewFileContent(FilePathBox.Text, cp);
            await ConvertToUtf8(FilePathBox.Text, cp, false, MaxPreviewLines);

            if (!string.IsNullOrEmpty(_utf8FilePath)) {
                var expression = BuildCommandLine(preview: true);
                if (expression != null) {
                    try {
                        var session = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate().RSession;
                        var grid = await session.GetGridDataAsync(expression, null);
                        PopulateDataFramePreview(grid);
                        DataFramePreview.Visibility = Visibility.Visible;
                    } catch (Exception ex) {
                        OnError(ex.Message);
                    }
                }
            }
        }

        private bool Execute(string expression) {
            try {
                var workflow = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
                workflow.Operations.ExecuteExpression(expression);
                return true;
            } catch (Exception ex) {
                OnError(ex.Message);
                return false;
            }
        }

        private void OnError(string errorText) {
            _services.ShowErrorMessage(errorText);
            ProgressBarText.Text = string.Empty;
            ProgressBar.Value = -10;
        }

        private void PopulateEncodingList() {
            var encodings = Encoding.GetEncodings().OrderBy(x => x.DisplayName);
            foreach (var enc in encodings) {
                string item = Invariant($"{enc.DisplayName} (CP {enc.CodePage})");
                Encodings[item] = enc.CodePage;
            }

            EncodingComboBox.ItemsSource = Encodings;
            EncodingComboBox.SelectedIndex = Encodings.IndexWhere(kvp => kvp.Value == Encoding.Default.CodePage).FirstOrDefault();
        }

        private void PopulateDataFramePreview(IGridData<string> gridData) {
            var dg = DataFramePreview;
            dg.Columns.Clear();

            for (long i = 0; i < gridData.ColumnHeader.Range.Count; i++) {
                dg.Columns.Add(new DataGridTextColumn() {
                    Header = gridData.ColumnHeader[gridData.ColumnHeader.Range.Start + i],
                    Binding = new Binding(Invariant($"Values[{i}]")),
                });
            }

            var rows = new List<DataFramePreviewRowItem>();
            for (long r = 0; r < gridData.Grid.Range.Rows.Count; r++) {
                var row = new DataFramePreviewRowItem {
                    RowName = gridData.RowHeader[gridData.RowHeader.Range.Start + r]
                };

                for (long c = 0; c < gridData.Grid.Range.Columns.Count; c++) {
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
            PreviewContentAsync().DoNotWait();
        }

        private void HeaderCheckBox_Changed(object sender, RoutedEventArgs e) {
            PreviewContentAsync().DoNotWait();
        }

        private void PreviewFileContent(string file, int codePage) {
            Encoding encoding = Encoding.GetEncoding(codePage);
            string text = ReadFilePreview(file, encoding);
            InputFilePreview.Text = text;
        }

        private async Task ConvertToUtf8(string file, int codePage, bool reportProgress, int nRows = Int32.MaxValue) {
            try {
                DeleteTempFile();
                await ConvertToUtf8Worker(file, codePage, reportProgress, nRows);
            } catch (IOException ex) {
                OnError(ex.Message);
            } catch (UnauthorizedAccessException ex) {
                OnError(ex.Message);
            }
        }

        private async Task ConvertToUtf8Worker(string file, int codePage, bool reportProgress, int nRows = Int32.MaxValue) {
            await TaskUtilities.SwitchToBackgroundThread();

            Encoding encoding = Encoding.GetEncoding(codePage);
            _utf8FilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(file) + ".utf8");

            int lineCount = 0;
            double progressValue = 0;

            using (var sr = new StreamReader(file, encoding, detectEncodingFromByteOrderMarks: true)) {
                if (reportProgress) {
                    await StartReportProgress(Package.Resources.Converting);
                }

                long read = 0;
                using (var sw = new StreamWriter(_utf8FilePath, append: false, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))) {
                    string line;
                    while (true) {
                        line = sr.ReadLine();
                        lineCount++;
                        if (line == null || lineCount > nRows) {
                            break;
                        }

                        read += line.Length;
                        sw.WriteLine(line);

                        if (reportProgress) {
                            var newProgressValue = 90 * (double)read / sr.BaseStream.Length;
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
                if (!string.IsNullOrEmpty(_utf8FilePath) && File.Exists(_utf8FilePath)) {
                    File.Delete(_utf8FilePath);
                }
            } catch (UnauthorizedAccessException) { }
        }

        private async Task DoDefaultAction() {
            await _services.MainThread().SwitchToAsync();
            RunButton.IsEnabled = CancelButton.IsEnabled = false;
            var result = false;

            try {
                int cp = GetSelectedValueAsInt(EncodingComboBox);

                var nRowsString = NRowsTextBox.Text;
                int nrows = Int32.MaxValue;
                if (!string.IsNullOrWhiteSpace(nRowsString)) {
                    if (!Int32.TryParse(nRowsString, out nrows) || nrows <= 0) {
                        _services.ShowErrorMessage(Package.Resources.ImportData_NRowsError);
                        return;
                    }
                    nrows++; // for possible header
                }

                await ConvertToUtf8(FilePathBox.Text, cp, true, nrows);
                ProgressBarText.Text = Package.Resources.Importing;

                var expression = BuildCommandLine(false);
                if (expression != null) {
                    // TODO: this may take a while and must be cancellable
                    result = Execute(expression);
                }
            } finally {
                if (result) {
                    Close();
                } else {
                    RunButton.IsEnabled = CancelButton.IsEnabled = true;
                }
            }
        }

        private async Task StartReportProgress(string message) {
            await _services.MainThread().SwitchToAsync();
            ProgressBarText.Text = message;
            ProgressBar.Value = 0;
        }

        private async Task ReportProgress(double value) {
            await _services.MainThread().SwitchToAsync();
            ProgressBar.Value = value;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e) {
            DoDefaultAction().DoNotWait();
        }

        private void RunButton_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Space) {
                DoDefaultAction().DoNotWait();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void CancelButton_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Space) {
                Close();
            }
        }
    }
}