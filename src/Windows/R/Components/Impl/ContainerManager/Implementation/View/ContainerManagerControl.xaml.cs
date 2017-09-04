// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ContainerManager.Implementation.ViewModel;

namespace Microsoft.R.Components.ContainerManager.Implementation.View {
    public partial class ContainerManagerControl {
        private ContainerManagerViewModel ViewModel { get; }

        public ContainerManagerControl(IServiceContainer services) {
            InitializeComponent();
            ViewModel = new ContainerManagerViewModel(services);
            DataContext = ViewModel;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e) => ViewModel.ShowCreateLocalDocker();

        private void ButtonShowWorkspaces_Click(object sender, RoutedEventArgs e) => ViewModel.ShowConnections();

        private void Container_PreviewKeyUp(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.Key == Key.Delete && !(keyEventArgs.OriginalSource is TextBox)) {
                ViewModel.Delete(GetContainer(keyEventArgs));
            }
        }

        private void AddLocalDocker_PreviewKeyUp(object sender, KeyEventArgs keyEventArgs) {
            if (keyEventArgs.Key == Key.Escape) {
                ViewModel.CancelCreateLocalDocker();
            }
        }

        private void ButtonCreateLocalDocker_Click(object sender, RoutedEventArgs e) => ViewModel.CreateLocalDocker();

        private void ButtonCancelCreateLocalDocker_Click(object sender, RoutedEventArgs e) => ViewModel.CancelCreateLocalDocker();

        private void ButtonStart_Click(object sender, RoutedEventArgs e) => ViewModel.Start(GetContainer(e));

        private void ButtonStop_Click(object sender, RoutedEventArgs e) => ViewModel.Stop(GetContainer(e));

        private void ButtonDelete_Click(object sender, RoutedEventArgs e) => ViewModel.Delete(GetContainer(e));

        private static ContainerViewModel GetContainer(RoutedEventArgs e) => ((FrameworkElement)e.Source).DataContext as ContainerViewModel;
    }
}
