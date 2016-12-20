// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.View.DesignTime {
#if DEBUG
    internal class DesignTimeConnectionStatusBarViewModel : IConnectionStatusBarViewModel {
        public bool IsConnected { get; set; } = false;
        public bool IsRunning { get; set; } = false;
        public bool IsRemote { get; set; } = false;
        public string SelectedConnection { get; set; } = "Local: Microsoft R Open v3.3.0";
        public void ShowContextMenu(Point pointToScreen) {}
        public void Dispose() {}
    }
#endif
}
