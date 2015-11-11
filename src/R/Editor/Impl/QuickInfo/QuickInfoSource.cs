using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.QuickInfo;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Editor.QuickInfo {
    internal sealed class QuickInfoSource : IQuickInfoSource {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        private ITextBuffer _subjectBuffer;
        private int _lastPosition = -1;

        public QuickInfoSource(ITextBuffer subjectBuffer) {
            EditorShell.Current.CompositionService.SatisfyImportsOnce(this);

            _subjectBuffer = subjectBuffer;
            _subjectBuffer.Changed += OnTextBufferChanged;
        }

        void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            _lastPosition = -1;
        }

        #region IQuickInfoSource
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan) {
            applicableToSpan = null;

            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue)
                return;

            int position = triggerPoint.Value;

            if (_lastPosition == position)
                return;

            _lastPosition = position;
            ITextSnapshot snapshot = triggerPoint.Value.Snapshot;

            IREditorDocument document = REditorDocument.TryFromTextBuffer(_subjectBuffer);
            if (document != null) {
                // Document may be null in REPL window as projections are not
                // getting set immediately or may change as user moves mouse over.
                AugmentQuickInfoSession(document.EditorTree.AstRoot, position,
                                        session, quickInfoContent, out applicableToSpan,
                                        (object o) => RetriggerQuickInfoSession(o as IQuickInfoSession));
            }
        }

        internal bool AugmentQuickInfoSession(AstRoot ast, int position, IQuickInfoSession session,
                                              IList<object> quickInfoContent, out ITrackingSpan applicableToSpan,
                                              Action<object> retriggerAction) {
            int signatureEnd = position;
            applicableToSpan = null;

            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, ref position, out signatureEnd);
            if (!string.IsNullOrEmpty(functionName)) {
                ITextSnapshot snapshot = session.TextView.TextBuffer.CurrentSnapshot;

                position = Math.Min(signatureEnd, position);
                int start = Math.Min(position, snapshot.Length);
                int end = Math.Min(signatureEnd, snapshot.Length);

                applicableToSpan = snapshot.CreateTrackingSpan(Span.FromBounds(start, end), SpanTrackingMode.EdgeInclusive);

                IFunctionInfo functionInfo = FunctionIndex.GetFunctionInfo(functionName, retriggerAction, session);

                if (functionInfo != null && functionInfo.Signatures != null) {
                    foreach (ISignatureInfo sig in functionInfo.Signatures) {
                        string signatureString = sig.GetSignatureString(functionInfo.Name);
                        int wrapLength = Math.Min(SignatureInfo.MaxSignatureLength, signatureString.Length);
                        string text;

                        if (string.IsNullOrWhiteSpace(functionInfo.Description)) {
                            text = string.Empty;
                        } else {
                            /// VS may end showing very long tooltip so we need to keep 
                            /// description reasonably short: typically about
                            /// same length as the function signature.
                            text = signatureString + "\r\n" + functionInfo.Description.Wrap(wrapLength);
                        }

                        if (text.Length > 0) {
                            var content = text;
                            new QuickInfoWithLink(functionInfo.Name, functionInfo.Name, (o) => {
                                // Execute help command here
                            },
                            null,
                            () => {
                                if (session != null)
                                    session.Dismiss();
                            });

                            quickInfoContent.Add(content);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        private void RetriggerQuickInfoSession(IQuickInfoSession session) {
            if (session != null && !session.IsDismissed) {
                session.Dismiss();
            }

            _lastPosition = -1;
            IQuickInfoBroker broker = EditorShell.Current.ExportProvider.GetExport<IQuickInfoBroker>().Value;
            broker.TriggerQuickInfo(session.TextView, session.GetTriggerPoint(session.TextView.TextBuffer), session.TrackMouse);
        }

        #region IDisposable
        public void Dispose() {
        }
        #endregion
    }
}
