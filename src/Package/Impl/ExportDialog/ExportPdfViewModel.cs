// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.Plots;


namespace Microsoft.VisualStudio.R.Package.ExportDialog {

    public class ExportPdfViewModel : BindableBase {
        private const double MIN_PDFSIZE_INCHES = 1.0;
        private const double MAX_PDFSIZE_INCHES = 1100.0;

        private bool _viewPlot;
        private bool _isSaveEnabled;
        private PDFExportOptions _pdfPaperType;

        private readonly IReadOnlyList<PDFExportOptions> _pdfPapers;
        private readonly IReadOnlyList<string> _paperOrientations;
        private readonly IReadOnlyList<string> _pdfDeviceOptions;

        public IReadOnlyList<PDFExportOptions> PDFPapers => _pdfPapers;
        public IReadOnlyList<string> Orientations => _paperOrientations;
        public IReadOnlyList<string> PrintOptions => _pdfDeviceOptions;

        public PDFExportOptions SelectedPDFPaperType {
            get { return _pdfPaperType; }
            set { SetProperty(ref _pdfPaperType, value); }
        }

        private string _selectedOrientation;
        public string SelectedOrientation {
            get { return _selectedOrientation; }
            set { SetProperty(ref _selectedOrientation, value); }
        }

        private string _selectedDevice;
        public string SelectedDevice {
            get { return _selectedDevice; }
            set { SetProperty(ref _selectedDevice, value); }
        }

        public bool ViewPlotAfterSaving {
            get { return _viewPlot; }
            set { SetProperty(ref _viewPlot, value); }
        }

        public bool IsSaveEnabled {
            get { return _isSaveEnabled; }
            set { SetProperty(ref _isSaveEnabled, value); }
        }

        private bool IsValidWidth { get; set; }
        private bool IsValidHeight { get; set; }
           
        public ExportPdfViewModel(ExportArguments pdfArguments) {
            IsSaveEnabled = true;
            IsValidWidth = true;
            IsValidHeight = true;

            _pdfPapers = PDFExportOptions.GetPdfPapers(pdfArguments.PixelWidth, pdfArguments.PixelHeight,pdfArguments.Resolution).ToList();
            SelectedPDFPaperType = _pdfPapers[0];
            SelectedPDFPaperType.PaperHeight = Validate(SelectedPDFPaperType.PaperHeight);
            SelectedPDFPaperType.PaperWidth = Validate(SelectedPDFPaperType.PaperWidth);

            _paperOrientations = PDFExportOptions.GetPaperOrientations().ToList();
            SelectedOrientation = _paperOrientations[0];

            _pdfDeviceOptions = PDFExportOptions.GetPdfDeviceOptions().ToList();
            SelectedDevice = _pdfDeviceOptions[0];
        }

        public void ValidateWidth(string val) {
            double res = ValidateValues(val);
            if (res != -1) {
                SelectedPDFPaperType.PaperWidth = res;
                IsValidWidth = true;
            } else {
                IsValidWidth = false;
            }
            IsSaveEnabled = (IsValidHeight && IsValidWidth);
        }

        public void ValidateHeight(string val) {
            double res = ValidateValues(val);
            if (res != -1) {
                SelectedPDFPaperType.PaperHeight = res;
                IsValidHeight = true;
            } else {
                IsValidHeight = false;
            }
            IsSaveEnabled = (IsValidHeight && IsValidWidth);
        }

        private double Validate(double val) {
            if (val > MAX_PDFSIZE_INCHES) {
                return MAX_PDFSIZE_INCHES;
            } else if (val < MIN_PDFSIZE_INCHES) {
                return MIN_PDFSIZE_INCHES;
            }
            return Math.Round(val, 2);
        }

        private double ValidateValues(string result) {
            float l = 0;
            bool isValid = float.TryParse(result, out l);
            if (!isValid) {
                return -1;
            }
            return Validate(l);
        }
    }
}
