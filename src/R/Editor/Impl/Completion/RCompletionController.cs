using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Completion
{
    using Languages.Editor.Completion;
    using Languages.Editor.Controller;
    using Languages.Editor.Services;
    using Languages.Editor.Completion.TypeThrough;
    using R.Editor.Document;
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;
    using Languages.Editor;
    using Settings;
    using Definitions;

    public sealed class RCompletionController : CompletionController, ICommandTarget
    {
        internal static readonly string DisableSpaceCommitKey = "NoCommitOnSpace";

        private ITextBuffer _textBuffer;
        private List<ProvisionalText> _provisionalTexts = new List<ProvisionalText>();
        private char _eatNextQuote = '\0';
        private char _commitChar = '\0';
        private RCompletion _committedCompletion;

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

        // There are startup sequences (eg: rename txt->html) where the document hasn't been created yet. 
        private void OnDocumentReady(REditorDocument document)
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
            get { return RSettings.CompletionEnabled; }
        }

        protected override bool AutoSignatureHelpEnabled
        {
            get { return RSettings.SignatureHelpEnabled; }
        }

        private void OnArtifactsChanged(object sender, EventArgs e)
        {
            this.DismissAllSessions();
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
        /// This uses the range that is applicable to completion and returns the character before it.
        /// Returns zero if the range can't be determined or if there is no character before it.
        /// </summary>
        private char GetCharacterBeforeCompletion()
        {
            if (CompletionSession.SelectedCompletionSet != null)
            {
                ITrackingSpan span = CompletionSession.SelectedCompletionSet.ApplicableTo;
                if (span != null)
                {
                    int startPos = span.GetStartPoint(_textBuffer.CurrentSnapshot).Position;
                    if (startPos > 0)
                    {
                        return _textBuffer.CurrentSnapshot[startPos - 1];
                    }
                }
            }

            return '\0';
        }

        private char GetCharacterAfterCompletion()
        {
            if (CompletionSession.SelectedCompletionSet != null)
            {
                ITrackingSpan span = CompletionSession.SelectedCompletionSet.ApplicableTo;
                if (span != null)
                {
                    int endPos = span.GetEndPoint(_textBuffer.CurrentSnapshot).Position;
                    if (endPos < _textBuffer.CurrentSnapshot.Length)
                    {
                        return _textBuffer.CurrentSnapshot[endPos];
                    }
                }
            }

            return '\0';
        }

        /// <summary>
        /// Should this key press commit a completion session?
        /// </summary>
        public override bool IsCommitChar(char typedChar)
        {
            if (HasActiveCompletionSession && typedChar != 0)
            {
                string completionType = CompletionTypes.None;

                RCompletionSet curCompletionSet = CompletionSession.SelectedCompletionSet as RCompletionSet;
                if ((curCompletionSet != null) && (curCompletionSet.SelectionStatus.IsSelected))
                {
                    //CompletionEntry curCompletion = curCompletionSet.SelectionStatus.Completion as CompletionEntry;
                    //if ((curCompletion != null) && curCompletion.IsCommitChar(typedChar))
                    //{
                    //    return true;
                    //}
                }

                if (CompletionSession.Properties.ContainsProperty(RCompletionSource.CompletionTypeKey))
                    completionType = (string)CompletionSession.Properties[RCompletionSource.CompletionTypeKey];

                switch (typedChar)
                {
                    case '=':
                        var selectionStatus = CompletionSession.SelectedCompletionSet.SelectionStatus;
                        if (selectionStatus.IsSelected)
                        {
                            var completion = selectionStatus.Completion as RCompletion;
                            if (completion != null && completion.RetriggerIntellisense)
                                return true;
                        }

                        // This will commit if there is a selection, otherwise it will dismiss
                        CommitCompletionSession();
                        return false;
                }

                if (typedChar == ' ' && CompletionSession.Properties.ContainsProperty(RCompletionController.DisableSpaceCommitKey))
                    return false;

                if (char.IsWhiteSpace(typedChar) || typedChar == '\n' || typedChar == '\t')
                    return true;
            }

            return false;
        }

        public override bool IsMuteCharacter(char typedCharacter)
        {
            if (CompletionSession != null)
            {
                RCompletionSet curCompletionSet = CompletionSession.SelectedCompletionSet as RCompletionSet;
                if ((curCompletionSet != null) && (curCompletionSet.SelectionStatus.IsSelected))
                {
                    //CompletionEntry curCompletion = curCompletionSet.SelectionStatus.Completion as CompletionEntry;
                    //if ((curCompletion != null) && curCompletion.IsMuteCharacter(typedCharacter))
                    //{
                    //    return true;
                    //}
                }
            }

            switch (typedCharacter)
            {
                case '=':
                    if (CanCommitCompletionSession(typedCharacter))
                    {
                        return true;
                    }
                    break;
            }

            return base.IsMuteCharacter(typedCharacter);
        }

        /// <summary>
        /// Should this key press start a completion session?
        /// </summary>
        public override bool IsTriggerChar(char typedChar)
        {
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
                    curCompletion.InsertionText = insertionText.Substring(0, insertionText.Length - 1);
            }
        }

        public override bool InCommit
        {
            set
            {
                if (value && (CompletionSession != null) && (CompletionSession.SelectedCompletionSet != null))
                {
                    // We store the completion that is being committed as this information needs to be used
                    //    in OnCompletionSessionCommitted and may not be available at that point.
                    //    It is unavailable in scenarios where a Dismiss occurs during the commit 
                    //    (such as when the VSCore CompletionSession class determines that our
                    //    ApplicableTo text has been deleted which may occur when url completion ".." entry
                    //    is committed.
                    CompletionSet compSet = CompletionSession.SelectedCompletionSet;
                    Completion completion = compSet.SelectionStatus.Completion;
                    _committedCompletion = completion as RCompletion;
                }

                base.InCommit = value;
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

        protected override void OnCompletionSessionCommitted(object sender, EventArgs eventArgs)
        {
            base.OnCompletionSessionCommitted(sender, eventArgs);
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

        public override void OnPostTypeChar(char typedCharacter)
        {
            // Dev12 864544: Razor can call into this method after the view has been closed.
            if (TextView == null || TextView.IsClosed)
            {
                return;
            }

            base.OnPostTypeChar(typedCharacter);
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