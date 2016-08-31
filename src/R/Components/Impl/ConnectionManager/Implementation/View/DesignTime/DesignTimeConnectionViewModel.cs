// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.View.DesignTime {
#if DEBUG
    internal class DesignTimeConnectionViewModel : IConnectionViewModel {
        public event PropertyChangedEventHandler PropertyChanged;

        public Uri Id { get; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string RCommandLineArguments { get; set; }
        public bool IsActive { get; set; }
        public bool IsEditing { get; set; }
        public bool IsConnected { get; set; }
        public bool IsRemote { get; set; }

        public string SaveButtonTooltip { get; } = string.Empty;
        public bool IsValid { get; } = false;
        public bool HasChanges { get; } = false;

        public void Reset() {}
        public void Dispose() {}
    }
#endif
}