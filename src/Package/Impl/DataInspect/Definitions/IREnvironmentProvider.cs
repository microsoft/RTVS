// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal interface IREnvironmentProvider : IDisposable, INotifyPropertyChanged {
        ObservableCollection<IREnvironment> Environments { get; }
        IREnvironment SelectedEnvironment { get; }
        Task RefreshEnvironmentsAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
