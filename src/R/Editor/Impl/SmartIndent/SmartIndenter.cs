// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Arguments;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;

namespace Microsoft.R.Editor.SmartIndent {
    /// <summary>
    /// Provides block and smart indentation in R code
    /// </summary>
    public sealed class SmartIndenter : ISmartIndenter {
        private readonly IREditorSettings _settings;
        private readonly IEditorView _view;

        public SmartIndenter(IEditorView view, IREditorSettings settings) {
            _view = view;
            _settings = settings;
        }

        #region ISmartIndenter
        public int? GetDesiredIndentation(IEditorLine line) {
            var res = GetDesiredIndentation(line, _settings.IndentStyle);
            if (res != null && line.Snapshot.EditorBuffer != _view.EditorBuffer) {
                var target = _view.MapToView(line.Snapshot, line.Start);
                if (target != null) {
                    // The indentation level is relative to the line in the text view when
                    // we were created, not to the line we were provided with on this call.
                    var diff = target.Position - target.GetContainingLine().Start;
                    return diff + res;
                }
            }
            return res;
        }

        public int? GetDesiredIndentation(IEditorLine line, IndentStyle indentStyle) {
            if (line != null) {
                switch (indentStyle) {
                    case IndentStyle.Block:
                        return GetBlockIndent(line, _settings);
                    case IndentStyle.Smart:
                        return GetSmartIndent(line, _settings);
                }
            }
            return null;
        }
        #endregion

        public static int GetBlockIndent(IEditorLine line, IREditorSettings settings) {
            var lineNumber = line.LineNumber;

            //Scan the previous lines for the first line that isn't an empty line.
            while (--lineNumber >= 0) {
                var previousLine = line.Snapshot.GetLineFromLineNumber(lineNumber);
                if (previousLine.Length > 0) {
                    return OuterIndentSizeFromLine(previousLine, settings.FormatOptions);
                }
            }

            return 0;
        }

