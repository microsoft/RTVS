using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl
{
    internal class RSessionInteraction : IRSessionInteraction, IRExpressionEvaluator
    {
        private readonly TaskCompletionSource<string> _requestTcs;
        private readonly TaskCompletionSource<string> _responseTcs;
        private IRExpressionEvaluator _expressionEvaluator;

        public string Prompt { get; }
        public int MaxLength { get; }
        public IReadOnlyCollection<IRContext> Contexts { get; }

        public RSessionInteraction(
            TaskCompletionSource<string> requestTcs,
            TaskCompletionSource<string> responseTcs,
            string prompt,
            int maxLength,
            IReadOnlyCollection<IRContext> contexts,
            IRExpressionEvaluator evaluator)
        {
            _requestTcs = requestTcs;
            _responseTcs = responseTcs;
            _expressionEvaluator = evaluator;
            Prompt = prompt;
            MaxLength = maxLength;
            Contexts = contexts;
        }

        public Task<string> RespondAsync(string messageText)
        {
            _expressionEvaluator = null;
            _requestTcs.SetResult(messageText);
            return _responseTcs.Task;
        }

        public Task<REvaluationResult> EvaluateAsync(string expression) {
            if (_expressionEvaluator == null) {
                throw new InvalidOperationException("EvaluateAsync cannot be used after RespondAsync was invoked.");
            }

            return _expressionEvaluator.EvaluateAsync(expression);
        }
    }
}