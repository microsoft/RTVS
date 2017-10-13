// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ContainerManager.Implementation.ViewModel;
using Microsoft.R.Wpf;
using Microsoft.R.Wpf.Themes;

namespace Microsoft.R.Components.ContainerManager.Implementation.View {
    public partial class ContainerManagerControl {
        private readonly IServiceContainer _services;
        private readonly IThemeUtilities _theme;
        private ContainerManagerViewModel ViewModel { get; }

        public ContainerManagerControl(IServiceContainer services) {
            InitializeComponent();
            _services = services;

            _theme = services.GetService<IThemeUtilities>();
            var ui = services.UI();
            ui.UIThemeChanged += OnThemeChanged;
            SetImageBackground();

            ViewModel = new ContainerManagerViewModel(services);
            DataContext = ViewModel;
        }

        private void OnThemeChanged(object sender, EventArgs e) => SetImageBackground();

        private void SetImageBackground() {
            _theme.SetImageBackgroundColor(this, Brushes.ToolWindowBackgroundColorKey);
            _theme.SetThemeScrollBars(this);
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e) => ViewModel.ShowCreateLocalDocker();

        private void ButtonFromFile_Click(object sender, RoutedEventArgs e) => ViewModel.ShowLocalDockerFromFile();

        private void ButtonShowWorkspaces_Click(object sender, RoutedEventArgs e) => ViewModel.ShowConnections();

        private void Container_PreviewKeyUp(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.Key == Key.Delete && !(keyEventArgs.OriginalSource is TextBox)) {
                ViewModel.DeleteAsync(GetContainer(keyEventArgs)).DoNotWait();
            }
        }

        private void Container_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ViewModel.ConnectToContainerDefaultConnectionAsync(GetContainer(e)).DoNotWait();
            e.Handled = true;
        }

        private void AddLocalDocker_PreviewKeyUp(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.Key == Key.Escape) {
                ViewModel.CancelCreateLocalDocker();
            }
        }

        private void PasswordBoxNewDocker_OnPasswordChanged(object sender, RoutedEventArgs e) {
            var newLocalDocker = ViewModel.NewLocalDocker;
            if (newLocalDocker != null) {
                newLocalDocker.Password = ((PasswordBox) e.OriginalSource).SecurePassword;
            }
        }

        private void ButtonBrowseDockerTemplate_Click(object sender, RoutedEventArgs e) {
            ViewModel.BrowseDockerTemplate();
        }

        private void ButtonCreateLocalDocker_Click(object sender, RoutedEventArgs e) => ViewModel.CreateLocalDockerAsync().DoNotWait();

        private void ButtonCancelCreateLocalDocker_Click(object sender, RoutedEventArgs e) => ViewModel.CancelCreateLocalDocker();

        private void ButtonCreateLocalDockerFromFile_Click(object sender, RoutedEventArgs e) => ViewModel.CreateLocalDockerFromFileAsync().DoNotWait();

        private void ButtonCancelLocalDockerFromFile_Click(object sender, RoutedEventArgs e) => ViewModel.CancelLocalDockerFromFile();

        private void ButtonStart_Click(object sender, RoutedEventArgs e) => ViewModel.StartAsync(GetContainer(e)).DoNotWait();

        private void ButtonStop_Click(object sender, RoutedEventArgs e) => ViewModel.StopAsync(GetContainer(e)).DoNotWait();

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) => ViewModel.DeleteAsync(GetContainer(e)).DoNotWait();

        private void RefreshDocker(object sender, RequestNavigateEventArgs e) => ViewModel.RefreshDocker();

        private void RefreshDockerVersions(object sender, RequestNavigateEventArgs e) => ViewModel.RefreshDockerVersions();

        private void RepositoryUri_RequestNavigate(object sender, RequestNavigateEventArgs e) => _services.Process().Start(e.Uri.AbsoluteUri);

        private static ContainerViewModel GetContainer(RoutedEventArgs e) => ((FrameworkElement)e.Source).DataContext as ContainerViewModel;
    }
}
