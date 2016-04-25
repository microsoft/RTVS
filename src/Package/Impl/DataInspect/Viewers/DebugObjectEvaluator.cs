// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IDebugObjectEvaluator))]
    public sealed class DebugObjectEvaluator : IDebugObjectEvaluator {
        private readonly IDebugSessionProvider _debugSessionProvider;
        private readonly IRSessionProvider _rSessionProvider;
        private DebugSession _debugSession;
        private IRSession _rSession;

        [ImportingConstructor]
        public DebugObjectEvaluator(IRSessionProvider rSessionProvider, IDebugSessionProvider debugSessionProvider) {
            _rSessionProvider = rSessionProvider;
            _debugSessionProvider = debugSessionProvider;
        }

        public async Task<DebugEvaluationResult> EvaluateAsync(string expression, DebugEvaluationResultFields fields) {
            await TaskUtilities.SwitchToBackgroundThread();
            var frame = await GetFrameAsync();
            return await frame?.EvaluateAsync(expression, fields) as DebugValueEvaluationResult;
        }

        private async Task<DebugStackFrame> GetFrameAsync(int index = 0) {
            var debugSession = await GetDebugSessionAsync();
            var frames = await debugSession.GetStackFramesAsync();
            return frames.FirstOrDefault(f => f.Index == index);
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
