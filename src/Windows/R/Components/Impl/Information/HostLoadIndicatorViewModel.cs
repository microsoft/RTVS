// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Common.Core.UI;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Information {
    public sealed class HostLoadIndicatorViewModel : BindableBase, IDisposable {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IMainThread _mainThread;
        private readonly DisposableBag _disposableBag;

        private double _cpuLoad;
        private double _memoryLoad;
        private double _networkLoad;
        private string _tooltip;

        public double CpuLoad {
            get => _cpuLoad; set => SetProperty(ref _cpuLoad, value);
        }

        public double MemoryLoad {
            get => _memoryLoad; set => SetProperty(ref _memoryLoad, value);
        }

        public double NetworkLoad {
            get => _networkLoad; set => SetProperty(ref _networkLoad, value);
        }

        public string Tooltip {
            get => _tooltip; set => SetProperty(ref _tooltip, value);
        }

        public HostLoadIndicatorViewModel(IRSessionProvider sessionProvider, IMainThread mainThread) {
            _sessionProvider = sessionProvider;
            _mainThread = mainThread;
            _disposableBag = DisposableBag.Create<HostLoadIndicatorViewModel>()
                .Add(() => _sessionProvider.HostLoadChanged -= OnHostLoadChanged);

            _sessionProvider.HostLoadChanged += OnHostLoadChanged;
        }

        private void OnHostLoadChanged(object sender, HostLoadChangedEventArgs e) {
            _mainThread.Post(() => {
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
