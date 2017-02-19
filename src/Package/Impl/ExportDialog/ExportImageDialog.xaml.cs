// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.ExportDialog {
    public partial class ExportImageDialog : PlatformDialogWindow, INotifyPropertyChanged {

        private int _userWidth;
        private int _userHeight;
        private bool _viewPlot;
        private const int MIN = 100;
        private const int MAX = 100000;
        private int _prevValidWidth;
        private int _prevValidHeight;
        private bool _maintainAspectRatio;
        private int _originalWidth;
        private int _originalHeight;

        public ExportImageDialog(ExportArguments imageArguments) {
            InitializeComponent();
            this.HasMaximizeButton = false;
            this.HasMinimizeButton = false;
            UserWidth = imageArguments.PixelWidth;
            UserHeight = imageArguments.PixelHeight;
            this.DataContext = this;
        }

        public int UserWidth {
            get { return _userWidth; }
            set {
                _userWidth = value;
                OnPropertyChanged("UserWidth");
            }
        }

        public int UserHeight {
            get { return _userHeight; }
            set {
                _userHeight = value;
                OnPropertyChanged("UserHeight");
            }
        }

        public bool ViewPlotAfterSaving {
            get { return _viewPlot; }
            set {
                _viewPlot = value;
                OnPropertyChanged("ViewPlotAfterSaving");
            }
        }

        public bool MaintainAspectRatio {
            get { return _maintainAspectRatio; }
            set {
                _maintainAspectRatio = value;
                if (_maintainAspectRatio) {
                    _originalWidth = UserWidth;
                    _originalHeight = UserHeight;
                }
                OnPropertyChanged("MaintainAspectRatio");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ExportImageParameters GetExportParameters() {
            ExportImageParameters exportImageParams = new ExportImageParameters();

            exportImageParams.PixelHeight = UserHeight;
            exportImageParams.PixelWidth = UserHeight;
            exportImageParams.ViewPlot = ViewPlotAfterSaving;
            return exportImageParams;
        }

        protected void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void HeightTextbox_LostFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            int res = ValidateValues(textBox.Text);
            if (res != -1) {
                textBox.Text = res.ToString();
                _prevValidHeight = UserHeight;
            } else {
                UserHeight = _prevValidHeight;
            }

            if (_maintainAspectRatio) {
                int w = GetSizeForWidth();
                UserWidth = w;
                if (w == MIN || w == MAX) {
                    int h = GetSizeForHeight();
                    UserHeight = h;
                }
            }
        }

        private void WidthTextbox_LostFocus(object sender, RoutedEventArgs e) {
            TextBox textBox = (TextBox)sender;
            int res = ValidateValues(textBox.Text);
            if (res != -1) {
                textBox.Text = res.ToString();
                _prevValidWidth = UserWidth;
            } else {
                UserWidth = _prevValidWidth;
            }

            if (_maintainAspectRatio) {
                int h = GetSizeForHeight();
                UserHeight = h;
                if (h == MIN || h == MAX) {
                    int w = GetSizeForWidth();
                    UserWidth = w;
                }
            }
        }

        private int GetSizeForWidth() {
            int width = UserHeight * _originalWidth / _originalHeight;
            int w = ValidateValues(width.ToString());
            return width = w;
        }

        private int GetSizeForHeight() {
            int height = UserWidth * _originalHeight / _originalWidth;
            int h = ValidateValues(height.ToString());
            return height = h;
        }

        private int ValidateValues(string result) {
            long l = 0;
            bool isValid = long.TryParse(result, out l);
            if (!isValid) {
                return -1;
            }
            if (l > MAX) {
                return MAX;
            } else if (l < MIN) {
                return MIN;
            }
            return (int)l;
        }
    }
}
