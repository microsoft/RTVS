// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.Plots;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public class ExportImageViewModel : BindableBase {

        private int _userWidth;
        private int _userHeight;
        private bool _viewPlot;
        private bool _maintainAspectRatio;
        private bool _isSaveEnabled;
        private const int MIN_PAPERSIZE_PIXELS = 100;
        private const int MAX_PAPERSIZE_PIXELS = 100000;

        public ExportImageViewModel(ExportArguments imageArguments) {
            UserWidth = imageArguments.PixelWidth;
            UserHeight = imageArguments.PixelHeight;
            IsSaveEnabled = true;
            IsValidHeight = true;
            IsValidWidth = true;
        }

        public int OriginalWidth { get; set; }

        public int OriginalHeight { get; set; }

        public int UserWidth {
            get { return _userWidth; }
            set { SetProperty(ref _userWidth, value); }
        }

        public int UserHeight {
            get { return _userHeight; }
            set { SetProperty(ref _userHeight, value); }
        }

        public bool ViewPlotAfterSaving {
            get { return _viewPlot; }
            set { SetProperty(ref _viewPlot, value); }
        }

        public bool MaintainAspectRatio {
            get { return _maintainAspectRatio; }
            set {
                if (value) {
                    OriginalWidth = UserWidth;
                    OriginalHeight = UserHeight;
                }
                SetProperty(ref _maintainAspectRatio, value);
            }
        }
        public bool IsSaveEnabled {
            get { return _isSaveEnabled; }
            set { SetProperty(ref _isSaveEnabled, value); }
        }
        private bool IsValidWidth { get; set; }
        private bool IsValidHeight { get; set; }

        public void ValidateWidth(string val) {
            int res = ValidateValues(val);
            if (res != -1) {
                UserWidth = res;
                IsValidWidth = true;
            } else {
                IsValidWidth = false;
            }
            IsSaveEnabled = (IsValidHeight && IsValidWidth);
            if (MaintainAspectRatio) {
                int h = CalculateHeight();
                UserHeight = h;
                if (h == MIN_PAPERSIZE_PIXELS || h == MAX_PAPERSIZE_PIXELS) {
                    UserWidth = CalculateWidth();
                }
            }
        }

        public void ValidateHeight(string val) {
            int res = ValidateValues(val);
            if (res != -1) {
                UserHeight = res;
                IsValidHeight = true;
            } else {
                IsValidHeight = false;
            }
            IsSaveEnabled = (IsValidHeight && IsValidWidth);
            if (MaintainAspectRatio) {
                int w = CalculateWidth();
                UserWidth = w;
                if (w == MIN_PAPERSIZE_PIXELS || w == MAX_PAPERSIZE_PIXELS) {
                    UserHeight = CalculateHeight();
                }
            }
        }

        private int ValidateValues(string result) {
            long l = 0;
            bool isValid = long.TryParse(result, out l);
            if (!isValid) {
                return -1;
            }
            return Validate((int)l);
        }

        private int Validate(int val) {
            if (val > MAX_PAPERSIZE_PIXELS) {
                return MAX_PAPERSIZE_PIXELS;
            } else if (val < MIN_PAPERSIZE_PIXELS) {
                return MIN_PAPERSIZE_PIXELS;
            }
            return val;
        }

        private int CalculateWidth() {
            double whratio = OriginalWidth / (double)OriginalHeight;
            int width = (int)Math.Round(UserHeight * whratio);
            return Validate(width);
        }

        private int CalculateHeight() {
            double hwratio = OriginalHeight / (double)OriginalWidth;
            int height = (int)Math.Round(UserWidth * hwratio);
            return Validate(height);
        }
    }
}
