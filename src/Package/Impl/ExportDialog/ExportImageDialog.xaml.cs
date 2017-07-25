// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.R.Components.Plots;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public partial class ExportImageDialog : PlatformDialogWindow {

        private ExportImageViewModel _exportVM;
        public ExportImageDialog(ExportArguments imageArguments) {
            InitializeComponent();
            _exportVM = new ExportImageViewModel(imageArguments);
            DataContext = _exportVM;
        }

        public ExportImageParameters GetExportParameters() {
            ExportImageParameters exportImageParams = new ExportImageParameters();
            exportImageParams.PixelHeight = _exportVM.UserHeight;
            exportImageParams.PixelWidth = _exportVM.UserWidth;
            exportImageParams.ViewPlot = _exportVM.ViewPlotAfterSaving;
            return exportImageParams;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            DoSave();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DoCancel();
        }

        private void HeightTextbox_LostFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            _exportVM.ValidateHeight(textBox.Text);
        }

        private void WidthTextbox_LostFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            _exportVM.ValidateWidth(textBox.Text);
        }

        private void SaveButton_PreviewKeyUp(object sender,KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DoSave();
            }
        }

        private void CancelButton_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                DoCancel();
            }
        }

        private void DoSave() {
            DialogResult = true;
            Close();
        }

        private void DoCancel() {
            DialogResult = false;
            Close();
        }
    }
}
