// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IDataObjectEvaluator))]
    public sealed class DebugObjectEvaluator : IDataObjectEvaluator {
        private const string Repr = "rtvs:::make_repr_str()";
        private readonly IDebugSessionProvider _debugSessionProvider;
        private readonly IRSessionProvider _rSessionProvider;
        private DebugSession _debugSession;
        private IRSession _rSession;

        [ImportingConstructor]
        public DebugObjectEvaluator(IRSessionProvider rSessionProvider, IDebugSessionProvider debugSessionProvider) {
            _rSessionProvider = rSessionProvider;
            _debugSessionProvider = debugSessionProvider;
        }

        public async Task<DebugEvaluationResult> EvaluateAsync(string expression, DebugEvaluationResultFields fields, string repr = Repr) {
            await TaskUtilities.SwitchToBackgroundThread();

            var debugSession = await GetDebugSessionAsync();
            var frames = await debugSession.GetStackFramesAsync();
            if(frames == null || frames.Count == 0) {
                throw new InvalidOperationException("Debugger frames stack is empty");
            }
            return await frames.Last().EvaluateAsync(expression, fields, repr) as DebugValueEvaluationResult;
        }

        private async Task<DebugSession> GetDebugSessionAsync() {
            if (_debugSession == null) {
                if (_rSession == null) {
                    _rSession = _rSessionProvider.GetInteractiveWindowRSession();
                }
                _debugSession = await _debugSessionProvider.GetDebugSessionAsync(_rSession);
            }
            return _debugSession;
        }
    }
}
