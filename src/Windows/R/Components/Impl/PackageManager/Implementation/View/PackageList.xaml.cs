// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Wpf;
using Microsoft.R.Wpf.Themes;

namespace Microsoft.R.Components.PackageManager.Implementation.View {
    public partial class PackageList : UserControl {
        private IRPackageManagerViewModel Model => DataContext as IRPackageManagerViewModel;
        private AutomationPeer _peer;
        private IServiceContainer _services;
        private IUIService _ui;

        // Indicates wether check boxes are enabled on packages
        private bool _checkBoxesEnabled;

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

        public void Initialize(IServiceContainer services) {
            _services = services;
            _ui = services.UI();
            _ui.UIThemeChanged += OnUIThemeChanged;
            SetImageBackground();
        }

        private void OnUIThemeChanged(object sender, EventArgs e) {
            SetImageBackground();
        }

        public void SetImageBackground() {
            var theme = _services.GetService<IThemeUtilities>();
            theme.SetImageBackgroundColor(List, Brushes.ToolWindowBackgroundColorKey);
            theme.SetThemeScrollBars(List);
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.UpdateAsync(package).DoNotWait();
        }

        private static IRPackageViewModel GetPackage(RoutedEventArgs e) {
            return ((IRPackageViewModel)((FrameworkElement)e.Source).DataContext);
        }

        private void List_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Space) {
                Model?.DefaultActionAsync().DoNotWait();
                AutomationPeer?.RaiseAutomationEvent(AutomationEvents.AutomationFocusChanged);
            }
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Model?.DefaultActionAsync().DoNotWait();
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var package = e.AddedItems.OfType<IRPackageViewModel>().FirstOrDefault();
            if (package != null) {
                Model.SelectPackage(package);
                List.ScrollIntoView(package);
            }
        }

        private void ButtonUninstall_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.UninstallAsync(package).DoNotWait();
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.InstallAsync(package).DoNotWait();
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.LoadAsync(package).DoNotWait();
        }

        private void ButtonUnload_Click(object sender, RoutedEventArgs e) {
            var package = GetPackage(e);
            Model?.UnloadAsync(package).DoNotWait();
        }

        protected override AutomationPeer OnCreateAutomationPeer() {
            _peer = _peer ?? base.OnCreateAutomationPeer();
            return _peer;
        }

        private AutomationPeer AutomationPeer {
            get {
                _peer = _peer ?? UIElementAutomationPeer.CreatePeerForElement(this);
                return _peer;
            }
        }
    }
}
