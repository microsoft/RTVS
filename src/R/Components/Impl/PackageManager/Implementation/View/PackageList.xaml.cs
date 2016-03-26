// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    public partial class PackageList : UserControl {
        // Indicates wether check boxes are enabled on packages
        private bool _checkBoxesEnabled;
        private ScrollViewer _scrollViewer;

        public bool CheckBoxesEnabled {
            get { return _checkBoxesEnabled; }
            set {
                _checkBoxesEnabled = value;

                if (!_checkBoxesEnabled) {
                    // the current tab is not "updates", so the container
                    // should become invisible.
                    UpdateButtonContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        public PackageList() {
            InitializeComponent();
            CheckBoxesEnabled = false;
        }

        private void CheckBoxSelectAllPackages_Checked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void CheckBoxSelectAllPackages_Unchecked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void List_PreviewKeyUp(object sender, KeyEventArgs e) {
            throw new NotImplementedException();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ButtonUninstall_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void List_Loaded(object sender, RoutedEventArgs e) {
            List.Loaded -= List_Loaded;

            var c = VisualTreeHelper.GetChild(List, 0) as Border;
            if (c == null) {
                return;
            }

            c.Padding = new Thickness(0);
            _scrollViewer = VisualTreeHelper.GetChild(c, 0) as ScrollViewer;
            if (_scrollViewer == null) {
                return;
            }

            _scrollViewer.Padding = new Thickness(0);
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {

        }
    }
}
