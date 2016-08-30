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
        public ReadOnlyObservableCollection<IConnectionViewModel> Items { get; } = new ReadOnlyObservableCollection<IConnectionViewModel>(new ObservableCollection<IConnectionViewModel> {
            new DesignTimeConnectionViewModel { IsActive = false, IsRemote = false, IsConnected = false, Name = "Microsoft R", Path = @"c:\Program Files\Microsoft\R Client\R_SERVER" },
            new DesignTimeConnectionViewModel { IsActive = true, IsRemote = false, IsConnected = false, Name = "CRAN R", Path = @"c:\Program Files\R\R-3.3.1", RCommandLineArguments = "--slave" },
            new DesignTimeConnectionViewModel { IsActive = true, IsRemote = false, IsConnected = true, Name = "Old CRAN R", Path = @"c:\Program Files\R\R-3.2.3", RCommandLineArguments = "--slave" },
            new DesignTimeConnectionViewModel { IsActive = false, IsRemote = true, IsConnected = false, Name = "Corporate Remote", Path = @"https:\\corporate\RemoteRHost" },
            new DesignTimeConnectionViewModel { IsActive = true, IsRemote = true, IsConnected = false, Name = "Personal Remote", Path = @"https:\\192.168.1.120:5000" },
            new DesignTimeConnectionViewModel { IsActive = true, IsRemote = true, IsConnected = true, Name = "Public Remote", Path = @"https:\\public\FreeRHost" },
        });

        public event PropertyChangedEventHandler PropertyChanged;

        public IConnectionViewModel SelectedConnection => Items[0];
        public IConnectionViewModel NewConnection => new DesignTimeConnectionViewModel();
        public bool IsConnected => false;

        public void SelectConnection(IConnectionViewModel connection) { }
        public void AddNew() {}
        public void Cancel(IConnectionViewModel connection) { }
        public void BrowseLocalPath(IConnectionViewModel connection) { }
        public void Edit(IConnectionViewModel connection) { }
        public Task TestConnectionAsync(IConnectionViewModel connection) => Task.CompletedTask;
        public void Save(IConnectionViewModel connectionViewModel) { }
        public bool TryDelete(IConnectionViewModel connection) => false;
        public Task ConnectAsync(IConnectionViewModel connection) => Task.CompletedTask;

        public void Dispose() {}
        public Task<int> Search(string searchString, CancellationToken cancellationToken) => Task.FromResult(0);
    }
#endif
}