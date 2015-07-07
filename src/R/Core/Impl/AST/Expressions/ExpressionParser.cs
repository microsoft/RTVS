using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.AST.Values;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Expressions
{
    /// <summary>
    /// Implements shunting yard algorithm of expression parsing.
    /// https://en.wikipedia.org/wiki/Shunting-yard_algorithm
    /// </summary>
    public sealed class ExpressionParser
    {
        private static readonly IOperator Sentinel = new TokenOperator(false);

        enum OperationType
        {
            None,
            UnaryOperator,
            BinaryOperator,
            Operand
        }

        private Stack<IAstNode> operands = new Stack<IAstNode>();
        private Stack<IOperator> operators = new Stack<IOperator>();
        private OperationType previousOperationType = OperationType.None;

        public IAstNode Parse(ParseContext context, IAstNode parent)
        {
            // https://en.wikipedia.org/wiki/Shunting-yard_algorithm
            // http://www.engr.mun.ca/~theo/Misc/exp_parsing.htm
            // Instead of evaluating expressions like calculator would do, 
            // we create tree nodes with operator and its operands.

            TokenStream<RToken> tokens = context.Tokens;
            OperationType currentOperationType = OperationType.None;
            ParseErrorType errorType = ParseErrorType.None;
            bool endOfExpression = false;
            IAstNode result = null;

            // Push sentinel
            this.operators.Push(ExpressionParser.Sentinel);

            while (!tokens.IsEndOfStream() && errorType == ParseErrorType.None && !endOfExpression)
            {
                RToken token = tokens.CurrentToken;

                switch (token.TokenType)
                {
                    // Terminal constants
                    case RTokenType.Number:
                    case RTokenType.Complex:
                    case RTokenType.Logical:
                    case RTokenType.String:
                    case RTokenType.Null:
                    case RTokenType.Missing:
                    case RTokenType.NaN:
                    case RTokenType.Infinity:
                        IAstNode constant = ExpressionParser.CreateConstant(context);
                        this.operands.Push(constant);
                        currentOperationType = OperationType.Operand;
                        break;

                    // Variables and function calls
                    case RTokenType.Identifier:
                        Variable variable = new Variable();
                        variable.Parse(context, null);
                        this.operands.Push(variable);
                        currentOperationType = OperationType.Operand;
                        break;

                    // Nested expression such as a*(b+c) or a nameless 
                    // function call like a[2](x, y) or func(a, b)(c, d)
                    case RTokenType.OpenBrace:

                        // Separate expression from function call. In case of 
                        // function call previous token is either closing indexer 
                        // brace or closing function brace. Identifier with brace 
                        // is handled up above.

                        if (tokens.PreviousToken.TokenType == RTokenType.CloseBrace ||
                            tokens.PreviousToken.TokenType == RTokenType.CloseSquareBracket ||
                            tokens.PreviousToken.TokenType == RTokenType.CloseSquareBracket ||
                            tokens.PreviousToken.TokenType == RTokenType.Identifier)
                        {
                            FunctionCall functionCall = new FunctionCall();
                            if (functionCall.Parse(context, null))
                            {
                                errorType = HandleFunctionOrIndexer(functionCall);
                                currentOperationType = OperationType.UnaryOperator;
                            }
                        }
                        else
                        {
                            Expression exp = new Expression();
                            if (exp.Parse(context, null))
                            {
                                this.operands.Push(exp);
                                currentOperationType = OperationType.Operand;
                            }
                        }

                        break;

                    case RTokenType.OpenSquareBracket:
                    case RTokenType.OpenDoubleSquareBracket:
                        Indexer indexer = new Indexer();
                        if (indexer.Parse(context, null))
                        {
                            errorType = HandleFunctionOrIndexer(indexer);
                            currentOperationType = OperationType.UnaryOperator;
                        }
                        else
                        {
                            return null;
                        }
                        break;

                    case RTokenType.Operator:
                        {
                            bool isUnary;
                            errorType = this.HandleOperator(context, null, out isUnary);
                            currentOperationType = isUnary ? OperationType.UnaryOperator : OperationType.BinaryOperator;
                        }
                        break;

                    case RTokenType.CloseBrace:
                    case RTokenType.CloseCurlyBrace:
                    case RTokenType.CloseSquareBracket:
                    case RTokenType.CloseDoubleSquareBracket:
                    case RTokenType.Comma:
                    case RTokenType.Semicolon:
                    case RTokenType.Keyword:
                        endOfExpression = true;
                        break;

                    default:
                        errorType = ParseErrorType.UnexpectedToken;
                        break;

                }

                if (errorType != ParseErrorType.None)
                {
                    break;
                }

                if (this.previousOperationType == currentOperationType)
                {
                    // 'operator, operator' or 'identifier identifier' sequence is an error
                    switch (currentOperationType)
                    {
                        case OperationType.Operand:
                            context.Errors.Add(new MissingItemParseError(ParseErrorType.OperatorExpected, tokens.PreviousToken));
                            break;

                        case OperationType.None:
                            if (tokens.IsEndOfStream())
                            {
                                context.Errors.Add(new ParseError(ParseErrorType.UnexpectedEndOfFile, ParseErrorLocation.AfterToken, tokens.PreviousToken));
                            }
                            else
                            {
                                context.Errors.Add(new ParseError(ParseErrorType.UnexpectedToken, ParseErrorLocation.Token, tokens.CurrentToken));
                            }
                            break;

                        default:
                            context.Errors.Add(new MissingItemParseError(ParseErrorType.OperandExpected, tokens.PreviousToken));
                            break;
                    }

                    return null;
                }
                else if (this.previousOperationType == OperationType.UnaryOperator && currentOperationType == OperationType.BinaryOperator)
                {
                    // unary followed by binary doesn't make sense 
                    context.Errors.Add(new MissingItemParseError(ParseErrorType.IndentifierExpected, tokens.PreviousToken));
                    return null;
                }
            }

            if (errorType == ParseErrorType.None && this.operators.Count > 1)
            {
                // If there are still operators to process,
                // construct final node. After this only sentil
                // operator should be in the operators stack
                // and a single final root node in the operand stack.
                errorType = this.ProcessHigherPrecendenceOperators(ExpressionParser.Sentinel);
            }

            if (errorType != ParseErrorType.None)
            {
                context.Errors.Add(new MissingItemParseError(errorType, tokens.PreviousToken));
                return null;
            }

            Debug.Assert(this.operators.Count == 1);

            // If operand stack ie empty and there is no error
            // then the expression is empty.
            if (this.operands.Count > 0)
            {
                Debug.Assert(this.operands.Count == 1);

                result = this.operands.Pop();
                parent.AppendChild(result);
            }

            return result;
        }

        private ParseErrorType HandleFunctionOrIndexer(IAstNode operatorNode)
        {
            // Indexing or function call is performed on the topmost operand which 
            // generally should be a variable or a node that evaluates to it.
            // However, we leave syntax check to separate code.

            IAstNode operand = this.SafeGetOperand();
            if (operand == null)
            {
                // Oddly, no operand
                return ParseErrorType.IndentifierExpected;
            }

            operatorNode.AppendChild(operand);
            this.operands.Push(operatorNode);

            return ParseErrorType.None;
        }

        private ParseErrorType HandleOperator(ParseContext context, IAstNode parent, out bool isUnary)
        {
            ParseErrorType errorType = ParseErrorType.None;

            // If operands stack is empty operator is unary.
            // If operator is preceded by another operator, 
            // it is interpreted as unary.

            TokenOperator currentOperator = new TokenOperator(this.operands.Count == 0);
            currentOperator.Parse(context, null);

            IOperator lastOperator = this.operators.Peek();
            if (currentOperator.Precedence <= lastOperator.Precedence)
            {
                // New operator has lower or equal precedence. We need to make a tree from
                // the topmost operator and its operand(s). Example: a*b+c. + has lower priority
                // and a and b should be on the stack along with * on the operator stack.
                // Repeat until there are no more higher precendece operators on the stack.

                errorType = this.ProcessHigherPrecendenceOperators(currentOperator);
            }

            this.operators.Push(currentOperator);
            isUnary = currentOperator.IsUnary;

            return errorType;
        }

        private ParseErrorType ProcessHigherPrecendenceOperators(IOperator currentOperator)
        {
            Debug.Assert(this.operators.Count > 1);

            // At least one operator above sentinel is on the stack.
            do
            {
                IOperator operatorNode = this.operators.Pop();

                IAstNode rightOperand = this.SafeGetOperand();
                if (rightOperand == null)
                {
                    // Oddly, no operands
                    return ParseErrorType.OperandExpected;
                }

                if (operatorNode.IsUnary)
                {
                    operatorNode.AppendChild(rightOperand);
                    operatorNode.RightOperand = rightOperand;
                }
                else
                {
                    IAstNode leftOperand = this.SafeGetOperand();
                    if (leftOperand == null)
                    {
                        return ParseErrorType.OperandExpected;
                    }

                    operatorNode.LeftOperand = leftOperand;
                    operatorNode.RightOperand = rightOperand;

                    operatorNode.AppendChild(leftOperand);
                    operatorNode.AppendChild(rightOperand);
                }

                this.operands.Push(operatorNode);

                IOperator nextOperatorNode = this.operators.Peek();
                if (nextOperatorNode.Precedence <= currentOperator.Precedence)
                {
                    break;
                }

            } while (this.operators.Count > 1);

            return ParseErrorType.None;
        }

        private IAstNode SafeGetOperand()
        {
            return this.operands.Count > 0 ? this.operands.Pop() : null;
        }

        private static IAstNode CreateConstant(ParseContext context)
        {
            TokenStream<RToken> tokens = context.Tokens;
            RToken currentToken = tokens.CurrentToken;
            IAstNode term = null;

            switch (currentToken.TokenType)
            {
                case RTokenType.Number:
                    term = new NumericalValue();
                    break;

                case RTokenType.Complex:
                    term = new ComplexValue();
                    break;

                case RTokenType.Logical:
                    term = new LogicalValue();
                    break;

                case RTokenType.String:
                    term = new StringValue();
                    break;

                case RTokenType.Null:
                    term = new NullValue();
                    break;

                case RTokenType.Missing:
                    term = new MissingValue();
                    break;
            }

            Debug.Assert(term != null);
            term.Parse(context, null);
            return term;
        }
    }
}
