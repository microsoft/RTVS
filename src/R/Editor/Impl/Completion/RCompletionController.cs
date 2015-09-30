using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Completion.TypeThrough;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Completion
{
    using Core.AST;
    using Core.AST.Operators;
    using Core.AST.Variables;
    using Support.Help.Functions;
    using Support.Help.Definitions;
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;
    using Signatures;
    using Definitions;
    using Document.Definitions;

    public sealed class RCompletionController : CompletionController, ICommandTarget
    {
        private ITextBuffer _textBuffer;
        private List<ProvisionalText> _provisionalTexts = new List<ProvisionalText>();
        private char _eatNextQuote = '\0';
        private char _commitChar = '\0';

        private RCompletionController(
            ITextView textView,
            IList<ITextBuffer> subjectBuffers,
            ICompletionBroker completionBroker,
            IQuickInfoBroker quickInfoBroker,
            ISignatureHelpBroker signatureBroker)
            : base(textView, subjectBuffers, completionBroker, quickInfoBroker, signatureBroker)
        {
            _textBuffer = subjectBuffers[0];

            ServiceManager.AdviseServiceAdded<REditorDocument>(_textBuffer, OnDocumentReady);
        }

        public override void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            if (_textBuffer == subjectBuffer)
            {
                ServiceManager.AdviseServiceAdded<REditorDocument>(_textBuffer, OnDocumentReady);
            }

            base.ConnectSubjectBuffer(subjectBuffer);
        }

        public override void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            if (_textBuffer == subjectBuffer)
            {
                RCompletionController existingController = ServiceManager.GetService<RCompletionController>(TextView);

                // This can get called multiple times without a ConnectSubjectBuffer call between
                if (existingController != null)
                {
                    Debug.Assert(existingController == this);
                    if (existingController == this)
                    {
                        ServiceManager.RemoveService<RCompletionController>(TextView);
                    }
                }
            }

            base.DisconnectSubjectBuffer(subjectBuffer);
        }

        private void OnDocumentReady(REditorDocument document)
        {
            // This object isn't released on content type changes, 
            // instead using the (Dis)ConnectSubjectBuffer
            // methods to control it's lifetime.
            ServiceManager.AddService<RCompletionController>(this, TextView);
        }

        public static RCompletionController Create(
            ITextView textView,
            IList<ITextBuffer> subjectBuffers,
            ICompletionBroker completionBroker,
            IQuickInfoBroker quickInfoBroker,
            ISignatureHelpBroker signatureBroker)
        {
            RCompletionController completionController = null;

            completionController = ServiceManager.GetService<RCompletionController>(textView);
            if (completionController == null)
            {
                completionController = new RCompletionController(textView, subjectBuffers, completionBroker, quickInfoBroker, signatureBroker);
            }

            return completionController;
        }

        protected override bool AutoCompletionEnabled
        {
            get { return REditorSettings.CompletionEnabled; }
        }

        protected override bool AutoSignatureHelpEnabled
        {
            get { return REditorSettings.SignatureHelpEnabled; }
        }

        /// <summary>
        /// Should this key press commit a completion session?
        /// </summary>
        public override bool IsCommitChar(char typedChar)
        {
            if (HasActiveCompletionSession && typedChar != 0)
            {
                // only ( completes keywords
                CompletionSet completionSet = CompletionSession.SelectedCompletionSet;
                string completionText = completionSet.SelectionStatus.Completion.InsertionText;

                if (completionText == "else" || completionText == "repeat")
                {
                    if (typedChar == '{')
                        return true;

                    if (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique)
                        return true;

                    return false;
                }

                if (completionText == "break" || completionText == "next")
                {
                    if (typedChar == ';')
                        return true;

                    if (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique)
                        return true;
                }

                if (completionText == "if" || completionText == "for" || completionText == "while" || 
                    completionText == "return" || completionText == "library" || completionText == "require")
                {
                    if (typedChar == '(')
                        return true;

                    if (char.IsWhiteSpace(typedChar) && completionSet.SelectionStatus.IsUnique)
                        return true;

                    return false;
                }

                switch (typedChar)
                {
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

                if (char.IsWhiteSpace(typedChar) || typedChar == '\n' || typedChar == '\t')
                {
                    IREditorDocument document = REditorDocument.TryFromTextBuffer(TextView.TextBuffer);
                    if(document != null && document.IsTransient)
                    {
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Should this key press start a completion session?
        /// </summary>
        public override bool IsTriggerChar(char typedCharacter)
        {
            if (!HasActiveCompletionSession)
            {
                switch (typedCharacter)
                {
                    //case '$':
                    //case '@':
                    case ':':
                        return RCompletionContext.IsInNamespace(TextView);

                    case '(':
                        return RCompletionContext.IsInLibraryStatement(TextView);

                    default:
                        return Char.IsLetter(typedCharacter);
                }
            }

            return false;
        }

        protected override bool IsRetriggerChar(ICompletionSession session, char typedCharacter)
        {
            switch (typedCharacter)
            {
                case '@':
                case '$':
                    return true;
            }

            return false;
        }

        public override void OnPostTypeChar(char typedCharacter)
        {
            if (typedCharacter == '(' || typedCharacter == ',')
            {
                if (!IsSameSignatureContext())
                {
                    DismissAllSessions();
                    SignatureBroker.TriggerSignatureHelp(TextView);
                }
            }
            else if (HasActiveSignatureSession(TextView) && typedCharacter == ')')
            {
                DismissAllSessions();

                AstRoot ast = REditorDocument.FromTextBuffer(TextView.TextBuffer).EditorTree.AstRoot;
                FunctionCall f = ast.GetNodeOfTypeFromPosition<FunctionCall>(TextView.Caret.Position.BufferPosition);
                if (f != null)
                {
                    SignatureBroker.TriggerSignatureHelp(TextView);
                }
            }
            else if (HasActiveSignatureSession(TextView) && typedCharacter == '\n')
            {
                DismissAllSessions();
                SignatureBroker.TriggerSignatureHelp(TextView);
            }
            else if (this.HasActiveCompletionSession)
            {
                if (typedCharacter == ',')
                {
                    CompletionSession.Dismiss();
                }
                else if (typedCharacter == '\'' || typedCharacter == '\"')
                {
                    base.OnPostTypeChar(typedCharacter);

                    DismissAllSessions();
                    ShowCompletion(autoShownCompletion: true);
                    return;
                }
            }

            base.OnPostTypeChar(typedCharacter);
        }

        private bool IsSameSignatureContext()
        {
            var sessions = SignatureBroker.GetSessions(TextView);
            Debug.Assert(sessions.Count < 2);
            if (sessions.Count == 1)
            {
                IFunctionInfo sessionFunctionInfo = null;
                sessions[0].Properties.TryGetProperty<IFunctionInfo>("functionInfo", out sessionFunctionInfo);

                if (sessionFunctionInfo != null)
                {
                    try
                    {
                        IREditorDocument document = REditorDocument.FromTextBuffer(TextView.TextBuffer);
                        document.EditorTree.EnsureTreeReady();

                        ParametersInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(
                            document.EditorTree.AstRoot, _textBuffer.CurrentSnapshot, 
                            TextView.Caret.Position.BufferPosition);

                        return parametersInfo != null && parametersInfo.FunctionName == sessionFunctionInfo.Name;
                    }
                    catch (Exception) { }
                }
            }

            return false;
        }

        public override bool CommitCompletionSession(char typedCharacter)
        {
            try
            {
                _commitChar = typedCharacter;
                return base.CommitCompletionSession(typedCharacter);
            }
            finally
            {
                _commitChar = '\0';
            }
        }

        protected override void UpdateInsertionText()
        {
            if (CompletionSession != null && !IsMuteCharacter(_commitChar))
            {
                Completion curCompletion = CompletionSession.SelectedCompletionSet.SelectionStatus.Completion;
                string insertionText = curCompletion.InsertionText;

                if (insertionText[insertionText.Length - 1] == _commitChar)
                {
                    curCompletion.InsertionText = insertionText.Substring(0, insertionText.Length - 1);
                }
            }
        }

        #region Provisional text
        protected override void OnCompletionSessionDismissed(object sender, EventArgs eventArgs)
        {
            if (_commitChar == '\0')
            {
                // Only call the base if we aren't in the midst of a commit
                base.OnCompletionSessionDismissed(sender, eventArgs);
            }
        }

        private void OnCloseProvisionalText(object sender, EventArgs e)
        {
            var provisionalText = sender as ProvisionalText;
            if (provisionalText != null)
            {
                _provisionalTexts.Remove(provisionalText);
                provisionalText.Closing -= OnCloseProvisionalText;
            }
        }

        internal ProvisionalText GetInnerProvisionalText()
        {
            int minLength = Int32.MaxValue;
            ProvisionalText innerText = null;

            foreach (ProvisionalText provisionalText in _provisionalTexts)
            {
                if (provisionalText.CurrentSpan.Length < minLength)
                {
                    minLength = provisionalText.CurrentSpan.Length;
                    innerText = provisionalText;
                }
            }

            return innerText;
        }

        internal ProvisionalText CreateProvisionalText(Span span, char eatNextQuote)
        {
            var provisionalText = new ProvisionalText(TextView, span);

            provisionalText.Closing += OnCloseProvisionalText;
            _provisionalTexts.Add(provisionalText);

            if (_provisionalTexts.Count == 1)
            {
                _eatNextQuote = eatNextQuote;
            }

            return provisionalText;
        }
        #endregion

        #region ICommandTarget

        public CommandStatus Status(Guid group, int id)
        {
            return CommandStatus.SupportedAndEnabled;
        }

        public CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg)
        {
            var typedCharacter = TypingCommandHandler.GetTypedChar(group, id, inputArg);

            // Various unrelated commands (eg: VSStandardCommandSet97.SolutionCfg) comes through quite often while typing
            if (typedCharacter == '\0')
                return CommandResult.NotSupported;

            var eatNextQuote = _eatNextQuote;
            bool isQuote = typedCharacter == '\"' || typedCharacter == '\'';

            _eatNextQuote = '\0';

            if (isQuote)
            {
                if (eatNextQuote != '\0' && _provisionalTexts.Count > 0)
                {
                    ProvisionalText innerProvisionalText = _provisionalTexts[_provisionalTexts.Count - 1];
                    if (innerProvisionalText.CurrentSpan.Length >= 2)
                    {
                        if ((innerProvisionalText.ProvisionalChar == typedCharacter) &&
                            (_textBuffer.CurrentSnapshot[innerProvisionalText.CurrentSpan.Start] == typedCharacter))
                        {
                            return CommandResult.Executed; // eat character
                        }
                    }
                }

                var caretPosition = TextView.Caret.Position.BufferPosition;
                foreach (var pt in _provisionalTexts)
                {
                    var span = pt.CurrentSpan;
                    if (caretPosition == span.End - 1 && typedCharacter == pt.ProvisionalChar)
                    {
                        return new CommandResult(CommandStatus.Supported, 0);
                    }
                }
            }

            return CommandResult.NotSupported;
        }

        public void PostProcessInvoke(CommandResult result, Guid group, int id, object inputArg, ref object outputArg)
        {
        }
        #endregion
    }
}