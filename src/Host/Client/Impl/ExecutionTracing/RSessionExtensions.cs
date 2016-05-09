// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.ExecutionTracing {
    public static class RSessionExtensions {
        private static readonly Dictionary<IRSession, RExecutionTracer> _tracers = new Dictionary<IRSession, RExecutionTracer>();
        private static readonly SemaphoreSlim _tracersSem = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Obtain an instance of <see cref="IRExecutionTracer"/> for the given <see cref="IRSession"/>.
        /// </summary>
        public static async Task<IRExecutionTracer> TraceExecutionAsync(this IRSession session, CancellationToken cancellationToken = default(CancellationToken)) {
            RExecutionTracer tracer;

            await _tracersSem.WaitAsync(cancellationToken).ConfigureAwait(false);
            try {
                if (!_tracers.TryGetValue(session, out tracer)) {
                    tracer = new RExecutionTracer(session);
                    await tracer.InitializeAsync(cancellationToken).ConfigureAwait(false);
                    _tracers.Add(session, tracer);
                }

                session.Disposed += Session_Disposed;
            } finally {
                _tracersSem.Release();
            }

            return tracer;
        }

        private static void Session_Disposed(object sender, EventArgs e) {
            var session = (IRSession)sender;
            RExecutionTracer tracer;

            _tracersSem.Wait();
            try {
                if (_tracers.TryGetValue(session, out tracer)) {
                    session.Disposed -= Session_Disposed;
                    tracer.Detach();
                    _tracers.Remove(session);
                }
            } finally {
                _tracersSem.Release();
            }
        }
    }
}
