// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.R.Components.ConnectionManager.ViewModel {
    public interface IConnectionViewModel : INotifyPropertyChanged {
        Uri Id { get; }

        string Name { get; set; }
        string Path { get; set; }
        string RCommandLineArguments { get; set; }
        bool IsUserCreated { get; set; }
        bool IsActive { get; set; }
        bool IsEditing { get; set; }
        bool IsConnected { get; set; }

        string SaveButtonTooltip { get; }
        bool IsRemote { get; }
        bool IsValid { get; }
        bool HasChanges { get; }
        
        void Reset();
    }
}