// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public partial class ExportPDFDialog : PlatformDialogWindow, INotifyPropertyChanged {

        private const double MIN = 1.0;
        private const double MAX = 1100.0;
        private double _prevValidWidth = MIN;
        private double _prevValidHeight = MIN;
        private bool _viewPlot;
        private ExportArguments _pdfArguments;
        private PDFExportOptions _pdfPaperType;
        private List<PDFExportOptions> _pdfPapers;
        private List<OrientationEnum> _orientations;
        private List<PdfPrintOptionsEnum> _printOptions;
        public event PropertyChangedEventHandler PropertyChanged;

        public List<PDFExportOptions> PDFPapers => _pdfPapers;
        public List<OrientationEnum> Orientations => _orientations;
        public List<PdfPrintOptionsEnum> PrintOptions => _printOptions;

        public PDFExportOptions SelectedPDFPaperType {
            get { return _pdfPaperType; }
            set {
                _pdfPaperType = value;
                OnPropertyChanged("SelectedPDFPaperType");
            }
        }

        private OrientationEnum _orientationEnum;
        public OrientationEnum SelectedOrientation {
            get { return _orientationEnum; }
            set {
                _orientationEnum = value;
                OnPropertyChanged("SelectedOrientation");
            }
        }

        private PdfPrintOptionsEnum _selectedDevice;
        public PdfPrintOptionsEnum SelectedDevice {
            get { return _selectedDevice; }
            set {
                _selectedDevice = value;
                OnPropertyChanged("SelectedDevice");
            }
        }

        public bool ViewPlotAfterSaving {
            get { return _viewPlot; }
            set {
                _viewPlot = value;
                OnPropertyChanged("ViewPlotAfterSaving");
            }
        }

        public ExportPDFDialog(ExportArguments pdfArguments) {
            InitializeComponent();

            _pdfArguments = pdfArguments;
            _pdfPapers = PDFExportOptions.GetPdfPapers(pdfArguments.PixelWidth, pdfArguments.PixelHeight);
            _orientations = new List<OrientationEnum> { OrientationEnum.Portrait, OrientationEnum.Landscape };
            _printOptions = new List<PdfPrintOptionsEnum> { PdfPrintOptionsEnum.DefaultPDF, PdfPrintOptionsEnum.CairoPDF };
            SelectedPDFPaperType = _pdfPapers[0];
            SelectedPDFPaperType.PaperHeight = ValidateValues(SelectedPDFPaperType.PaperHeight.ToString());
            SelectedPDFPaperType.PaperWidth = ValidateValues(SelectedPDFPaperType.PaperWidth.ToString());
            DataContext = this;
        }

        internal ExportPdfParameters GetExportParameters() {
            ExportPdfParameters exportPdfParams = new ExportPdfParameters(SelectedPDFPaperType, SelectedOrientation, SelectedDevice, ViewPlotAfterSaving);
            return exportPdfParams;
        }

        protected void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void VariableWidthTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            double res = ValidateValues(textBox.Text);
            if (res != -1) {
                textBox.Text = res.ToString();
                _prevValidWidth = SelectedPDFPaperType.PaperWidth;
            } else {
                textBox.Text = _prevValidWidth.ToString();
            }
        }

        private void VariableHeightTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            double res = ValidateValues(textBox.Text);
            if (res != -1) {
                textBox.Text = res.ToString();
                _prevValidHeight = SelectedPDFPaperType.PaperHeight;
            } else {
                textBox.Text = _prevValidHeight.ToString();
            }
        }

        private double ValidateValues(string result) {
            float l = 0;
            bool isValid = float.TryParse(result, out l);
            if (!isValid) {
                return -1;
            }

            if (l > MAX) {
                return MAX;
            } else if (l < MIN) {
                return MIN;
            }
            return Math.Round(l, 2);
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
