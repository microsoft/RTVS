using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Editor.QuickInfo
{
    sealed class QuickInfoSource : IQuickInfoSource
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        private ITextBuffer _subjectBuffer;
        private int _lastPosition = -1;

        public QuickInfoSource(ITextBuffer subjectBuffer)
        {
            EditorShell.CompositionService.SatisfyImportsOnce(this);

            _subjectBuffer = subjectBuffer;
            _subjectBuffer.Changed += OnTextBufferChanged;
        }

        void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            _lastPosition = -1;
        }

        #region IQuickInfoSource
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue)
                return;

            ITextSnapshot snapshot = triggerPoint.Value.Snapshot;
            EditorDocument document = EditorDocument.FromTextBuffer(_subjectBuffer);
            int position = triggerPoint.Value;
            int signatureEnd = position;

            if (_lastPosition == position)
                return;

            _lastPosition = position;

            string functionName = SignatureHelp.GetFunctionNameFromBuffer(document.EditorTree.AstRoot, position, out signatureEnd);
            if (!string.IsNullOrEmpty(functionName))
            {
                applicableToSpan = snapshot.CreateTrackingSpan(position, signatureEnd - position, SpanTrackingMode.EdgeInclusive);

                IFunctionInfo functionInfo = FunctionIndex.GetFunctionInfo(functionName,
                    (object o) => RetriggerQuickInfoSession(o as IQuickInfoSession), session);

                if (functionInfo != null && functionInfo.Signatures != null)
                {
                    foreach (ISignatureInfo sig in functionInfo.Signatures)
                    {
                        var signatureString = sig.GetSignatureString(functionInfo.Name);
                        string text;

                        if (string.IsNullOrWhiteSpace(functionInfo.Description))
                        {
                            text = string.Empty;
                        }
                        else
                        {
                            /// VS may end showing very long tooltip so we need to keep 
                            /// description reasonably short: typically about
                            /// same length as the function signature.
                            text = signatureString + "\r\n" + functionInfo.Description.Wrap(signatureString.Length);
                        }

                        quickInfoContent.Add(text);
                        break;
                    }
                }
            }
        }
        #endregion

        private void RetriggerQuickInfoSession(IQuickInfoSession session)
        {
            if (session != null && !session.IsDismissed)
            {
                session.Dismiss();
            }

            _lastPosition = -1;
            IQuickInfoBroker broker = EditorShell.ExportProvider.GetExport<IQuickInfoBroker>().Value;
            broker.TriggerQuickInfo(session.TextView, session.GetTriggerPoint(session.TextView.TextBuffer), session.TrackMouse);
        }

        #region IDisposable
        public void Dispose()
        {
        }
        #endregion
    }
}
