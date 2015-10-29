namespace Microsoft.R.Core.AST.Operators {

    public static class OperatorAssociation {

        public static Association GetAssociation(OperatorType operatorType) {
            switch (operatorType) {
                case OperatorType.Exponent:     // ^
                case OperatorType.Equals:       // =
                case OperatorType.LeftAssign:   // <- or <<-
                case OperatorType.FunctionCall: // ()
                case OperatorType.Index:        // [] [[]]
                    return Association.Right;
            }

            return Association.Left;
        }
    }
}
