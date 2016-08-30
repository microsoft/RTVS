// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.R.Components.ConnectionManager.ViewModel {
    public interface IConnectionManagerViewModel : INotifyPropertyChanged, IDisposable {
        ReadOnlyObservableCollection<IConnectionViewModel> Items { get; }
        IConnectionViewModel SelectedConnection { get; }
        IConnectionViewModel NewConnection { get; }
        bool IsConnected { get; }

        void SelectConnection(IConnectionViewModel connection);
        void AddNew();
        void Cancel(IConnectionViewModel connection);
        void BrowseLocalPath(IConnectionViewModel connection);
        void Edit(IConnectionViewModel connection);
        Task TestConnectionAsync(IConnectionViewModel connection);
        void Save(IConnectionViewModel connectionViewModel);
        bool TryDelete(IConnectionViewModel connection);

        Task ConnectAsync(IConnectionViewModel connection);
    }
}