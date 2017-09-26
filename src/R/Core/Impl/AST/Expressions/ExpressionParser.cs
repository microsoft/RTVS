// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Operands;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Values;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Expressions {
    /// <summary>
    /// Implements shunting yard algorithm of expression parsing.
    /// https://en.wikipedia.org/wiki/Shunting-yard_algorithm
    /// </summary>
    public sealed partial class Expression {
        private static readonly IOperator Sentinel = new TokenOperator(OperatorType.Sentinel, unary: false);

        /// <summary>
        /// Describes current and previous operation types
        /// in the current expression. Helps to detect
        /// errors like missing operands or operators.
        /// </summary>
        enum OperationType {
            None,
            UnaryOperator,
            BinaryOperator,
            Operand,
            Function,
            EndOfExpression
        }

        private readonly Stack<IRValueNode> _operands = new Stack<IRValueNode>();
        private readonly Stack<IOperator> _operators = new Stack<IOperator>();
        private OperationType _previousOperationType = OperationType.None;

        private bool ParseExpression(ParseContext context) {
            // https://en.wikipedia.org/wiki/Shunting-yard_algorithm
            // http://www.engr.mun.ca/~theo/Misc/exp_parsing.htm
            // Instead of evaluating expressions like calculator would do, 
            // we create tree nodes with operator and its operands.

            var tokens = context.Tokens;
            var currentOperationType = OperationType.None;
            var errorType = ParseErrorType.None;
            var errorLocation = ErrorLocation.AfterToken;
            var endOfExpression = false;

            // Push sentinel
            _operators.Push(Expression.Sentinel);

            while (!tokens.IsEndOfStream() && errorType == ParseErrorType.None && !endOfExpression) {
                var token = tokens.CurrentToken;

                switch (token.TokenType) {
                    // Terminal constants
                    case RTokenType.Number:
                    case RTokenType.Complex:
                    case RTokenType.Logical:
                    case RTokenType.String:
                    case RTokenType.Null:
                    case RTokenType.Missing:
                    case RTokenType.NaN:
                    case RTokenType.Infinity:
                        currentOperationType = HandleConstant(context);
                        break;

                    // Variables and function calls
                    case RTokenType.Identifier:
                        if (context.ExpressionTermFilter.IsInertRange(context.Tokens.CurrentToken)) {
                            // This is a workaround for constructs like ```{r x = 1, y = FALSE} where the { }
                            // block is treated as R fragment. The fragment is syntactually incorrect since
                            // 'r' is indentifier and there is an operator expected between 'r' and 'x'.
                            // In order to avoid parsing errors expression parser will use this flag and
                            // allow standalone indentifier 'r' or 'R' right after the opening curly brace.
                            context.Tokens.MoveToNextToken();
                            continue;
                        }
                        currentOperationType = HandleIdentifier(context);
                        break;

                    case RTokenType.Ellipsis:
                        currentOperationType = HandleIdentifier(context);
                        break;

                    // Nested expression such as a*(b+c) or a nameless 
                    // function call like a[2](x, y) or func(a, b)(c, d)
                    case RTokenType.OpenBrace:
                        currentOperationType = HandleOpenBrace(context, out errorType);
                        break;

                    case RTokenType.OpenCurlyBrace:
                        currentOperationType = HandleLambda(context, out errorType);
                        break;

                    case RTokenType.OpenSquareBracket:
                    case RTokenType.OpenDoubleSquareBracket:
                        currentOperationType = HandleSquareBrackets(context, out errorType);
                        break;

                    case RTokenType.Operator:
                        currentOperationType = HandleOperator(context, out errorType);
                        break;

                    case RTokenType.CloseBrace:
                        currentOperationType = OperationType.EndOfExpression;
                        endOfExpression = true;
                        break;

                    case RTokenType.CloseCurlyBrace:
                    case RTokenType.CloseSquareBracket:
                    case RTokenType.CloseDoubleSquareBracket:
                        currentOperationType = OperationType.EndOfExpression;
                        endOfExpression = true;
                        break;

                    case RTokenType.Comma:
                    case RTokenType.Semicolon:
                        currentOperationType = OperationType.EndOfExpression;
                        endOfExpression = true;
                        break;

                    case RTokenType.Keyword:
                        currentOperationType = HandleKeyword(context, out errorType);
                        endOfExpression = true;
                        break;

                    default:
                        errorType = ParseErrorType.UnexpectedToken;
                        errorLocation = ErrorLocation.Token;
                        break;
                }

                if (errorType == ParseErrorType.None && !IsConsistentOperationSequence(context, currentOperationType)) {
                    return false;
                }

                if (errorType != ParseErrorType.None || endOfExpression) {
                    break;
                }

                _previousOperationType = currentOperationType;
                endOfExpression = CheckEndOfExpression(context, currentOperationType);
            }

            if (errorType != ParseErrorType.None) {
                if (errorLocation == ErrorLocation.AfterToken) {
                    context.AddError(new ParseError(errorType, ErrorLocation.AfterToken, tokens.PreviousToken));
                } else {
                    context.AddError(new ParseError(errorType, ErrorLocation.Token, tokens.CurrentToken));
                }
            }

            if (_operators.Count > 1) {
                // If there are still operators to process, construct the final subtree. 
                // After that only the sentinel operator should be in the operators stack
                // and a single final root node in the operand stack.
                errorType = this.ProcessHigherPrecendenceOperators(context, Expression.Sentinel);
            }

            if (errorType != ParseErrorType.None) {
                if (errorType != ParseErrorType.LeftOperandExpected) {
                    context.AddError(new ParseError(errorType, ErrorLocation.Token, GetErrorRange(context)));
                }

                // Although there may be errors such as incomplete function
                // we still want to include the result into the tree since
                // even in the code like 'func(,,, for(' we want intellisense
                // and parameter to work.
                if (_operands.Count == 1) {
                    Content = _operands.Pop();
                    AppendChild(this.Content);
                }
            } else {
                Debug.Assert(_operators.Count == 1);

                // If operand stack ie empty and there is no error
                // then the expression is empty.
                if (_operands.Count > 0) {
                    Debug.Assert(_operands.Count == 1);

                    Content = _operands.Pop();
                    AppendChild(Content);
                }
            }
            return true;
        }

        private bool CheckEndOfExpression(ParseContext context, OperationType currentOperationType) {
            // In R there may not be explicit end of statement. Semicolon is optional and 
            // R console figures out if there is continuation from the context. For example, 
            // if statement is incomplete such as brace is not closed or last token in  the line 
            // is an operator, it continues with the next line. However, 
            // in 'x + 1 <line_break> + y' it stops expression parsing at the line break.

            if (currentOperationType == OperationType.Function || (currentOperationType == OperationType.Operand && _previousOperationType != OperationType.None)) {
                // Since we haven't seen explicit end of expression and the last operation
                // was 'operand' which is a variable or a constant and there is a line break
                // ahead of us then the expression is complete. Outer parser may still continue
                // if braces are not closed yet.

                if (!IsInGroup || context.Tokens.CurrentToken.TokenType == RTokenType.CloseBrace) {
                    if (context.Tokens.IsLineBreakAfter(context.TextProvider, context.Tokens.Position - 1)) {
                        // There is a line break before this token
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsConsistentOperationSequence(ParseContext context, OperationType currentOperationType) {
            // Binary operator followed by another binary like 'a +/ b' is an error.
            // 'func()()' or 'func() +' is allowed but 'func() operand' is not. 
            // Binary operator without anything behind it is an error;
            if ((_previousOperationType == OperationType.Function && currentOperationType == OperationType.Operand) ||
                (_previousOperationType == currentOperationType && currentOperationType != OperationType.Function)) {
                switch (currentOperationType) {
                    case OperationType.Operand:
                        // 'operand operand' sequence is an error
                        context.AddError(new ParseError(ParseErrorType.OperatorExpected, ErrorLocation.Token, GetOperandErrorRange(context)));
                        break;

                    case OperationType.UnaryOperator:
                        // 'unary unary' and 'binary unary' are OK
                        return true;

                    default:
                        // 'operator operator' sequence is an error
                        context.AddError(new ParseError(ParseErrorType.RightOperandExpected, ErrorLocation.Token, GetOperatorErrorRange(context)));
                        break;
                }

                return false;
            }
            if (currentOperationType == OperationType.BinaryOperator && context.Tokens.IsEndOfStream()) {
                // 'operator <EOF>' sequence is an error
                context.AddError(new ParseError(ParseErrorType.RightOperandExpected, ErrorLocation.Token, GetOperatorErrorRange(context)));
                return false;
            }
            if (_previousOperationType == OperationType.UnaryOperator && currentOperationType == OperationType.BinaryOperator) {
                // unary followed by binary doesn't make sense 
                context.AddError(new ParseError(ParseErrorType.IndentifierExpected, ErrorLocation.Token, GetOperatorErrorRange(context)));
                return false;
            }
            if (_previousOperationType == OperationType.BinaryOperator && currentOperationType == OperationType.EndOfExpression) {
                // missing list selector: z$ }
                context.AddError(new ParseError(ParseErrorType.RightOperandExpected, ErrorLocation.Token, GetErrorRange(context)));
                return false;
            }

            return true;
        }

        private ITextRange GetErrorRange(ParseContext context) {
            return context.Tokens.IsEndOfStream() ? context.Tokens.PreviousToken : context.Tokens.CurrentToken;
        }

        private ITextRange GetIndexerOrFunctionErrorRange(ParseContext context, IOperator operatorNode) {
            ITextRange range = null;
            if (operatorNode is Indexer) {
                range = ((Indexer)operatorNode).LeftBrackets;
            } else if (operatorNode is FunctionCall) {
                range = ((FunctionCall)operatorNode).OpenBrace;
            }

            if (range == null) {
                range = context.Tokens.FirstOrDefault((x) => x.Start >= operatorNode.End);
            }

            if (range == null) {
                range = operatorNode;
            }

            return range;
        }

        private ITextRange GetOperandErrorRange(ParseContext context) {
            if (_operands.Count > 0) {
                IAstNode node = _operands.Peek();
                if (node.Children.Count > 0) {
                    return node.Children[0];
                }
                return node;
            }

            return GetErrorRange(context);
        }

        private ITextRange GetOperatorErrorRange(ParseContext context) {
            if (_operators.Count > 0) {
                IAstNode node = _operators.Peek();
                if (node.Children.Count > 0) {
                    return node.Children[0];
                }
                return node;
            }

            return GetErrorRange(context);
        }

        private OperationType HandleConstant(ParseContext context) {
            var constant = Expression.CreateConstant(context);
            _operands.Push(constant);
            return OperationType.Operand;
        }

        private OperationType HandleIdentifier(ParseContext context) {
            var variable = new Variable();
            variable.Parse(context, null);

            _operands.Push(variable);
            return OperationType.Operand;
        }

        private OperationType HandleLambda(ParseContext context, out ParseErrorType errorType) {
            errorType = ParseErrorType.None;

            var lambda = new Lambda();
            lambda.Parse(context, null);

            _operands.Push(lambda);
            return OperationType.Operand;
        }

        private OperationType HandleOpenBrace(ParseContext context, out ParseErrorType errorType) {
            var tokens = context.Tokens;
            errorType = ParseErrorType.None;

            // Separate expression from function call. In case of 
            // function call previous token is either closing indexer 
            // brace or closing function brace. Identifier with brace 
            // is handled up above. 
            // Indentifier followed by a brace needs to be analyzed
            // so we can tell between previous expression that ended
            // with identifier and identifier that is a function name:
            //
            //      a <- 2*b
            //      (expression)
            //
            // in this case b is not a function name. Similarly,
            //
            //      a <- 2*b[1]
            //      (expression)
            //
            // is not a function call operator over b[1].

            if (_operators.Count > 1 || _operands.Count > 0) {
                // We are not in the beginning of the expression
                if (tokens.PreviousToken.TokenType == RTokenType.CloseBrace ||
                    tokens.PreviousToken.TokenType == RTokenType.CloseSquareBracket ||
                    tokens.PreviousToken.TokenType == RTokenType.CloseDoubleSquareBracket ||
                    tokens.PreviousToken.IsVariableKind()) {
                    var functionCall = new FunctionCall();
                    functionCall.Parse(context, null);

                    errorType = HandleOperatorPrecedence(context, functionCall);
                    return OperationType.Function;
                }
            }

            var group = new Group();
            group.Parse(context, null);

            _operands.Push(group);
            return OperationType.Operand;
        }

        private OperationType HandleSquareBrackets(ParseContext context, out ParseErrorType errorType) {
            var indexer = new Indexer();
            indexer.Parse(context, null);

            errorType = HandleOperatorPrecedence(context, indexer);
            return OperationType.Function;
        }

        private OperationType HandleOperator(ParseContext context, out ParseErrorType errorType) {
            errorType = HandleOperator(context, out OperationType operationType);
            return operationType;
        }

        private OperationType HandleKeyword(ParseContext context, out ParseErrorType errorType) {
            errorType = ParseErrorType.None;

            var keyword = context.TextProvider.GetText(context.Tokens.CurrentToken);
            if (IsTerminatingKeyword(keyword)) {
                return OperationType.EndOfExpression;
            }

            errorType = HandleKeyword(context, keyword);
            if (errorType == ParseErrorType.None) {
                return OperationType.Operand;
            }

            return _previousOperationType;
        }

        private ParseErrorType HandleKeyword(ParseContext context, string keyword) {
            var errorType = ParseErrorType.None;

            if (keyword.Equals("function", StringComparison.Ordinal)) {
                // Special case 'exp <- function(...) { }
                var funcDef = new FunctionDefinition();
                funcDef.Parse(context, null);

                // Add to the stack even if it has errors in order
                // to avoid extra errors
                _operands.Push(funcDef);
            } else if (keyword.Equals("if", StringComparison.Ordinal)) {
                // If needs to know parent expression since
                // it must figure out how to handle 'else'
                // when if is without { }.
                context.Expressions.Push(this);

                var inlineIf = new InlineIf();
                inlineIf.Parse(context, null);

                context.Expressions.Pop();

                // Add to the stack even if it has errors in order
                // to avoid extra errors
                _operands.Push(inlineIf);
            } else {
                errorType = ParseErrorType.UnexpectedToken;
            }

            return errorType;
        }

        private bool IsTerminatingKeyword(string s) {
            if (_terminatingKeyword == null) {
                return false;
            }

            return s.Equals(_terminatingKeyword, StringComparison.Ordinal);
        }

        private ParseErrorType HandleOperator(ParseContext context, out OperationType operationType) {
            var currentOperator = new TokenOperator(firstInExpression: _operands.Count == 0);
            currentOperator.Parse(context, null);
            var isUnary = currentOperator.IsUnary;
            operationType = isUnary? OperationType.UnaryOperator: OperationType.BinaryOperator;

            if (currentOperator.OperatorType == OperatorType.Not) {
                //This comes from the fact that
                //  x < -1
                //  y < -1
                //  2 + !x + y
                // yields 2 (i.e. bang applies to the entire expression that follows
                isUnary = true;
                if (!IsInGroup && context.Tokens.IsLineBreakAfter(context.TextProvider, context.Tokens.Position - 1)) {
                    // Make sure expression that follows is not empty
                    return ParseErrorType.RightOperandExpected;
                }

                var exp = new Expression(IsInGroup);
                if (!exp.Parse(context, null)) {
                    return ParseErrorType.RightOperandExpected;
                }

                _operators.Push(currentOperator);
                _operands.Push(exp);
                operationType = OperationType.Operand;
                return ParseErrorType.None;
            }
            return HandleOperatorPrecedence(context, currentOperator);
        }

        private ParseErrorType HandleOperatorPrecedence(ParseContext context, IOperator currentOperator) {
            var errorType = ParseErrorType.None;
            var lastOperator = _operators.Peek();

            if (currentOperator.Precedence < lastOperator.Precedence ||
                (currentOperator.Precedence == lastOperator.Precedence &&
                 currentOperator.Associativity == Associativity.Left)) {
                // New operator has lower or equal precedence. We need to make a tree from
                // the topmost operator and its operand(s). Example: a*b+c. + has lower priority
                // and a and b should be on the stack along with * on the operator stack.
                // Repeat until there are no more higher precendece operators on the stack.

                // If operator cas right associativity (exponent), it needs to be processed right away. 
                // Consider the expression a~b~c. If the operator ~ has left associativity, this 
                // expression is interpreted as (a~b)~c. If the operator has right associativity, 
                // the expression is interpreted as a~(b~c). a^b^c = a^(b^c). 
                // Left -associated indexer works like x[1][2] = (x[1])[2]
                errorType = this.ProcessHigherPrecendenceOperators(context, currentOperator);
            }

            if (errorType == ParseErrorType.None) {
                _operators.Push(currentOperator);
            }

            return errorType;
        }

        private ParseErrorType ProcessHigherPrecendenceOperators(ParseContext context, IOperator currentOperator) {
            Debug.Assert(_operators.Count > 1);
            var errorType = ParseErrorType.None;
            var associativity = currentOperator.Associativity;
            var nextOperatorNode = _operators.Peek();

            // At least one operator above sentinel is on the stack.
            do {
                // If associativity is right, we stop if the next operator
                // on the stack has lower or equal precedence than the current one.
                // Example: in 'a^b^c' before pushing last ^ on the stack
                // only b^c is processed since a^b^c = a^(b^c).
                if (associativity == Associativity.Right &&
                    nextOperatorNode.Precedence <= currentOperator.Precedence) {
                    break;
                }

                errorType = MakeNode(context);
                if (errorType == ParseErrorType.None) {
                    nextOperatorNode = _operators.Peek();

                    // If associativity is left, we stop if the next operator
                    // on the stack has lower precedence than the current one.
                    // Example: in 'a+b*c*d+e' before pushing last + on the stack
                    // we need to make subtree out of b*c*d.
                    if (associativity == Associativity.Left &&
                        nextOperatorNode.Precedence < currentOperator.Precedence) {
                        break;
                    }
                }
            } while (_operators.Count > 1 && errorType == ParseErrorType.None);

            return errorType;
        }

        /// <summary>
        /// Constructs AST node from operator (root) and one or two operands.
        /// In order for the node to be successfully created stacks must contain
        /// an operator and, depending on the operator, one or two operands.
        /// </summary>
        /// <example>
        /// The newly created subtree (operator and root and operands are children)
        /// is then pushed into the operands stack. Example: in a*b+c before '+'
        /// can be processed, a*b is turned into an subtree and pushed as an operand
        /// to the operands stack. Then new subtree can be created with + at the root
        /// and 'c' and 'a*b' as its child nodes.
        /// </example>
        /// <param name="context">Parsing context</param>
        /// <returns>Parsing error of any</returns>
        private ParseErrorType MakeNode(ParseContext context) {
            var operatorNode = _operators.Pop();

            var rightOperand = SafeGetOperand(operatorNode);
            if (rightOperand == null) {
                // Oddly, no operands
                return ParseErrorType.RightOperandExpected;
            }

            if (operatorNode.IsUnary) {
                operatorNode.AppendChild(rightOperand);
                operatorNode.RightOperand = rightOperand;
            } else {
                var leftOperand = SafeGetOperand(operatorNode);
                if (leftOperand == null) {
                    // Operand is missing in expression like x <- [].
                    // Operator on top of the stack is <- since [] was not
                    // successfully parsed. So we need to mark right operand
                    // as error token.
                    context.AddError(new ParseError(ParseErrorType.LeftOperandExpected, ErrorLocation.Token, GetIndexerOrFunctionErrorRange(context, operatorNode)));
                    return ParseErrorType.LeftOperandExpected;
                }

                if (leftOperand.End <= operatorNode.Start && rightOperand.Start >= operatorNode.End) {
                    operatorNode.LeftOperand = leftOperand;
                    operatorNode.RightOperand = rightOperand;

                    operatorNode.AppendChild(leftOperand);
                    operatorNode.AppendChild(rightOperand);
                } else {
                    return ParseErrorType.UnexpectedToken;
                }
            }

            _operands.Push(operatorNode);
            return ParseErrorType.None;
        }

        private static IRValueNode CreateConstant(ParseContext context) {
            var tokens = context.Tokens;
            var currentToken = tokens.CurrentToken;
            IRValueNode term = null;

            switch (currentToken.TokenType) {
                case RTokenType.NaN:
                case RTokenType.Infinity:
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

        private IRValueNode SafeGetOperand(IOperator operatorNode) {
            if (!operatorNode.IsUnary) {
                return _operands.Count > 0 ? _operands.Pop() : null;
            }
            // Indexer expects operand to the left of the indexing expression as in 'a[]`. 
            // Unary minus expects operand to the right as in '-1'. Consider expression 
            // like `x ---`. Unary minus will be on to of the stack but 'x' that appears 
            // on top of the operand stack is not applicable. We need to filter operands
            // by position depending on the operator type.
            switch (operatorNode.OperatorType) {
                case OperatorType.Index:
                case OperatorType.FunctionCall:
                    return SafeGetLeftOperand(operatorNode);
            }
            return SafeGetRightOperand(operatorNode);
        }

        private IRValueNode SafeGetLeftOperand(IOperator operatorNode) {
            // Operand is on top the stack and must be to the left of the operator
            if (_operands.Count > 0) {
                if (_operands.Peek().End <= operatorNode.Start) {
                    return _operands.Pop();
                }
            }
            return null;
        }

        private IRValueNode SafeGetRightOperand(IOperator operatorNode) {
            // Operand is on top the stack and must be to the right of the operator
            if (_operands.Count > 0) {
                if (_operands.Peek().Start >= operatorNode.End) {
                    return _operands.Pop();
                }
            }
            return null;
        }
    }
}
