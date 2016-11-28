// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Wpf;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Information {
    public sealed class HostLoadIndicatorViewModel : BindableBase, IDisposable {
        private readonly IRSessionProvider _sessionProvider;
        private readonly ICoreShell _shell;
        private readonly DisposableBag _disposableBag;

        private double _cpuLoad;
        private double _memoryLoad;
        private double _networkLoad;
        private string _tooltip;

        public double CpuLoad {
            get { return _cpuLoad; }
            set { SetProperty(ref _cpuLoad, value); }
        }

        public double MemoryLoad {
            get { return _memoryLoad; }
            set { SetProperty(ref _memoryLoad, value); }
        }

        public double NetworkLoad {
            get { return _networkLoad; }
            set { SetProperty(ref _networkLoad, value); }
        }

        public string Tooltip {
            get { return _tooltip; }
            set { SetProperty(ref _tooltip, value); }
        }

        public HostLoadIndicatorViewModel(IRSessionProvider sessionProvider, ICoreShell shell) {
            _sessionProvider = sessionProvider;
            _shell = shell;
            _disposableBag = DisposableBag.Create<HostLoadIndicatorViewModel>()
                .Add(() => _sessionProvider.BrokerStateChanged -= BrokerStateChanged);

            _sessionProvider.BrokerStateChanged += BrokerStateChanged;
        }

        internal void BrokerStateChanged(object sender, BrokerStateChangedEventArgs e) {
            _shell.DispatchOnUIThread(() => {
                CpuLoad = e.HostLoad.CpuLoad;
                MemoryLoad = e.HostLoad.MemoryLoad;
                NetworkLoad = e.HostLoad.NetworkLoad;

                Tooltip = string.Format(CultureInfo.InvariantCulture,
                    Resources.HostLoad_Tooltip,
                    (int)Math.Round(100 * CpuLoad),
                    (int)Math.Round(100 * MemoryLoad),
                    (int)Math.Round(100 * NetworkLoad));
            });
        }

        public void Dispose() => _disposableBag.TryDispose();
    }
}
