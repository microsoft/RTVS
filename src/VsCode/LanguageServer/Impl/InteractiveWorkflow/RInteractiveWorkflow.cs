// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ConnectionManager;
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

        public RInteractiveWorkflow(IServiceContainer services) {
            _services = services;

            Console = new Console(_services);
            RSessions = new RSessionProvider(_services, Console);
            RSession = RSessions.GetOrCreate(SessionNames.InteractiveWindow);

            _disposableBag = DisposableBag.Create<RInteractiveWorkflow>()
                .Add(RSessions);
        }

        public void Dispose() => _disposableBag.TryDispose();

        public ICoreShell Shell => _services.GetService<ICoreShell>();
        public IConnectionManager Connections { get; }
        public IConsole Console { get; }
        public IRHistory History { get; }
        public IRPackageManager Packages { get; }
        public IRPlotManager Plots { get; }
        public IRSessionProvider RSessions { get; }
        public IRSession RSession { get; }
        public IRInteractiveWorkflowOperations Operations { get; }
    }
}
