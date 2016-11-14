// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Components.Information {
    public sealed class HostLoadIndicatorViewModel : BindableBase, IDisposable {
        private readonly Timer _timer = new Timer();
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private bool _disposed;

        private double _cpuLoad;
        private double _memoryLoad;
        private double _networkLoad;

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

        public HostLoadIndicatorViewModel(IRInteractiveWorkflow interactiveWorkflow) {
            _interactiveWorkflow = interactiveWorkflow;

            _timer.Interval = 5000;
            _timer.AutoReset = true;
            _timer.Elapsed += OnTimer;
            _timer.Start();
        }

        private void OnTimer(object sender, ElapsedEventArgs e) {
            if (!_disposed) {
                _interactiveWorkflow.RSessions.Broker.GetHostInformationAsync<HostLoad>().ContinueWith((t) => {
                    if (t.IsCompleted && t.Result != null) {
                        _interactiveWorkflow.Shell.DispatchOnUIThread(() => {
                            CpuLoad = t.Result.CpuLoad;
                            MemoryLoad = t.Result.MemoryLoad;
                            NetworkLoad = t.Result.NetworkLoad;
                        });
                    }
                }).DoNotWait();
            }
        }

        public void Dispose() {
            _timer.Stop();
            _timer.Dispose();
            _disposed = true;
        }
    }
}
