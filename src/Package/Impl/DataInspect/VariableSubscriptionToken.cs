using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Token to distinguish variable subscription
    /// </summary>
    internal class VariableSubscriptionToken : IEquatable<VariableSubscription> {
        public VariableSubscriptionToken(string environmentExpression, string variableExpression) {
            Environment = environmentExpression;
            Expression = variableExpression;
        }

        /// <summary>
        /// R expression to evaluate environment.
        /// </summary>
        public string Environment { get; }

        /// <summary>
        /// R expression to evaluate variable in environment 
        /// </summary>
        public string Expression { get; }

        public bool Equals(VariableSubscription other) {
            if (other == null) {
                return false;
            }

            return Environment == other.Environment && Expression == other.Expression;
        }
    }
}
