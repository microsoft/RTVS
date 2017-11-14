// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Formatting;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;

namespace Microsoft.R.Editor.Formatting {
    public class AutoFormat {
        private readonly Guid _treeUserId = new Guid("03DC828F-0F9F-44B5-B502-5875AD4145AF");
        private readonly IIncrementalWhitespaceChangeHandler _changeHandler;

        protected IServiceContainer Services { get; }
        protected IREditorSettings Settings { get; }
        protected IEditorView EditorView { get; }
        protected IEditorBuffer EditorBuffer { get; private set; }

        public AutoFormat(IServiceContainer services, IEditorView editorView, IEditorBuffer editorBuffer, IIncrementalWhitespaceChangeHandler changeHandler) {
            EditorView = editorView;
            EditorBuffer = editorBuffer;
            Services = services;
            Settings = services.GetService<IREditorSettings>();
            _changeHandler = changeHandler ?? services.GetService<IIncrementalWhitespaceChangeHandler>();
        }

        public bool IsPreProcessAutoformatTriggerCharacter(char ch) => ch == ';';
        public bool IsPostProcessAutoformatTriggerCharacter(char ch) => ch.IsLineBreak() || ch == '}';

        public void HandleTyping(char typedChar, int position, IEditorBuffer editorBuffer = null, IIncrementalWhitespaceChangeHandler changeHandler = null) {
            if (!Settings.AutoFormat || (!Settings.FormatScope && typedChar == '}')) {
                return;
            }

            EditorBuffer = editorBuffer ?? EditorBuffer;
            var document = EditorBuffer.GetEditorDocument<IREditorDocument>();
            // AST may or may not be ready. Upto the caller to decide if it is worth waiting.
            var ast = document.EditorTree.AstRoot; 

            // Make sure we are not formatting damaging the projected range in R Markdown
            // which looks like ```{r. 'r' should not separate from {.
            if (!CanFormatContainedLanguageLine(position, typedChar)) {
                return;
            }

            // We don't want to auto-format inside strings
            if (ast.IsPositionInsideString(position)) {
                return;
            }

            var fo = new FormatOperations(Services, EditorView, EditorBuffer, _changeHandler);
            if (typedChar.IsLineBreak()) {
                // Special case for hitting caret after } and before 'else'. We do want to format
                // the construct as '} else {' but if user types Enter after } and we auto-format
                // it will look as if the editor just eats the Enter. Instead, we will not be
                // autoformatting in this specific case. User can always format either the document
                // or select the block and reformat it.
                if (!IsBetweenCurlyAndElse(position)) {
                    var scopeStatement = GetFormatScope(position, ast);
                    // Do not format large scope blocks for performance reasons
                    if (scopeStatement != null && scopeStatement.Length < 200) {
                        fo.FormatNode(scopeStatement);
                    } else if (CanFormatLine(position, -1)) {
                        fo.FormatViewLine(-1);
                    }
                }
            } else if (typedChar == ';') {
                // Verify we are at the end of the string and not in a middle
                // of another string or inside a statement.
                var line = EditorBuffer.CurrentSnapshot.GetLineFromPosition(position);
                var positionInLine = position - line.Start;
                var lineText = line.GetText();
                if (positionInLine >= lineText.TrimEnd().Length) {
                    fo.FormatViewLine(0);
                }
            } else if (typedChar == '}') {
                fo.FormatCurrentStatement(limitAtCaret: true, caretOffset: -1);
            }
        }

        /// <summary>
        /// Determines if contained language line can be formatted.
        /// </summary>
        /// <param name="position">Position in the contained language buffer</param>
        /// <param name="typedChar">Typed character</param>
        /// <remarks>In R Markdown lines with ```{r should not be formatted</remarks>
        protected virtual bool CanFormatContainedLanguageLine(int position, char typedChar) => true;

        protected virtual bool CanFormatLine(int position, int lineOffset) {
            // Do not format inside strings. At this point AST may be empty due to the nature 
            // of [destructive] changes made to the document. We have to resort to tokenizer. 
            // In order to keep performance good during typing we'll use token stream from the classifier.
            var snapshot = EditorBuffer.CurrentSnapshot;
            var line = snapshot.GetLineFromPosition(position);
            var tokens = new RTokenizer().Tokenize(snapshot.GetText());
            var tokenIndex = tokens.GetItemContaining(line.Start);
            return tokenIndex < 0 || tokens[tokenIndex].TokenType != RTokenType.String;
        }

        private bool IsBetweenCurlyAndElse(int position) {
            // Note that this is post-typing to the construct is now '}<line_break>else'
            var snapshot = EditorBuffer.CurrentSnapshot;
            var lineNum = snapshot.GetLineNumberFromPosition(position);
            if (lineNum < 1) {
                return false;
            }

            var prevLine = snapshot.GetLineFromLineNumber(lineNum - 1);

            var leftSide = prevLine.GetText().TrimEnd();
            if (!leftSide.EndsWith("}", StringComparison.Ordinal)) {
                return false;
            }

            var currentLine = snapshot.GetLineFromLineNumber(lineNum);
            var rightSide = currentLine.GetText().TrimStart();
            if (!rightSide.StartsWith("else", StringComparison.Ordinal)) {
                return false;
            }

            return true;
        }

        private IKeywordScopeStatement GetFormatScope(int position, AstRoot ast) {
            try {
                var snapshot = EditorBuffer.CurrentSnapshot;
                var lineNumber = snapshot.GetLineNumberFromPosition(position);
                var line = snapshot.GetLineFromLineNumber(lineNumber);
                var lineText = line.GetText();
                if (lineText.TrimEnd().EndsWithOrdinal("}")) {
                    var scopeStatement = ast.GetNodeOfTypeFromPosition<IKeywordScopeStatement>(position);
                    return scopeStatement;
                }
            } catch (ArgumentException) { }
            return null;
        }
    }
}
