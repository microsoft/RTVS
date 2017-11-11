// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Plots;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    internal sealed class RInteractiveWorkflow: IRInteractiveWorkflow {
        private readonly IServiceContainer _services;
        private readonly DisposableBag _disposableBag;

        public IServiceContainer Services => _services;
        public IConsole Console => _services.GetService<IConsole>();
        public IRSessionProvider RSessions => _services.GetService<IRSessionProvider>();
        public IContainerManager Containers { get; }

        public IRSession RSession { get; }
        public IConnectionManager Connections { get; }
        public IRHistory History { get; }
        public IRPackageManager Packages { get; }
        public IRPlotManager Plots { get; }
        public IRInteractiveWorkflowOperations Operations { get; }

        public RInteractiveWorkflow(IServiceContainer services) {
            _services = services.Extend()
                .AddService<IRInteractiveWorkflow>(this)
                .AddService<IConsole, Console>()
                .AddService<IRSessionProvider, RSessionProvider>();

            RSession = RSessions.GetOrCreate(SessionNames.InteractiveWindow);

            _disposableBag = DisposableBag.Create<RInteractiveWorkflow>()
                .Add(RSessions);
        }

        public void Dispose() => _disposableBag.TryDispose();
    }
}
