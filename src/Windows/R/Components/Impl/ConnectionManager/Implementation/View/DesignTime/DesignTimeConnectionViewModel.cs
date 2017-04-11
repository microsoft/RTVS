// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Threading;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.View.DesignTime {
#if DEBUG
    internal class DesignTimeConnectionViewModel : IConnectionViewModel {
        public string Name { get; set; }
        public string Path { get; set; }
        public string RCommandLineArguments { get; set; }
        public bool IsUserCreated { get; set; }
        public bool IsActive { get; set; }
        public bool IsEditing { get; set; }
        public bool IsConnected { get; set; }
        public bool IsRunning { get; set; }
        public CancellationTokenSource TestingConnectionCts { get; set; }
        public bool IsTestConnectionSucceeded { get; set; }
        public string TestConnectionFailedText { get; set; }
        public bool IsRemote { get; set; }

        public string OriginalName => Name;
        public string NameTextBoxTooltip => string.Empty;
        public string PathTextBoxTooltip => string.Empty;
        public string SaveButtonTooltip => string.Empty;
        public bool IsValid => false;
        public bool IsNameValid => false;
        public bool IsPathValid => false;

        public bool HasChanges => false;
        public bool IsRenamed => false;

        public DateTime LastUsed {
            get { return DateTime.Now; }
            set { throw new NotImplementedException(); }
        }

        public void Reset() { }
        public void Dispose() { }
        public string ConnectionTooltip => string.Empty;
        public void UpdatePath() { }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
    }
#endif
}