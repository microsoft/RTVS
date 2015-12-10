using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSessionEvaluationSource {
        private readonly TaskCompletionSource<IRSessionEvaluation> _tcs;

        public bool IsMutating { get; }

        public RSessionEvaluationSource(bool isMutating) {
            _tcs = new TaskCompletionSource<IRSessionEvaluation>();
            IsMutating = isMutating;
        }

        public Task<IRSessionEvaluation> Task => _tcs.Task;

        public Task BeginEvaluationAsync(IReadOnlyList<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct) {
            var evaluation = new RSessionEvaluation(contexts, evaluator, ct);
            _tcs.SetResult(evaluation);
            return evaluation.Task;
        }

        public bool TryCancel() {
            return _tcs.TrySetCanceled();
        }
    }
}