        /// <summary>
        /// Determines level of indentation in the line from AST and surrounding context.
        /// Called when user hits ENTER and editor needs to know level of indentation in
        /// the new line as well as when code is being auto-formatted and range formatter
        /// needs to know how to indent freshly formatted lines of code.
        /// </summary>
        /// <param name="line">Line to find the indent for</param>
        /// <param name="settings">Editor settings</param>
        /// <param name="ast">Optional AST</param>
        /// <param name="originalIndentSizeInSpaces">
        /// Original (user) indentation in spaces. Used to preserve custom function argument alignment.
        /// </param>
        /// <param name="formatting">
        /// Indicates if current call is from formatter or 
        /// from the core editor for indentation when user typed Enter.
        /// </param>
        /// <returns>Level of indent in spaces</returns>
        public static int GetSmartIndent(IEditorLine line, IREditorSettings settings, AstRoot ast = null,
                                         int originalIndentSizeInSpaces = -1, bool formatting = false) {
            var editorBuffer = line.Snapshot.EditorBuffer;

            if (line.LineNumber == 0) {
                // Nothing to indent at the first line
                return 0;
            }

            ast = ast ?? editorBuffer.GetEditorDocument<IREditorDocument>()?.EditorTree?.AstRoot;
            if (ast == null) {
                return 0;
            }

            // The challenge here is to find scope to base the indent on.
            // The scope may or may not have braces and may or may not be closed. 
            // Current line is normally empty so we use previous line data assuming 
            // it is not empty. If previous line is empty, we do not look up 
            // to the nearest non-empty. This is the same as C# behavior.
            // So we need to locate nearest node that implements IAstNodeWithScope
            // or the scope (implemeting IScope) itself is scope is just '{ }'.

            // First try based on the previous line. We will try start of the line
            // like in 'if(...)' { in order to locate 'if' and then, if nothing is found,
            // try end of the line as in 'x <- function(...) {'
            var prevLine = line.Snapshot.GetLineFromLineNumber(line.LineNumber - 1);
            var prevLineText = prevLine.GetText();
            if (prevLineText.Trim().Equals("else", StringComparison.Ordinal)) {
                // Quick short circuit for new 'else' since it is not in the ASt yet.
                return GetBlockIndent(line, settings) + settings.IndentSize;
            }

            // First, let's see if we are in a function argument list and then indent based on 
            // the opening brace position. This needs to be done before looking for scopes
            // since function definition is a scope-defining statement.
            // Examples: 'call(a,\n<Enter>' or 'x <- function(a,<Enter>'
            if (prevLine.Length > 0) {
                var fc1 = ast.GetNodeOfTypeFromPosition<IFunction>(prevLine.End - 1);
                var fc2 = ast.GetNodeOfTypeFromPosition<IFunction>(line.Start);
                // Pick narrowest function. This happens when function definition appears
                // inside the argument list such as list(a = function(...)).
                var fc = fc2 ?? fc1;
                var indent = IndentFromFunctionCall(fc, prevLine, line, settings, formatting, originalIndentSizeInSpaces);
                if (indent.HasValue) {
                    return indent.Value;
                }
            }

            // Candidate position #1 is first non-whitespace character
            // in the the previous line
            var startOfNoWsOnPreviousLine = prevLine.Start + (prevLineText.Length - prevLineText.TrimStart().Length) + 1;

            // Try current new line so in case of 'if () { } else { | }' we find
            // the 'else' which defines the scope and not the parent 'if'.
            var scopeStatement1 = ast.GetNodeOfTypeFromPosition<IAstNodeWithScope>(line.Start);
            if (scopeStatement1 == null) {
                // If not found, try previous line that may define the indent
                scopeStatement1 = ast.GetNodeOfTypeFromPosition<IAstNodeWithScope>(startOfNoWsOnPreviousLine);
                if (scopeStatement1 == null) {
                    // Line start position works for typical scope-defining statements like if() or while()
                    // but it won't find function definition in 'x <- function(a) {'
                    // Try end of the line instead
                    var lastNonWsOnPreviousLine = Math.Max(0, prevLineText.TrimEnd().Length - 1);
                    scopeStatement1 = ast.GetNodeOfTypeFromPosition<IAstNodeWithScope>(lastNonWsOnPreviousLine);
                    // Verify that line we are asked to provide the smart indent for is actually inside 
                    // this scope since we could technically find end of x <- function(a) {}
                    // when we went up one line.
                    if (scopeStatement1?.Scope?.CloseCurlyBrace != null && !scopeStatement1.Contains(line.Start)) {
                        scopeStatement1 = null; // line is outside of this scope.
                    }
                }
            }

            var scopeStatement2 = ast.GetNodeOfTypeFromPosition<IAstNodeWithScope>(startOfNoWsOnPreviousLine);
            // Locate standalone scope which is not a statement, if any
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(prevLine.End);

            // Pick the narrowest item
            // so in case of 
            //  x <- function() {
            //      if(...)<Enter>
            //  }
            // we will use the 'if' and not the function definition
            var scopeCandidates = new List<IAstNode>() { scopeStatement1, scopeStatement2, scope };
            var smallestScope = scopeCandidates.OrderBy(x => x?.Length ?? int.MaxValue).FirstOrDefault();

            var scopeStatement = smallestScope as IAstNodeWithScope;
            scope = smallestScope as IScope;

            // If IScope is a scope defined by the parent statement, use
            // the parent statement so in 
            // x <- function(...) {
            //      |
            // }
            // the indent in scope is defined by the function and not by the opening {
            if (scope != null) {
                if (scope.Parent is IAstNodeWithScope parentStarement && parentStarement.Scope == scope) {
                    scopeStatement = parentStarement;
                    scope = null;
                }
            }

            if (scopeStatement != null) {
                if (scopeStatement.Scope == null) {
                    // There is nothing after statement that allows simple scope
                    // such as in 'if(...)EOF'
                    return GetBlockIndent(line, settings) + settings.IndentSize;
                }

                if (scopeStatement.Scope is SimpleScope) {
                    // There is statement with a simple scope above such as 'if' without { }. 
                    // We need to check if the line that is being formatted is part of this scope.
                    if (line.Start < scopeStatement.Scope.End) {
                        // Indent line one level deeper that the statement
                        return GetBlockIndent(line, settings) + settings.IndentSize;
                    }
                    // Line is not part of the scope, provide regular indent
                    return OuterIndentSizeFromNode(editorBuffer, scopeStatement, settings.FormatOptions);
                }

                // Check if line is the last line in a real scope (i.e. scope with { }) and only consists
                // of the closing }, it should be indented at the outer indent so closing scope aligns with
                // the beginning of the statement.
                if (scopeStatement.Scope.CloseCurlyBrace != null) {
                    var endOfScopeLine = editorBuffer.CurrentSnapshot.GetLineNumberFromPosition(scopeStatement.Scope.CloseCurlyBrace.Start);
                    if (endOfScopeLine <= line.LineNumber) {
                        return OuterIndentSizeFromNode(editorBuffer, scopeStatement, settings.FormatOptions);
                    }
                }

                if (scopeStatement.Scope.OpenCurlyBrace != null && settings.FormatOptions.BracesOnNewLine) {
                    var startOfScopeLine = editorBuffer.CurrentSnapshot.GetLineNumberFromPosition(scopeStatement.Scope.OpenCurlyBrace.Start);
                    if (startOfScopeLine == line.LineNumber) {
                        return OuterIndentSizeFromNode(editorBuffer, scopeStatement, settings.FormatOptions);
                    }
                }

                // We are inside a scope so provide inner indent
                return InnerIndentSizeFromNode(editorBuffer, scopeStatement, settings.FormatOptions);
            }

            // Try locate the scope itself, if any

            if (scope?.OpenCurlyBrace != null) {
                if (scope.CloseCurlyBrace != null) {
                    var endOfScopeLine = editorBuffer.CurrentSnapshot.GetLineNumberFromPosition(scope.CloseCurlyBrace.Start);
                    if (endOfScopeLine == line.LineNumber) {
                        return OuterIndentSizeFromNode(editorBuffer, scope, settings.FormatOptions);
                    }
                }
                return InnerIndentSizeFromNode(editorBuffer, scope, settings.FormatOptions);
            }

            return IndentByIncompleteStatement(ast, line, prevLine, scope, settings);
        }

