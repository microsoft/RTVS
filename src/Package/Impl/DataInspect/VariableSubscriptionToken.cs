using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Token to distinguish variable subscription
    /// </summary>
    public class VariableSubscriptionToken : IEquatable<VariableSubscriptionToken> {
        public VariableSubscriptionToken(int frameIndex, string variableExpression) {
            if (variableExpression == null) {
                throw new ArgumentNullException("variableExpression");
            }
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

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as VariableSubscriptionToken);
        }

        public override int GetHashCode() {
            return FrameIndex.GetHashCode() ^ Expression.GetHashCode();
        }
    }
}
