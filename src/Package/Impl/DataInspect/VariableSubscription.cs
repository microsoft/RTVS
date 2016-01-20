using System;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Variable subscription from <see cref="VariableProvider"/>
    /// </summary>
    internal sealed class VariableSubscription : IDisposable {
        Action<DebugEvaluationResult> _execute;
        Action<VariableSubscription> _unsubscribe;

        public VariableSubscription(
            VariableSubscriptionToken token,
            Action<DebugEvaluationResult> executeAction,
            Action<VariableSubscription> unsubscribeAction) {

            Token = token;
            _execute = executeAction;   // TODO: use weak reference
            _unsubscribe = unsubscribeAction;
        }

        internal VariableSubscriptionToken Token { get; }

        /// <summary>
        /// R expression to evaluate environment.
        /// </summary>
        public string Environment {
            get {
                return Token.Environment;
            }
        }

        /// <summary>
        /// R expression to evaluate variable in environment 
        /// </summary>
        public string Expression {
            get {
                return Token.Expression;
            }
        }

        public void Dispose() {
            if (_unsubscribe != null) {
                _unsubscribe(this);
                _unsubscribe = null;
            }

            GC.SuppressFinalize(this);
        }

        public Action<DebugEvaluationResult> GetExecuteAction() {
            return _execute;
        }
    }
}