        private static int IndentByIncompleteStatement(AstRoot ast, IEditorLine currentLine, IEditorLine prevLine, IAstNode scope, IREditorSettings settings) {
            // See if [ENTER] was hit in a middle of a statement. If it was hit in the first line of the statement,
            // indent one level deeper. Otherwise use block indent based on the previous line indent.
            //  x <-[ENTER]
            //    |
            //      1
            //
            //  x <-[ENTER]
            //    |
            //
            //  x <-
            //          a +[ENTER]
            //          |
            var snapshot = prevLine.Snapshot;
            var statement = ast.GetNodeOfTypeFromPosition<IStatement>(prevLine.End);
            if (statement != null) {
                return prevLine.Contains(statement.Start)
                        ? InnerIndentSizeFromNode(snapshot.EditorBuffer, scope, settings.FormatOptions)
                        : GetBlockIndent(currentLine, settings);
            }

            // The statement may be incomplete and hence expression parser
            // failed and hence there is no statement node in the AST.
            if (LineHasContinuation(prevLine)) {
                // We need to determine if last line was the first in the statement
                // or is it itself a continuation.
                if (prevLine.LineNumber > 0) {
                    var prevPrevLine = snapshot.GetLineFromLineNumber(prevLine.LineNumber - 1);
                    if (LineHasContinuation(prevPrevLine)) {
                        return GetBlockIndent(currentLine, settings);
                    }
                }
                return InnerIndentSizeFromNode(snapshot.EditorBuffer, scope, settings.FormatOptions);
            }
            return 0;
        }

        private static bool LineHasContinuation(IEditorLine line) {
            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(line.GetText());
            var lastTokenType = tokens.Count > 0 ? tokens[tokens.Count - 1].TokenType : RTokenType.Unknown;
            return lastTokenType == RTokenType.Operator || lastTokenType == RTokenType.Comma;
        }

        private static int? IndentFromFunctionCall(IFunction fc, IEditorLine prevLine, IEditorLine currentLine, IREditorSettings settings, bool formatting, int originalIndentSizeInSpaces) {
            var snapshot = currentLine.Snapshot;
            if (fc?.Arguments == null || fc.OpenBrace == null) {
                // No arguments or somehow open brace is missing
                return null;
            }

            if (fc.CloseBrace == null || fc.CloseBrace.End > prevLine.End) {
                // We only want to indent here if position is in arguments and not in the function scope.
                if (currentLine.Start >= fc.OpenBrace.End && !(fc.CloseBrace != null && currentLine.Start >= fc.CloseBrace.End)) {
                    if (originalIndentSizeInSpaces >= 0) {
                        // Preserve user indentation
                        return originalIndentSizeInSpaces;
                    }

                    // Indent one level deeper from the function definition line.
                    var fcLine = snapshot.GetLineFromPosition(fc.Start);
                    if (fcLine.LineNumber == prevLine.LineNumber) {
                        // Determine current base indent of the line (leading whitespace)
                        var fcIndentSize = IndentBuilder.TextIndentInSpaces(fcLine.GetText(), settings.TabSize);
                        if (fc.CloseBrace == null || fc.CloseBrace.End >= (formatting ? currentLine.Start : currentLine.End)) {
                            // Depending on options indent a) one level deeper or b) by first argument or c) by opening brace + 1
                            if (settings.SmartIndentByArgument) {
                                var indent = GetIndentFromArguments(fc, prevLine, settings);
                                fcIndentSize = indent.HasValue
                                    ? IndentBuilder.GetIndentString(indent.Value, settings.IndentType, settings.TabSize).Length
                                    : fcIndentSize + settings.IndentSize;
                            } else {
                                // Default indent is one level deeper
                                fcIndentSize += settings.IndentSize;
                            }
                        }
                        return fcIndentSize;
                    }
                    // If all fails, indent based on the previous line
                    return GetBlockIndent(currentLine, settings);
                }
            }
            return null;
        }

