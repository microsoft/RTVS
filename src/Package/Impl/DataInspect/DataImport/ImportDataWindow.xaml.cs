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

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataImport {
    /// <summary>
    /// Interaction logic for ImportDataWindow.xaml
    /// </summary>
    public partial class ImportDataWindow : DialogWindow {
        public ImportDataWindow() {
            InitializeComponent();

            Title = "Import Dataset";

            Header = true;
        }

        private bool Header { get; set; }

        private string Separator { get; }

        private string Decimal { get; }

        private string QuoteChars { get; }

        private string CommentChar { get; }

        private string NAString { get; }

        private string BuildCommandLine(bool preview = false) {
            var parameter = new StringBuilder();

            parameter.Append("file=");
            AppendString(parameter, FilePathBox.Text);

            parameter.AppendFormat(" ,header={0}", Header.ToString().ToUpperInvariant());

            if (!string.IsNullOrEmpty(Separator)) {
                parameter.Append(" ,separater=");
                AppendString(parameter, Separator);
            }

            if (!string.IsNullOrEmpty(Decimal)) {
                parameter.Append(" ,dec=");
                AppendString(parameter, Decimal);
            }

            if (!string.IsNullOrEmpty(QuoteChars)) {
                parameter.Append(" ,quote=");
                AppendString(parameter, QuoteChars);
            }

            if (!string.IsNullOrEmpty(CommentChar)) {
                Debug.Assert(CommentChar.Length == 1);
                parameter.Append(" ,comment.char=");
                AppendString(parameter, CommentChar);
            }

            if (!string.IsNullOrEmpty(NAString)) {
                parameter.Append(" ,na.strings=");
                AppendString(parameter, NAString);
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

            List<RowDataItem> rows = new List<RowDataItem>();
            for (int r = 0; r < gridData.Grid.Range.Rows.Count; r++) {
                var row = new RowDataItem();
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

        class RowDataItem {
            public RowDataItem() {
                Values = new List<string>();
            }

            public string RowName { get; set; }
            public List<string> Values { get; private set; }
        }
    }
}
