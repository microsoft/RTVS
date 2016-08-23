// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Components.ConnectionManager.ViewModel;

namespace Microsoft.R.Components.ConnectionManager.Implementation.View.DesignTime {
#if DEBUG
    internal class DesignTimeConnectionManagerViewModel : IConnectionManagerViewModel {
        public ReadOnlyObservableCollection<object> Items { get; } = new ReadOnlyObservableCollection<object>(new ObservableCollection<object> {
            new DesignTimeConnectionViewModel { IsConnected = false, IsRemote = false, Name = "Microsoft R", Path = @"c:\Program Files\Microsoft\R Client\R_SERVER" },
            new DesignTimeConnectionViewModel { IsConnected = false, IsRemote = false, Name = "CRAN R", Path = @"c:\Program Files\R\R-3.3.1", RCommandLineArguments = "--slave" },
            new DesignTimeConnectionViewModel { IsConnected = false, IsRemote = true, Name = "Corporate Remote", Path = @"https:\\corporate\RemoteRHost" },
            new DesignTimeConnectionViewModel { IsConnected = true, IsRemote = true, Name = "Personal Remote", Path = @"https:\\192.168.1.120:5000" }
        });

        public event PropertyChangedEventHandler PropertyChanged;

        public IConnectionViewModel SelectedConnection => (IConnectionViewModel)Items[1];

        public void SelectConnection(IConnectionViewModel connection) { }
        public void AddNew() {}
        public void CancelSelected() { }
        public void SaveSelected() { }
        public Task ConnectAsync(IConnectionViewModel connection) => Task.CompletedTask;

        public void Dispose() {}
        public Task<int> Search(string searchString, CancellationToken cancellationToken) => Task.FromResult(0);
    }
#endif
}