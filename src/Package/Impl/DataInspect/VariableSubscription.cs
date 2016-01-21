using System;
using System.Reflection;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Variable subscription from <see cref="VariableProvider"/>
    /// </summary>
    internal sealed class VariableSubscription : IDisposable {
        WeakReference _weakReference;
        private readonly MethodInfo _method;
        private readonly Type _delegateType;

        Action<VariableSubscription> _unsubscribe;

        public VariableSubscription(
            VariableSubscriptionToken token,
            Action<DebugEvaluationResult> executeAction,
            Action<VariableSubscription> unsubscribeAction) {

            Token = token;

            _weakReference = new WeakReference(executeAction.Target);
            _method = executeAction.Method;
            _delegateType = executeAction.GetType();

            _unsubscribe = unsubscribeAction;
        }

        internal VariableSubscriptionToken Token { get; }

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
        }

        public Action<DebugEvaluationResult> GetExecuteAction() {
            if (_method.IsStatic) {
                return (Action<DebugEvaluationResult>)Delegate.CreateDelegate(_delegateType, null, _method);
            }
            object target = _weakReference.Target;
            if (target != null) {
                return (Action<DebugEvaluationResult>)Delegate.CreateDelegate(_delegateType, target, _method, false);
            }
            return null;
        }
    }
}
