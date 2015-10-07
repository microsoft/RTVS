using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl.Session {
    internal sealed class RSessionEvaluationSource {
        private readonly TaskCompletionSource<IRSessionEvaluation> _tcs;

        public RSessionEvaluationSource() {
            _tcs = new TaskCompletionSource<IRSessionEvaluation>();
        }

        public Task<IRSessionEvaluation> Task => _tcs.Task;

        public Task BeginEvaluationAsync(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct) {
            var evaluation = new RSessionEvaluation(contexts, evaluator, ct);
            _tcs.SetResult(evaluation);
            return evaluation.Task;
        }

        public bool TryCancel() {
            return _tcs.TrySetCanceled();
        }
    }
}