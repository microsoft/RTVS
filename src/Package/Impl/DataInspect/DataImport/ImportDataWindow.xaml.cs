// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataImport {
    /// <summary>
    /// Interaction logic for ImportDataWindow.xaml
    /// </summary>
    public partial class ImportDataWindow : DialogWindow {
        private ObservableCollection<ComboBoxItemModel> Separators = new ObservableCollection<ComboBoxItemModel>() { new ComboBoxItemModel("White space", ""), new ComboBoxItemModel("Comma (,)", ","), new ComboBoxItemModel("Semicolon (;)", ";"), new ComboBoxItemModel(@"Tab (\t)", "\t") };
        private ObservableCollection<ComboBoxItemModel> Decimals = new ObservableCollection<ComboBoxItemModel>() { new ComboBoxItemModel("Period (.)", "."), new ComboBoxItemModel("Comma (,)", ",") };
        private ObservableCollection<ComboBoxItemModel> Quotes = new ObservableCollection<ComboBoxItemModel>() { new ComboBoxItemModel("Double quote (\")", "\""), new ComboBoxItemModel("Single quote (')", "'"), new ComboBoxItemModel("None", "") };
        private ObservableCollection<ComboBoxItemModel> Comments = new ObservableCollection<ComboBoxItemModel>() { new ComboBoxItemModel("None", ""), new ComboBoxItemModel("#", "#"), new ComboBoxItemModel("!", "!"), new ComboBoxItemModel("%", "%"), new ComboBoxItemModel("@", "@"), new ComboBoxItemModel("/", "/"), new ComboBoxItemModel("~", "~") };
        private ObservableCollection<ComboBoxItemModel> RowNames = new ObservableCollection<ComboBoxItemModel>() { new ComboBoxItemModel("Automatic", null), new ComboBoxItemModel("Use first column", "1"), new ComboBoxItemModel("Use number", "NULL") };

        public ImportDataWindow() {
            InitializeComponent();

            RowNamesComboBox.ItemsSource = RowNames;
            RowNamesComboBox.SelectedIndex = 0;

            SeparatorComboBox.ItemsSource = Separators;
            SeparatorComboBox.SelectedIndex = 1;

            DecimalComboBox.ItemsSource = Decimals;
            DecimalComboBox.SelectedIndex = 0;

            QuoteComboBox.ItemsSource = Quotes;
            QuoteComboBox.SelectedIndex = 0;

            CommentComboBox.ItemsSource = Comments;
            CommentComboBox.SelectedIndex = 0;

            SetEncodingComboBoxAsync().DoNotWait();
        }

        public ImportDataWindow(string filePath) : this() {
            SetFilePathAsync(filePath).DoNotWait();
        }

        private IRSession GetRSession() {
            IRSession rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().RSession;
            if (rSession == null) {
                throw new InvalidOperationException(Invariant($"{nameof(IRSessionProvider)} failed to return RSession for importing data set"));
            }
            return rSession;
        }

        private string BuildCommandLine(bool preview) {
            if (string.IsNullOrEmpty(FilePathBox.Text)) {
                return null;
            }

            var parameter = new StringBuilder();

            parameter.Append("file=");
            parameter.Append(FilePathBox.Text.ToRStringLiteral());

            parameter.AppendFormat(" ,header={0}", HeaderCheckBox.IsChecked.Value.ToString().ToUpperInvariant());

            var model = (ComboBoxItemModel)RowNamesComboBox.SelectedItem;
            if (model != null && model.Value != null) {
                parameter.AppendFormat(" ,row.names={0}", model.Value);
            }

            var encodingModel = (ComboBoxItemModel)EncodingComboBox.SelectedItem;
            if (encodingModel != null && model.Value != null) {
                parameter.AppendFormat(" ,encoding={0}", model.Value.ToRStringLiteral());
            }

            parameter.AppendFormat(" ,sep={0}", ComboBoxSelectedRStringLiteral(SeparatorComboBox));
            parameter.AppendFormat(" ,dec={0}", ComboBoxSelectedRStringLiteral(DecimalComboBox));
            parameter.AppendFormat(" ,quote={0}", ComboBoxSelectedRStringLiteral(QuoteComboBox));
            parameter.AppendFormat(" ,comment.char={0}", ComboBoxSelectedRStringLiteral(CommentComboBox));

            if (!string.IsNullOrEmpty(NAStringTextBox.Text)) {
                parameter.AppendFormat(" ,na.strings={0}", NAStringTextBox.Text.ToRStringLiteral());
            }

            if (preview) {
                parameter.Append(" ,nrows=20");
                return string.Format(CultureInfo.InvariantCulture, "read.csv({0})", parameter);
            } else {
                return string.Format(CultureInfo.InvariantCulture, "`{0}` <- read.csv({1})", VariableNameBox.Text, parameter);
            }
        }

        private string ComboBoxSelectedRStringLiteral(ComboBox comboBox) {
            return ((ComboBoxItemModel)comboBox.SelectedItem).Value.ToRStringLiteral();
        }

        private void AppendString(StringBuilder builder, string value) {
            value = value.Replace("\"", "\\\"");
            builder.AppendFormat("\"{0}\"", value);
        }

        private void FileOpenButton_Click(object sender, RoutedEventArgs e) {
            string filePath = VsAppShell.Current.BrowseForFileOpen(IntPtr.Zero, "CSV file|*.csv");
            if (!string.IsNullOrEmpty(filePath)) {
                SetFilePathAsync(filePath).DoNotWait();
            }
        }

        private async Task SetFilePathAsync(string filePath) {
            FilePathBox.Text = filePath;

            FilePathBox.CaretIndex = FilePathBox.Text.Length;
            FilePathBox.ScrollToEnd();

            var variableName = Path.GetFileNameWithoutExtension(FilePathBox.Text);
            VariableNameBox.Text = variableName;

            string text = ReadFile(FilePathBox.Text);
            InputFilePreview.Text = text;

            await PreviewAsync();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e) {
            RunButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            var expression = BuildCommandLine(false);
            if (expression != null) {
                RunAsync(expression).DoNotWait();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            base.Close();
        }

        private async Task PreviewAsync() {
            var expression = BuildCommandLine(true);
            if (expression != null) {
                try {
                    var grid = await GridDataSource.GetGridDataAsync(expression, null);

                    PopulateDataFramePreview(grid);
                    OnSuccess(true);
                } catch (Exception ex) {
                    OnError(ex.Message);
                }
            }
        }

        private async Task RunAsync(string expression) {
            try {
                REvaluationResult result = await EvaluateAsyncAsync(expression);
                if (result.ParseStatus == RParseStatus.OK && result.Error == null) {
                    OnSuccess(false);
                } else {
                    OnError(result.ToString());
                }
            } catch (Exception ex) {
                OnError(ex.Message);
            }
        }

        private async Task SetEncodingComboBoxAsync() {
            try {
                REvaluationResult result = await CallIconvListAsync();

                if (result.ParseStatus == RParseStatus.OK && result.Error == null) {
                    var jarray = result.JsonResult as JArray;
                    if (jarray != null) {
                        PopulateEncoding(jarray);
                    }
                } else {
                    OnError(result.ToString());
                }
            } catch (Exception ex) {
                OnError(ex.Message);
            }
        }

        private void OnSuccess(bool preview) {
            if (preview) {
                ErrorBlock.Visibility = Visibility.Collapsed;
                DataFramePreview.Visibility = Visibility.Visible;
            } else {
                base.Close();
            }
        }

        private void OnError(string errorText) {
            ErrorText.Text = errorText;
            ErrorBlock.Visibility = Visibility.Visible;
            DataFramePreview.Visibility = Visibility.Collapsed;
        }

        private async Task<REvaluationResult> CallIconvListAsync() {
            await TaskUtilities.SwitchToBackgroundThread();

            IRSession rSession = GetRSession();
            REvaluationResult result;
            using (var evaluator = await rSession.BeginEvaluationAsync()) {
                result = await evaluator.EvaluateAsync("as.list(iconvlist())", REvaluationKind.Json);
            }

            return result;
        }

        private async Task<REvaluationResult> EvaluateAsyncAsync(string expression) {
            await TaskUtilities.SwitchToBackgroundThread();

            IRSession rSession = GetRSession();
            REvaluationResult result;
            using (var evaluator = await rSession.BeginEvaluationAsync()) {
                result = await evaluator.EvaluateAsync(expression, REvaluationKind.Mutating);
            }

            return result;
        }

        private void PopulateEncoding(JArray jarray) {
            var encodings = new List<ComboBoxItemModel>();
            foreach (var item in jarray) {
                var value =  item.Value<string>();
                if (!string.IsNullOrEmpty(value)) {
                    encodings.Add(new ComboBoxItemModel(value, value));
                }
            }

            EncodingComboBox.ItemsSource = encodings;
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

            List<DataFramePreviewRowItem> rows = new List<DataFramePreviewRowItem>();
            for (int r = 0; r < gridData.Grid.Range.Rows.Count; r++) {
                var row = new DataFramePreviewRowItem();
                row.RowName = gridData.RowHeader[gridData.RowHeader.Range.Start + r];
                for (int c = 0; c < gridData.Grid.Range.Columns.Count; c++) {
                    row.Values.Add(gridData.Grid[gridData.Grid.Range.Rows.Start + r, gridData.Grid.Range.Columns.Start + c]);
                }
                rows.Add(row);
            }
            dg.ItemsSource = rows;
        }

        private string ReadFile(string filePath, int lineCount = 20) {
            StringBuilder sb = new StringBuilder();
            using(var sr = new StreamReader(filePath)) {
                int readCount = 0;
                while(readCount < lineCount) {
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
            public DataFramePreviewRowItem() {
                Values = new List<string>();
            }

            public string RowName { get; set; }
            public List<string> Values { get; private set; }
        }

        class ComboBoxItemModel {
            public ComboBoxItemModel(string name, string value) {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public string Value { get; }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            PreviewAsync().DoNotWait();
        }

        private void HeaderCheckBox_Changed(object sender, RoutedEventArgs e) {
            PreviewAsync().DoNotWait();
        }
    }
}
