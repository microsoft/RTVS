using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSessionEvaluation : IRSessionEvaluation {
        private readonly IRExpressionEvaluator _evaluator;
        private readonly TaskCompletionSource<object> _tcs;
        private readonly CancellationToken _ct;

        public IReadOnlyList<IRContext> Contexts { get; }
        public Task Task => _tcs.Task;

        public RSessionEvaluation(IReadOnlyList<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct) {
            Contexts = contexts;
            _evaluator = evaluator;
            _tcs = new TaskCompletionSource<object>();
            _ct = ct;
            ct.Register(() => _tcs.TrySetCanceled());
        }

        public void Dispose() {
            _tcs.TrySetResult(null);
        }

        public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind) {
            return _evaluator.EvaluateAsync(expression, kind, _ct);
        }
    }
}