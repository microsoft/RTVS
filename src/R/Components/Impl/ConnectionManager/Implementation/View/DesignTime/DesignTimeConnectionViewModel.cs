// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.View.DesignTime {
#if DEBUG
    internal class DesignTimeConnectionViewModel : IConnectionViewModel {
        public Uri Id { get; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string RCommandLineArguments { get; set; }
        public bool IsUserCreated { get; set; }
        public bool IsActive { get; set; }
        public bool IsEditing { get; set; }
        public bool IsConnected { get; set; }
        public bool IsRemote { get; set; }

        public string SaveButtonTooltip => string.Empty;
        public bool IsValid => false;
        public bool HasChanges => false;
        public DateTime LastUsed => DateTime.Now;
        public void Reset() { }
        public void Dispose() { }
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
    }
#endif
}