        public static int InnerIndentSizeFromNode(IEditorBuffer editorBuffer, IAstNode node, RFormatOptions options) {
            if (node != null) {
                // Scope indentation is based on the scope defining node i.e.
                // x <- function(a) {
                //      |
                // }
                // caret indent is based on the function definition and not
                // on the position of the opening {
                var scope = node as IScope;
                if (scope != null) {
                    if (node.Parent is IAstNodeWithScope scopeDefiningNode && scopeDefiningNode.Scope == scope) {
                        node = scopeDefiningNode;
                    }
                }
                var startLine = editorBuffer.CurrentSnapshot.GetLineFromPosition(node.Start);
                return InnerIndentSizeFromLine(startLine, options);
            }
            return 0;
        }

        public static int OuterIndentSizeFromNode(IEditorBuffer editorBuffer, IAstNode node, RFormatOptions options) {
            if (node != null) {
                var startLine = editorBuffer.CurrentSnapshot.GetLineFromPosition(node.Start);
                return OuterIndentSizeFromLine(startLine, options);
            }
            return 0;
        }

        public static int InnerIndentSizeFromLine(IEditorLine line, RFormatOptions options) {
            var lineText = line.GetText();
            var leadingWhitespace = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);
            var indentbuilder = new IndentBuilder(options.IndentType, options.IndentSize, options.TabSize);

            return IndentBuilder.TextIndentInSpaces(leadingWhitespace + indentbuilder.SingleIndentString, options.TabSize);
        }

        public static int OuterIndentSizeFromLine(IEditorLine line, RFormatOptions options) {
            var lineText = line.GetText();
            var leadingWhitespace = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);
            return IndentBuilder.TextIndentInSpaces(leadingWhitespace, options.TabSize);
        }

        private static int? GetIndentFromArguments(IFunction fc, IEditorLine prevLine, IREditorSettings settings) {
            // Fetch first argument on the previous line or first artument of the function
            // x < function(a,
            //              |
            // x < function(a,
            //                 b, c
            //                 |
            var snapshot = prevLine.Snapshot;
            var offset = 0;

            // If previous line contains start of the function call, format it
            // so whitespace is correct and we can determine proper indentation
            // based on the argument or the opening brace
            if (prevLine.Contains(fc.Start)) {
                var start = snapshot.GetLineFromPosition(fc.Start).Start;
                var end = snapshot.GetLineFromPosition(fc.End).End;
                var fcText = snapshot.GetText(TextRange.FromBounds(start, end));

                // Remember current indentation since formatter will remove it
                var currentIndent = IndentBuilder.TextIndentInSpaces(fcText, settings.TabSize);
                var formattedLineText = new RFormatter().Format(fcText);
                // Restore leading indent
                formattedLineText = IndentBuilder.GetIndentString(currentIndent, settings.IndentType, settings.TabSize) + formattedLineText;

                var ast = RParser.Parse(formattedLineText);
                var newFc = ast.FindFirstElement(n => n is IFunction) as IFunction;
                if (newFc != null) {
                    offset = prevLine.Start;
                }
                fc = newFc;
            }

            if (fc != null) {
                var arg = fc.Arguments.FirstOrDefault(a => !(a is StubArgument) && prevLine.Contains(a.Start + offset));

                if (arg != null) {
                    var argPosition = arg.Start + offset;
                    return argPosition - snapshot.GetLineFromPosition(argPosition).Start;
                }

                var bracePosition = fc.OpenBrace.Start + offset;
                return bracePosition - snapshot.GetLineFromPosition(bracePosition).Start + 1;
            }
            return null;
        }
    }
}
