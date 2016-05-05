// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.StackTracing;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IDataObjectEvaluator))]
    public sealed class DebugObjectEvaluator : IDataObjectEvaluator {
        private readonly IRSessionProvider _rSessionProvider;

        [ImportingConstructor]
        public DebugObjectEvaluator(IRSessionProvider rSessionProvider) {
            _rSessionProvider = rSessionProvider;
        }

        public async Task<IREvaluationInfo> EvaluateAsync(string expression, RValueProperties fields, string repr = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            repr = repr ?? RValueRepresentations.Str();

            // Don't cache sessions since they can be disposed, expecially the debug session
            // when host is restarts or gets re-created in tests
            var rSession = _rSessionProvider.GetInteractiveWindowRSession();

            var frames = await rSession.TracebackAsync();
            if (frames == null || frames.Count == 0) {
                throw new InvalidOperationException("Debugger frames stack is empty");
            }
            return await frames.Last().TryEvaluateAndDescribeAsync(expression, fields, repr) as IRValueInfo;
        }
    }
}
