using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Functions;
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
        private static readonly IOperator Sentinel = new TokenOperator(OperatorType.Sentinel, false);

        enum OperationType
        {
            None,
            UnaryOperator,
            BinaryOperator,
            Operand,
            Function
        }

        private Stack<IAstNode> _operands = new Stack<IAstNode>();
        private Stack<IOperator> _operators = new Stack<IOperator>();
        private OperationType _previousOperationType = OperationType.None;

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
            _operators.Push(ExpressionParser.Sentinel);

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
                        _operands.Push(constant);
                        currentOperationType = OperationType.Operand;
                        break;

                    // Variables and function calls
                    case RTokenType.Identifier:
                        Variable variable = new Variable();
                        variable.Parse(context, null);
                        _operands.Push(variable);
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
                                currentOperationType = OperationType.Function;
                            }
                        }
                        else
                        {
                            Expression exp = new Expression();
                            if (exp.Parse(context, null))
                            {
                                _operands.Push(exp);
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
                            currentOperationType = OperationType.Function;
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
                        if (_previousOperationType == OperationType.None)
                        {
                            context.Errors.Add(new ParseError(ParseErrorType.UnexpectedToken, ParseErrorLocation.Token, tokens.CurrentToken));
                        }
                        endOfExpression = true;
                        break;

                    case RTokenType.Comma:
                    case RTokenType.Semicolon:
                        endOfExpression = true;
                        break;

                    case RTokenType.Keyword:
                        errorType = HandleKeyword(context);
                        if (errorType == ParseErrorType.None)
                        {
                            currentOperationType = OperationType.Operand;
                        }
                        endOfExpression = true;
                        break;

                    default:
                        errorType = ParseErrorType.UnexpectedToken;
                        break;
                }

                if (errorType != ParseErrorType.None || endOfExpression)
                {
                    break;
                }

                if (_previousOperationType == currentOperationType && currentOperationType != OperationType.Function)
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
                else if (_previousOperationType == OperationType.UnaryOperator && currentOperationType == OperationType.BinaryOperator)
                {
                    // unary followed by binary doesn't make sense 
                    context.Errors.Add(new MissingItemParseError(ParseErrorType.IndentifierExpected, tokens.PreviousToken));
                    return null;
                }

                _previousOperationType = currentOperationType;

                // In R there may not be explicit end of statement.
                // Semicolon is optional and R console figures out if there is
                // continuation from the context. For example, if statement is
                // incomplete such as brace is not closed or last token in 
                // the line is an operator, it continues with the next line.
                // However, in 'x + 1 <line_break> + y' it stops expression
                // parsing at the line break.

                if (!endOfExpression)
                {
                    if (currentOperationType == OperationType.Function || (currentOperationType == OperationType.Operand && _previousOperationType != OperationType.None))
                    {
                        // Since we haven't seen explicit end of expression and 
                        // the last operation was 'operand' which is a variable 
                        // or a constant and there is a line break ahead of us
                        // then the expression is complete. Outer parser may still
                        // continue if braces are not closed yet.
                        if (context.Tokens.IsLineBreakAfter(context.TextProvider, context.Tokens.Position - 1))
                        {
                            endOfExpression = true;
                        }
                    }
                }
            }

            if (errorType == ParseErrorType.None && _operators.Count > 1)
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

            Debug.Assert(_operators.Count == 1);

            // If operand stack ie empty and there is no error
            // then the expression is empty.
            if (_operands.Count > 0)
            {
                Debug.Assert(_operands.Count == 1);

                result = _operands.Pop();
                parent.AppendChild(result);
            }

            return result;
        }

        private ParseErrorType HandleKeyword(ParseContext context)
        {
            ParseErrorType errorType = ParseErrorType.None;

            string keyword = context.TextProvider.GetText(context.Tokens.CurrentToken);
            if (keyword.Equals("function", StringComparison.Ordinal))
            {
                // Special case 'exp <- function(...) { }
                FunctionDefinition funcDef = new FunctionDefinition();
                if (funcDef.Parse(context, null))
                {
                    _operands.Push(funcDef);
                }
                else
                {
                    errorType = ParseErrorType.FunctionExpected;
                }
            }
            else
            {
                errorType = ParseErrorType.UnexpectedToken;
            }

            return errorType;
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
            _operands.Push(operatorNode);

            return ParseErrorType.None;
        }

        private ParseErrorType HandleOperator(ParseContext context, IAstNode parent, out bool isUnary)
        {
            ParseErrorType errorType = ParseErrorType.None;

            // If operands stack is empty operator is unary.
            // If operator is preceded by another operator, 
            // it is interpreted as unary.

            TokenOperator currentOperator = new TokenOperator(_operands.Count == 0);

            currentOperator.Parse(context, null);
            isUnary = currentOperator.IsUnary;

            IOperator lastOperator = _operators.Peek();
            if (currentOperator.Precedence <= lastOperator.Precedence &&
                !(currentOperator.OperatorType == lastOperator.OperatorType && currentOperator.Association == Association.Right))
            {
                // New operator has lower or equal precedence. We need to make a tree from
                // the topmost operator and its operand(s). Example: a*b+c. + has lower priority
                // and a and b should be on the stack along with * on the operator stack.
                // Repeat until there are no more higher precendece operators on the stack.

                errorType = this.ProcessHigherPrecendenceOperators(currentOperator);
            }

            if (errorType == ParseErrorType.None)
            {
                _operators.Push(currentOperator);
            }

            return errorType;
        }

        private ParseErrorType ProcessHigherPrecendenceOperators(IOperator currentOperator)
        {
            Debug.Assert(_operators.Count > 1);
            ParseErrorType errorType = ParseErrorType.None;
            Association association = currentOperator.Association;

            // At least one operator above sentinel is on the stack.
            do
            {
                errorType = MakeNode();
                if (errorType == ParseErrorType.None)
                {
                    IOperator nextOperatorNode = _operators.Peek();

                    if (association == Association.Left && nextOperatorNode.Precedence <= currentOperator.Precedence)
                    {
                        break;
                    }

                    if (association == Association.Right && nextOperatorNode.Precedence < currentOperator.Precedence)
                    {
                        break;
                    }
                }
            } while (_operators.Count > 1 && errorType == ParseErrorType.None);

            return errorType;
        }

        private ParseErrorType MakeNode()
        {
            IOperator operatorNode = _operators.Pop();

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

            _operands.Push(operatorNode);

            return ParseErrorType.None;
        }

        private IAstNode SafeGetOperand()
        {
            return _operands.Count > 0 ? _operands.Pop() : null;
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
