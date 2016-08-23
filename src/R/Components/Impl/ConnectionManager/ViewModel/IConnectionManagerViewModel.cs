// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.R.Components.Search;

namespace Microsoft.R.Components.ConnectionManager.ViewModel {
    public interface IConnectionManagerViewModel : ISearchHandler, INotifyPropertyChanged, IDisposable {
        ReadOnlyObservableCollection<object> Items { get; }
        IConnectionViewModel SelectedConnection { get; }

        void SelectConnection(IConnectionViewModel connection);
        void AddNew();
        void CancelSelected();
        void SaveSelected();

        Task ConnectAsync(IConnectionViewModel connection);
    }
}