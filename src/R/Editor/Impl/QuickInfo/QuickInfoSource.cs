using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Completion;
using Microsoft.R.Editor.Completion.Engine;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Tree.Search;
using Microsoft.R.Support.Engine;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Packages;
using Microsoft.R.Support.RD.Parser;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Editor.QuickInfo
{
    sealed class QuickInfoSource : IQuickInfoSource
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        ITextBuffer _subjectBuffer;
        ITextView _textView;
        int _lastPosition = -1;

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
            string functionName;
            int signatureEnd = position;
            int parameterIndex = 0;

            if (_lastPosition == position)
                return;

            _lastPosition = position;

            bool foundPositions = SignatureHelp.GetParameterPositionsFromBuffer(
                                    document, position,
                                    out functionName, out parameterIndex, out signatureEnd);

            if (foundPositions && signatureEnd >= position)
            {
                applicableToSpan = snapshot.CreateTrackingSpan(position, signatureEnd - position, SpanTrackingMode.EdgeInclusive);

                EngineResponse response = RCompletionEngine.GetFunctionHelp(document.EditorTree.AstRoot, functionName).Result;
                if (response != null)
                {
                    if (!response.IsReady)
                    {
                        response.DataReady += OnDataReady;
                        _textView = session.TextView;
                    }
                    else
                    {
                        IFunctionInfo functionInfo = response.Data as IFunctionInfo;

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
                                text = signatureString + "\r\n" + functionInfo.Description;
                            }

                            quickInfoContent.Add(text);
                            break;
                        }
                    }
                }
            }
        }

        private void OnDataReady(object sender, object e)
        {
            IQuickInfoBroker broker = EditorShell.ExportProvider.GetExport<IQuickInfoBroker>().Value;
            broker.TriggerQuickInfo(_textView);
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
        }
        #endregion
    }
}
