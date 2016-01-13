using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Signatures;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Completion {
    using Core.Tokens;
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    /// <summary>
    /// R-specific completion controller. Initiates, commits or dismisses
    /// completion, signature and parameter help sessions depending 
    /// on what was typed and the current editor context.
    /// </summary>
    public sealed class RCompletionController : CompletionController {
        private ITextBuffer _textBuffer;
        private char _commitChar = '\0';

        private RCompletionController(
            ITextView textView,
            IList<ITextBuffer> subjectBuffers,
            ICompletionBroker completionBroker,
            IQuickInfoBroker quickInfoBroker,
            ISignatureHelpBroker signatureBroker)
            : base(textView, subjectBuffers, completionBroker, quickInfoBroker, signatureBroker) {
            _textBuffer = subjectBuffers[0];

            ServiceManager.AddService<RCompletionController>(this, TextView);
        }

        public override void Detach(ITextView textView) {
            ServiceManager.RemoveService<RCompletionController>(TextView);
            base.Detach(textView);
        }

        /// <summary>
        /// Called when text buffer becomes visible in the text view.
        /// The buffer may not be a top-level buffer in the graph and
        /// may be projected into view.
        /// </summary>
        public override void ConnectSubjectBuffer(ITextBuffer subjectBuffer) {
            _textBuffer = subjectBuffer;
        }

        /// <summary>
        /// Called when text buffer becomes invisible in the text view.
        /// The buffer may not be a top-level buffer in the graph and
        /// may be projected into view. Typically called when document
        /// is closed or buffer is removed from the view buffer graph.
        /// </summary>
        public override void DisconnectSubjectBuffer(ITextBuffer subjectBuffer) {
        }

        public static RCompletionController Create(
            ITextView textView,
            IList<ITextBuffer> subjectBuffers,
            ICompletionBroker completionBroker,
            IQuickInfoBroker quickInfoBroker,
            ISignatureHelpBroker signatureBroker) {
            RCompletionController completionController = null;

            completionController = ServiceManager.GetService<RCompletionController>(textView);
            if (completionController == null) {
                completionController = new RCompletionController(textView, subjectBuffers, completionBroker, quickInfoBroker, signatureBroker);
            }

            return completionController;
        }

        public static RCompletionController FromTextView(ITextView textView) {
            return ServiceManager.GetService<RCompletionController>(textView);
        }

        protected override bool AutoCompletionEnabled {
            get { return REditorSettings.CompletionEnabled; }
        }

        protected override bool AutoSignatureHelpEnabled {
            get { return REditorSettings.SignatureHelpEnabled; }
        }

        /// <summary>
        /// Should this key commit a completion session?
        /// </summary>
        public override bool IsCommitChar(char typedChar) {
            if (HasActiveCompletionSession && typedChar != 0) {
                // only ( completes keywords
                CompletionSet completionSet = CompletionSession.SelectedCompletionSet;
                string completionText = completionSet.SelectionStatus.Completion.InsertionText;

                if (completionText == "else" || completionText == "repeat") {
                    // { after 'else' or 'repeat' completes keyword
                    if (typedChar == '{')
                        return true;

                    // Space completes if selection is unique
                    if (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique)
                        return true;

                    return false;
                }

                // ';' completes after next or break keyword
                if (completionText == "break" || completionText == "next") {
                    if (typedChar == ';')
                        return true;

                    // Space completes if selection is unique
                    if (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique)
                        return true;
                }

                // Handle ( after keyword that is usually followed by expression in braces
                // such as for(), if(), library(), ...
                if (completionText == "if" || completionText == "for" || completionText == "while" ||
                    completionText == "return" || completionText == "library" || completionText == "require") {
                    if (typedChar == '(')
                        return true;

                    if (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique)
                        return true;

                    return false;
                }

                switch (typedChar) {
                    case '<':
                    case '>':
                    case '+':
                    case '-':
                    case '*':
                    case '^':
                    case '=':
                    case '%':
                    case '|':
                    case '&':
                    case '!':
                    case ':':
                    case '@':
                    case '$':
                    case '(':
                    case '[':
                    case '{':
                    case ')':
                    case ']':
                    case '}':
                    case ';':
                        return completionSet.SelectionStatus.IsUnique;
                }

                if (typedChar == ' ' && !REditorSettings.CommitOnSpace)
                    return false;

                if (char.IsWhiteSpace(typedChar)) {
                    IREditorDocument document = REditorDocument.TryFromTextBuffer(TextView.TextBuffer);
                    if (document != null && document.IsTransient) {
                        return typedChar == '\t';
                    }

                    if (typedChar == '\n' || typedChar == '\r') {
                        if (REditorSettings.CommitOnEnter)
                            return true;

                        return !IsAutoShownCompletion();
                    }

                    return true;
                }
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
            if (typedCharacter == '\t' && !HasActiveCompletionSession) {
                // if previous character is not whitespace, bring it on
                SnapshotPoint? position = REditorDocument.MapCaretPositionFromView(TextView);
                if (position.HasValue) {
                    int pos = position.Value;
                    if (pos > 0 && pos <= position.Value.Snapshot.Length) {
                        if (!char.IsWhiteSpace(position.Value.Snapshot[pos - 1])) {
                            ShowCompletion(autoShownCompletion: false);
                            return true;
                        }
                    }
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
                        return true;

                    case ':':
                        return RCompletionContext.IsCaretInNamespace(TextView);

                    case '(':
                        return RCompletionContext.IsCaretInLibraryStatement(TextView);

                    default:
                        if (REditorSettings.ShowCompletionOnFirstChar) {
                            return Char.IsLetter(typedCharacter) || typedCharacter == '.';
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
                if (!SignatureHelper.IsSameSignatureContext(TextView, _textBuffer)) {
                    DismissAllSessions();
                    TriggerSignatureHelp();
                }
            } else if (HasActiveSignatureSession(TextView) && typedCharacter == ')') {
                // Typing closing ) closes signature and completion sessions.
                DismissAllSessions();
                // However, when user types closing brace is an expression inside
                // function argument like in x = y * (z + 1) we need to re-trigger
                // signature session
                AstRoot ast = REditorDocument.FromTextBuffer(TextView.TextBuffer).EditorTree.AstRoot;
                FunctionCall f = ast.GetNodeOfTypeFromPosition<FunctionCall>(TextView.Caret.Position.BufferPosition);
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

        /// <summary>
        /// Updates insertion text so it excludes final commit character 
        /// </summary>
        protected override void UpdateInsertionText() {
            if (CompletionSession != null && !IsMuteCharacter(_commitChar)) {
                Completion curCompletion = CompletionSession.SelectedCompletionSet.SelectionStatus.Completion;
                string insertionText = curCompletion.InsertionText;

                if (insertionText[insertionText.Length - 1] == _commitChar) {
                    curCompletion.InsertionText = insertionText.Substring(0, insertionText.Length - 1);
                }
            }
        }

        /// <summary>
        /// Overrides default session since we want to track signature as caret moves.
        /// Default signature session dismisses when caret changes position.
        /// </summary>
        public override void TriggerSignatureHelp() {
            DismissAllSessions();
            SnapshotPoint? point = REditorDocument.MapCaretPositionFromView(TextView);
            if (point.HasValue) {
                ITrackingPoint trackingPoint = _textBuffer.CurrentSnapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
                SignatureBroker.TriggerSignatureHelp(TextView, trackingPoint, trackCaret: false);
            }
        }
    }
}