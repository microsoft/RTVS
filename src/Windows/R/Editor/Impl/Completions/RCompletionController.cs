// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Completions.Engine;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Roxygen;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// R-specific completion controller. Initiates, commits or dismisses
    /// completion, signature and parameter help sessions depending 
    /// on what was typed and the current editor context.
    /// </summary>
    public sealed class RCompletionController : CompletionController {
        public const string IsRHistoryRequest = nameof(IsRHistoryRequest);

        private readonly IREditorSettings _settings;
        private ITextBuffer _textBuffer;
        private char _commitChar = '\0';

        public RCompletionController(ITextView textView, IList<ITextBuffer> subjectBuffers, IServiceContainer services)
            : base(textView, subjectBuffers, services) {
            _textBuffer = subjectBuffers[0];
            _settings = services.GetService<IREditorSettings>();
        }

        /// <summary>
        /// Called when text buffer becomes visible in the text view.
        /// The buffer may not be a top-level buffer in the graph and
        /// may be projected into view.
        /// </summary>
        public override void ConnectSubjectBuffer(ITextBuffer subjectBuffer) => _textBuffer = subjectBuffer;

        /// <summary>
        /// Called when text buffer becomes invisible in the text view.
        /// The buffer may not be a top-level buffer in the graph and
        /// may be projected into view. Typically called when document
        /// is closed or buffer is removed from the view buffer graph.
        /// </summary>
        public override void DisconnectSubjectBuffer(ITextBuffer subjectBuffer) { }

        protected override bool AutoCompletionEnabled => _settings.CompletionEnabled;
        protected override bool AutoSignatureHelpEnabled => _settings.SignatureHelpEnabled;

        /// <summary>
        /// Should this key commit a completion session?
        /// </summary>
        public override bool IsCommitChar(char typedChar) {
            if (HasActiveCompletionSession && typedChar != 0) {
                // only ( completes keywords
                var completionSet = CompletionSession.SelectedCompletionSet;
                var completionText = completionSet.SelectionStatus.Completion.InsertionText;

                if (completionText == "else" || completionText == "repeat") {
                    // { after 'else' or 'repeat' completes keyword
                    if (typedChar == '{') {
                        return true;
                    }

                    // Space completes if selection is unique
                    if (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique) {
                        return true;
                    }

                    return false;
                }

                // ';' completes after next or break keyword
                if (completionText == "break" || completionText == "next") {
                    if (typedChar == ';') {
                        return true;
                    }

                    // Space completes if selection is unique
                    if (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique) {
                        return true;
                    }
                }

                // Handle ( after keyword that is usually followed by expression in braces
                // such as for(), if(), library(), ...
                if (completionText == "if" || completionText == "for" || completionText == "while" ||
                    completionText == "return" || completionText == "library" || completionText == "require") {
                    return typedChar == '(' || typedChar == '\t' || (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique);
                }

                if (typedChar == '=') {
                    var rset = completionSet as RCompletionSet;
                    rset?.Filter(typedChar);
                    rset?.SelectBestMatch();
                }

                var unique = completionSet.SelectionStatus.IsUnique;
                switch (typedChar) {
                    case '=':
                    case '<':
                    case '>':
                    case '+':
                    case '-':
                    case '*':
                    case '^':
                    case '%':
                    case '|':
                    case '&':
                    case '!':
                    case '@':
                    case '$':
                    case '(':
                    case '[':
                    case '{':
                    case ')':
                    case ']':
                    case '}':
                    case ';':
                        return unique;

                    case ':':
                        // : completes only if not preceded by another :
                        // so we can avoid false positive when user is typing stats:::
                        var caretPosition = TextView.Caret.Position.BufferPosition;
                        if (caretPosition > 0) {
                            unique &= TextView.TextBuffer.CurrentSnapshot[caretPosition - 1] != ':';
                        }
                        return unique;
                }

                if (typedChar == ' ' && !_settings.CommitOnSpace) {
                    return false;
                }

                if (typedChar.IsLineBreak()) {
                    // Complete on Enter but only if selection does not exactly match
                    // applicable span. for example, if span is X and selection is X123
                    // then we do complete. However, if selection is X then text is already
                    // fully typed and Enter should be adding new line as with regular typing.
                    if (completionSet.SelectionStatus.IsSelected) {
                        var typedText = completionSet.ApplicableTo.GetText(_textBuffer.CurrentSnapshot).Trim();
                        return completionText.Length > typedText.Length;
                    }
                }

                return char.IsWhiteSpace(typedChar);
            }

            return false;
        }

        /// <summary>
        /// Called before character type is passed down to the core editor
        /// along the controll chain. Gives language-specific controller
        /// a chance to initiate different action and potentially 'eat'
        /// the character. For example, in R typing 'abc[TAB] should bring
        /// up intellisense list rather than actually insert the tab character.
        /// </summary>
        /// <returns>
        /// True if character was handled and should not be 
        /// passed down to core editor or false otherwise.
        /// </returns>
        public override bool OnPreTypeChar(char typedCharacter) {
            // Allow tab to bring intellisense if
            //  a) REditorSettings.ShowCompletionOnTab true
            //  b) Position is at the end of a string so we bring completion for files
            //  c) There is no selection
            if (typedCharacter == '\t' && !HasActiveCompletionSession && TextView.Selection.StreamSelectionSpan.Length == 0) {
                // if previous character is identifier character, bring completion list
                var position = TextView.GetCaretPosition(_textBuffer);
                if (position.HasValue) {
                    int pos = position.Value;
                    var document = position.Value.Snapshot.TextBuffer.GetEditorDocument<IREditorDocument>();
                    if (!document.IsPositionInComment(pos)) {
                        if (pos > 0 && pos <= position.Value.Snapshot.Length) {
                            var endOfIdentifier = RTokenizer.IsIdentifierCharacter(position.Value.Snapshot[pos - 1]);
                            var showCompletion = endOfIdentifier && _settings.ShowCompletionOnTab;
                            if (!showCompletion) {
                                showCompletion = RCompletionEngine.CanShowFileCompletion(document.EditorTree.AstRoot, pos, out string directory);
                            }
                            if (showCompletion) {
                                ShowCompletion(autoShownCompletion: false);
                                return true; // eat the character
                            }
                        }
                    }
                }
            } else if (typedCharacter == '#') {
                if (TryInsertRoxygenBlock()) {
                    return true;
                }
            }

            return base.OnPreTypeChar(typedCharacter);
        }


        /// <summary>
        /// Should this key press trigger a completion session?
        /// </summary>
        public override bool IsTriggerChar(char typedCharacter) {
            if (!HasActiveCompletionSession) {
                switch (typedCharacter) {
                    case '$':
                    case '@':
                        return true;

                    case ':':
                        return TextView.ToEditorView().IsCaretInNamespace(_textBuffer.ToEditorBuffer());

                    case '(':
                        return TextView.ToEditorView().IsCaretInLibraryStatement(_textBuffer.ToEditorBuffer());

                    default:
                        if (_settings.ShowCompletionOnFirstChar) {
                            var position = TextView.GetCaretPosition(_textBuffer);
                            if (position.HasValue) {
                                int pos = position.Value;
                                var snapshot = position.Value.Snapshot;
                                // Trigger on first character
                                if (RTokenizer.IsIdentifierCharacter(typedCharacter) && !char.IsDigit(typedCharacter)) {
                                    // Ignore if this is not the first character
                                    return pos <= 1 || (pos > 1 && !RTokenizer.IsIdentifierCharacter(snapshot[pos - 2]));
                                }
                            }
                        }
                        break;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if character is a re-trigger one. Re-trigger
        /// means 'commit and trigger again' such as when user
        /// hits $ that commits current session for the class/object
        /// and trigger it again for object members.
        /// </summary>
        protected override bool IsRetriggerChar(ICompletionSession session, char typedCharacter) {
            switch (typedCharacter) {
                case '@':
                case '$':
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Called after character is typed. Gives language-specific completion
        /// controller has a chance to dismiss or initiate completion and paramenter
        /// help sessions depending on the current context.
        /// </summary>
        public override void OnPostTypeChar(char typedCharacter) {
            if (typedCharacter == '(' || typedCharacter == ',') {
                // Check if caret moved into a different functions such as when
                // user types a sequence of nested function calls. If so,
                // dismiss current signature session and start a new one.
                if (!TextView.ToEditorView().IsSameSignatureContext(_textBuffer.ToEditorBuffer(), Services)) {
                    DismissAllSessions();
                    TriggerSignatureHelp();
                }
            } else if (HasActiveSignatureSession(TextView) && typedCharacter == ')') {
                // Typing closing ) closes signature and completion sessions.
                DismissAllSessions();
                // However, when user types closing brace is an expression inside
                // function argument like in x = y * (z + 1) we need to re-trigger
                // signature session
                var ast = TextView.TextBuffer.GetEditorDocument<IREditorDocument>().EditorTree.AstRoot;
                var f = ast.GetNodeOfTypeFromPosition<FunctionCall>(TextView.Caret.Position.BufferPosition);
                if (f != null) {
                    TriggerSignatureHelp();
                }
            } else if (HasActiveSignatureSession(TextView) && typedCharacter == '\n') {
                // Upon ENTER we need to dismiss all sessions and re-trigger
                // signature help. Triggering signature help outside of 
                // a function definition or call is a no-op so it is safe.
                DismissAllSessions();
                TriggerSignatureHelp();
            } else if (this.HasActiveCompletionSession) {
                if (typedCharacter == '\'' || typedCharacter == '\"') {
                    // First handle completion of a string.
                    base.OnPostTypeChar(typedCharacter);
                    // Then re-trigger completion.
                    DismissAllSessions();
                    ShowCompletion(autoShownCompletion: true);
                    return;
                } else {
                    // Backspace does not dismiss completion. Characters that may be an identifier
                    // name do not dismiss completion either allowing correction of typing.
                    if (typedCharacter != '\b' && !RTokenizer.IsIdentifierCharacter(typedCharacter)) {
                        DismissCompletionSession();
                    }
                }
            }
            base.OnPostTypeChar(typedCharacter);
        }

        public override bool CommitCompletionSession(char typedCharacter) {
            try {
                _commitChar = typedCharacter;
                return base.CommitCompletionSession(typedCharacter);
            } finally {
                _commitChar = '\0';
            }
        }

        protected override bool CanDismissSignatureOnCommit() {
            // Do not dismiss signature sessions by default.
            // We handle signature dismiss when handling
            // post type character
            return false;
        }

        public override bool IsMuteCharacter(char typedCharacter) {
            if (typedCharacter == '=') {
                var equalsCompletion = CompletionSession.SelectedCompletionSet?.SelectionStatus?
                                                         .Completion.InsertionText.TrimEnd().EndsWithOrdinal("=");
                if (equalsCompletion.HasValue && equalsCompletion.Value) {
                    return true;
                }
            }
            return base.IsMuteCharacter(typedCharacter);
        }
        /// <summary>
        /// Updates insertion text so it excludes final commit character 
        /// </summary>
        protected override void UpdateInsertionText() {
            if (CompletionSession != null && !IsMuteCharacter(_commitChar)) {
                var curCompletion = CompletionSession.SelectedCompletionSet.SelectionStatus.Completion;
                var insertionText = curCompletion.InsertionText;

                if (insertionText[insertionText.Length - 1] == _commitChar) {
                    curCompletion.InsertionText = insertionText.Substring(0, insertionText.Length - 1);
                }
            }
        }

        protected override void OnCompletionSessionCommitted(object sender, EventArgs eventArgs) {
            // Auto-insert of braces is disabled until we have reliable method
            // of determination if given token is a function or a variable
            // using both AST and R engine.

            //if (CompletionSession != null) {
            //    if (CompletionSession.CompletionSets.Count > 0) {
            //        Completion completion = CompletionSession.SelectedCompletionSet.SelectionStatus.Completion;
            //        string name = completion.InsertionText;
            //        SnapshotPoint position = CompletionSession.TextView.Caret.Position.BufferPosition;
            //        Task.Run(async () => await InsertFunctionBraces(position, name));
            //    }
            //}
            base.OnCompletionSessionCommitted(sender, eventArgs);
        }

        /// <summary>
        /// Overrides default session since we want to track signature as caret moves.
        /// Default signature session dismisses when caret changes position.
        /// </summary>
        public override void TriggerSignatureHelp() {
            DismissSignatureSession(TextView, Services);
            DismissQuickInfoSession(TextView);
            SignatureBroker.TriggerSignatureHelp(TextView);
        }

        private bool TryInsertRoxygenBlock() {
            var point = TextView.GetCaretPosition(_textBuffer);
            if (point.HasValue) {
                var snapshot = _textBuffer.CurrentSnapshot;
                var line = snapshot.GetLineFromPosition(point.Value);
                if (line.LineNumber < snapshot.LineCount - 1 && point.Value == line.End && line.GetText().EqualsOrdinal("##")) {
                    var nextLine = snapshot.GetLineFromLineNumber(line.LineNumber + 1);
                    var document = _textBuffer.GetEditorDocument<IREditorDocument>();
                    document.EditorTree.EnsureTreeReady();
                    return RoxygenBlock.TryInsertBlock(_textBuffer.ToEditorBuffer(), document.EditorTree.AstRoot, nextLine.Start);
                }
            }
            return false;
        }
    }
}