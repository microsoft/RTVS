using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Core.AST.Operators
{
    public enum OperatorType
    {
        Unknown,
        Add,
        Subtract,
        Multiply,
        Divide,
        Exponent, // ^
        Tilde, // ~ (formula)
        Equals, // =
        Modulo, // %%
        IntegerDivide, // %/%
        MatrixProduct, // %*%
        OuterProduct, // %o%
        KroneckerProduct, // %x%
        MatchingPperator, // %in%
        Special, // %abc%
        GreaterThan,
        GreaterThanOrEquals,
        LessThan,
        LessThanOrEquals,
        ListIndex, // $
        Sequence, // :
        Not, // !
        And, // &
        Or, // |
        ConditionalAnd, // &&
        CondtitionalOr, // ||
        ConditionalEquals, // ==
        ConditionalNotEquals, // !=
        Namespace, // ::, :::
        Index, // [ ]
        FunctionCall, // ()
        LeftAssign, // <- <<-
        RightAssign, // -> ->>
        Unary, // pseudo-type since unary-ness is defined from context
        Sentinel // pseudo-type used in expression parsing
    }
}
