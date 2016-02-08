using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionEvaluation : IDisposable {
        IReadOnlyList<IRContext> Contexts { get; }
        Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind = REvaluationKind.Normal);
    }
}