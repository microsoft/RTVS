using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Token to distinguish variable subscription
    /// </summary>
    internal class VariableSubscriptionToken : IEquatable<VariableSubscriptionToken> {
        public VariableSubscriptionToken(int frameIndex, string variableExpression) {
            FrameIndex = frameIndex;
            Expression = variableExpression;
        }

        /// <summary>
        /// frame index, global environment is 0
        /// </summary>
        public int FrameIndex { get; }

        /// <summary>
        /// R expression to evaluate variable in environment 
        /// </summary>
        public string Expression { get; }

        public bool Equals(VariableSubscriptionToken other) {
            if (other == null) {
                return false;
            }

            return FrameIndex == other.FrameIndex && Expression == other.Expression;
        }
    }
}
