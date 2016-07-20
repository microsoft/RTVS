// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace Microsoft.R.Components.ConnectionManager.ViewModel {
    public interface IConnectionStatusBarViewModel {
        bool IsConnected { get; set; }
        string SelectedConnection { get; set; }

        void ShowContextMenu(Point pointToScreen);
    }
}
