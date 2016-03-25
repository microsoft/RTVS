// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudio.R.Package.Repl;
using static System.FormattableString;
using Microsoft.Common.Core;
using System.IO;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using System.Collections.ObjectModel;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataImport {
    /// <summary>
    /// Interaction logic for ImportDataWindow.xaml
    /// </summary>
    public partial class ImportDataWindow : DialogWindow {
        private ObservableCollection<RStringModel> Separators = new ObservableCollection<RStringModel>() { new RStringModel("White space", ""), new RStringModel("Comma (,)", ","), new RStringModel("Semicolon (;)", ";"), new RStringModel(@"Tab (\t)", "\t") };
        private ObservableCollection<RStringModel> Decimals = new ObservableCollection<RStringModel>() { new RStringModel("Period (.)", "."), new RStringModel("Comma (,)", ",") };
        private ObservableCollection<RStringModel> Quotes = new ObservableCollection<RStringModel>() { new RStringModel("Double quote (\")", "\""), new RStringModel("Single quote (')", "'"), new RStringModel("None", "") };
        private ObservableCollection<RStringModel> Comments = new ObservableCollection<RStringModel>() { new RStringModel("None", ""), new RStringModel("#", "#"), new RStringModel("!", "!"), new RStringModel("%", "%"), new RStringModel("@", "@"), new RStringModel("/", "/"), new RStringModel("~", "~") };

        public ImportDataWindow() {
            InitializeComponent();

            Title = "Import Dataset";

            SeparatorComboBox.ItemsSource = Separators;
            SeparatorComboBox.SelectedIndex = 1;

            DecimalComboBox.ItemsSource = Decimals;
            DecimalComboBox.SelectedIndex = 0;

            QuoteComboBox.ItemsSource = Quotes;
            QuoteComboBox.SelectedIndex = 0;

            CommentComboBox.ItemsSource = Comments;
            CommentComboBox.SelectedIndex = 0;
        }

        private string BuildCommandLine(bool preview = false) {
            var parameter = new StringBuilder();

            parameter.Append("file=");
            parameter.Append(FilePathBox.Text.ToRStringLiteral());

            parameter.AppendFormat(" ,header={0}", HeaderCheckBox.IsChecked.Value.ToString().ToUpperInvariant());

            parameter.Append(" ,sep=");
            parameter.Append(((RStringModel)SeparatorComboBox.SelectedItem).Value.ToRStringLiteral());

            parameter.Append(" ,dec=");
            parameter.Append(((RStringModel)DecimalComboBox.SelectedItem).Value.ToRStringLiteral());

            parameter.Append(" ,quote=");
            parameter.Append(((RStringModel)QuoteComboBox.SelectedItem).Value.ToRStringLiteral());

            parameter.Append(" ,comment.char=");
            parameter.Append(((RStringModel)CommentComboBox.SelectedItem).Value.ToRStringLiteral());

            if (!string.IsNullOrEmpty(NAStringTextBox.Text)) {
                parameter.Append(" ,na.strings=");
                AppendString(parameter, NAStringTextBox.Text.ToRStringLiteral());
            }

            if (preview) {
                parameter.Append(" ,nrows=20");
                return string.Format(CultureInfo.InvariantCulture, "read.csv({0})", parameter);
            } else {
                return string.Format(CultureInfo.InvariantCulture, "\"{0}\" <- read.csv({1})", VariableNameBox.Text, parameter);
            }
        }

        private void AppendString(StringBuilder builder, string value) {
            value = value.Replace("\"", "\\\"");
            builder.AppendFormat("\"{0}\"", value);
        }

        private void FileOpenButton_Click(object sender, RoutedEventArgs e) {
            IVsUIShell uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IntPtr dialogOwner;
            uiShell.GetDialogOwnerHwnd(out dialogOwner);

            FilePathBox.Text = Dialogs.BrowseForFileOpen(dialogOwner, "CSV file|*.csv").Replace('\\', '/');
            FilePathBox.CaretIndex = FilePathBox.Text.Length;

            var variableName = System.IO.Path.GetFileNameWithoutExtension(FilePathBox.Text);
            variableName.Replace("\"", "\\\"");
            VariableNameBox.Text = variableName;

            CommandPreviewBox.Text = BuildCommandLine();

            // TODO: here?
            string text = ReadFileALittle(FilePathBox.Text);
            InputFilePreview.Text = text;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e) {
            //RunButton.IsEnabled = false;
            //CancelButton.IsEnabled = false;

            PreviewAsync(BuildCommandLine(true)).DoNotWait();
            //RunAsync(CommandPreviewBox.Text).DoNotWait();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            base.Close();
        }

        private async Task PreviewAsync(string expression) {
            var grid = await GridDataSource.GetGridDataAsync(expression, null);

            VsAppShell.Current.DispatchOnUIThread(() => PopulateDataFramePreview(grid));
        }

        private async Task RunAsync(string expression) {
            await TaskUtilities.SwitchToBackgroundThread();

            try {
                IRSession rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();
                if (rSession == null) {
                    throw new InvalidOperationException(Invariant($"{nameof(IRSessionProvider)} failed to return RSession for importing data set"));
                }

                using (var evaluator = await rSession.BeginEvaluationAsync(false)) {
                    var result = await evaluator.EvaluateAsync(expression);
                    if (result.ParseStatus != RParseStatus.OK || result.Error != null) {
                        VsAppShell.Current.DispatchOnUIThread(() => OnError(result.ToString()));
                    } else {
                        VsAppShell.Current.DispatchOnUIThread(() => OnSuccess());
                    }
                }
            } catch (Exception ex) {
                VsAppShell.Current.DispatchOnUIThread(() => OnError(ex.Message));
            }
        }

        private void OnSuccess() {
            base.Close();
        }

        private void OnError(string errorText) {
            // TODO: implement this
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

        private string ReadFileALittle(string filePath) {
            const int NumberCharBuffer = 2048;

            char[] buffer = new char[NumberCharBuffer];
            int readCount = 0;
            using(var sr = new StreamReader(filePath)) {
                readCount = sr.ReadBlock(buffer, 0, NumberCharBuffer);
            }

            return new string(buffer, 0, readCount);
        }

        class DataFramePreviewRowItem {
            public DataFramePreviewRowItem() {
                Values = new List<string>();
            }

            public string RowName { get; set; }
            public List<string> Values { get; private set; }
        }

        class RStringModel {
            public RStringModel(string name, string value) {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public string Value { get; }
        }
    }
}
