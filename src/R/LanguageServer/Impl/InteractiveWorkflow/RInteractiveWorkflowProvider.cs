// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.LanguageServer.InteractiveWorkflow {
    internal sealed class RInteractiveWorkflowProvider: IRInteractiveWorkflowProvider {
        private readonly IServiceContainer _services;
        private readonly DisposableBag _disposableBag = DisposableBag.Create<RInteractiveWorkflowProvider>();
        private Lazy<IRInteractiveWorkflow> _instanceLazy;

        public RInteractiveWorkflowProvider(IServiceContainer services) {
            _services = services;
        }

        public IRInteractiveWorkflow GetOrCreate() {
            _disposableBag.ThrowIfDisposed();
            Interlocked.CompareExchange(ref _instanceLazy, new Lazy<IRInteractiveWorkflow>(CreateRInteractiveWorkflow), null);
            return _instanceLazy.Value;
        }

        public IRInteractiveWorkflow Active { get; }

        public void Dispose() => _disposableBag.TryDispose();

        private IRInteractiveWorkflow CreateRInteractiveWorkflow() {
            _disposableBag.Add(DisposeInstance);
            return new RInteractiveWorkflow(_services);
        }

        private void DisposeInstance() {
            var lazy = Interlocked.Exchange(ref _instanceLazy, null);
            if (lazy != null && lazy.IsValueCreated) {
                lazy.Value.Dispose();
            }
        }
    }
}
