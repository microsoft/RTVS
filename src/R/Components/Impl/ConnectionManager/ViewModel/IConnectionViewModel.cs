// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Threading;

namespace Microsoft.R.Components.ConnectionManager.ViewModel {
    public interface IConnectionViewModel : IConnectionInfo, INotifyPropertyChanged {
        new string Name { get; set; }
        new string Path { get; set; }
        new string RCommandLineArguments { get; set; }

        bool IsActive { get; }
        bool IsEditing { get; set; }
        bool IsConnected { get; }
        bool IsRunning { get; }
        CancellationTokenSource TestingConnectionCts { get; set; }
        bool IsTestConnectionSucceeded { get; set; }
        string TestConnectionFailedText { get; set; }

        string OriginalName { get; }
        string NameTextBoxTooltip { get; }
        string PathTextBoxTooltip { get; }
        string SaveButtonTooltip { get; }
        bool HasChanges { get; }
        bool IsValid { get; }
        bool IsNameValid { get; }
        bool IsPathValid { get; }
        bool IsRenamed { get; }
        bool IsRemote { get; }
        void Reset();
        
        /// <summary>
        /// Update the path with a default scheme and port, if possible.
        /// </summary>
        void UpdatePath();
    }
}