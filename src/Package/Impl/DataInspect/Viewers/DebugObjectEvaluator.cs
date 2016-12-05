// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.DataInspection;
using Microsoft.R.StackTracing;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IDataObjectEvaluator))]
    public sealed class DebugObjectEvaluator : IDataObjectEvaluator {
        private readonly IRInteractiveWorkflow _workflow;

        [ImportingConstructor]
        public DebugObjectEvaluator(IRInteractiveWorkflowProvider workflowProvider) {
            _workflow = workflowProvider.GetOrCreate();
        }

        public async Task<IREvaluationResultInfo> EvaluateAsync(string expression, REvaluationResultProperties fields, string repr, CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            repr = repr ?? RValueRepresentations.Str();

            // Don't cache sessions since they can be disposed, expecially the debug session
            // when host is restarts or gets re-created in tests
            var rSession = _workflow.RSession;

            var frames = await rSession.TracebackAsync(cancellationToken:cancellationToken);
            if (frames == null || frames.Count == 0) {
                throw new InvalidOperationException("Debugger frames stack is empty");
            }
            return await frames.Last().TryEvaluateAndDescribeAsync(expression, fields, repr, cancellationToken) as IRValueInfo;
        }
    }
}
