// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Controls;
using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public partial class ExportPDFDialog : PlatformDialogWindow {


        ExportPdfViewModel _exportPdfViewModel;
        public ExportPDFDialog(ExportArguments pdfArguments) {
            InitializeComponent();
            _exportPdfViewModel = new ExportPdfViewModel(pdfArguments);
            DataContext = _exportPdfViewModel;
        }

        internal ExportPdfParameters GetExportParameters() {
            ExportPdfParameters exportPdfParams = new ExportPdfParameters(_exportPdfViewModel.SelectedPDFPaperType, _exportPdfViewModel.SelectedOrientation, _exportPdfViewModel.SelectedDevice, _exportPdfViewModel.ViewPlotAfterSaving);
            return exportPdfParams;
        }

        private void VariableWidthTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            _exportPdfViewModel.ValidateWidth(textBox.Text);
           
        }

        private void VariableHeightTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            _exportPdfViewModel.ValidateHeight(textBox.Text);
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}
