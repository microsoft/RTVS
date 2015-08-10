using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Completion;
using Microsoft.Languages.Editor.Controller;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Completion.TypeThrough;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Settings;

namespace Microsoft.R.Editor.Completion
{
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    public sealed class RCompletionController : CompletionController, ICommandTarget
    {
        internal static readonly string DisableSpaceCommitKey = "NoCommitOnSpace";

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

            ServiceManager.AdviseServiceAdded<EditorDocument>(_textBuffer, OnDocumentReady);
        }

        public override void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
            if (_textBuffer == subjectBuffer)
            {
                ServiceManager.AdviseServiceAdded<EditorDocument>(_textBuffer, OnDocumentReady);
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

        // There are startup sequences (eg: rename txt->html) where the document hasn't been created yet. 
        private void OnDocumentReady(EditorDocument document)
        {
            //document.HtmlEditorTree.UpdateCompleted += OnTreeUpdateCompleted;

            // This object isn't released on content type changes, instead using the (Dis)ConnectSubjectBuffer
            //   methods to control it's lifetime.
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

        //private void OnTreeUpdateCompleted(object sender, HtmlTreeUpdatedEventArgs e)
        //{
        //var tree = sender as HtmlEditorTree;
        //if (tree.IsDirty && (tree.PendingChanges.TextChangeType & (TextChangeType.Comments | TextChangeType.Artifacts)) != 0)
        //{
        //    var sessions = CompletionBroker.GetSessions(TextView);
        //    foreach (var s in sessions)
        //    {
        //        if (s.SelectedCompletionSet is RCompletionSet)
        //            s.Dismiss();
        //    }
        //}
        //}

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
                    if(typedChar == '{')
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

                if (completionText == "if" || completionText == "for" || completionText == "while" || completionText == "return" || completionText == "library")
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
                        return true;
                }

                if (typedChar == ' ' && CompletionSession.Properties.ContainsProperty(RCompletionController.DisableSpaceCommitKey))
                    return false;

                if (char.IsWhiteSpace(typedChar) || typedChar == '\n' || typedChar == '\t')
                    return true;
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
                return Char.IsLetter(typedCharacter);
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

        public object TextChangeType { get; private set; }

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