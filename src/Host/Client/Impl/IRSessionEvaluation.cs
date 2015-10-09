using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionEvaluation : IDisposable {
        IReadOnlyList<IRContext> Contexts { get; }
        /// <param name="reentrant">
        /// If <c>true</c>, nested evaluations are possible if R transitions to the state allowing evaluation
        /// while evaluating this expression. Otherwise, no nested evaluations are possible.
        /// </param>
        Task<REvaluationResult> EvaluateAsync(string expression, bool reentrant);
    }
}