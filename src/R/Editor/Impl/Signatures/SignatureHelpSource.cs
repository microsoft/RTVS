using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Completion.Engine;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Support.Engine;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.RD.Parser;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Signatures
{
    sealed class SignatureHelpSource : ISignatureHelpSource
    {
        ITextBuffer _textBuffer;
        ITextView _textView;

        public SignatureHelpSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            ServiceManager.AddService<SignatureHelpSource>(this, textBuffer);
        }

        #region ISignatureHelpSource
        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            if (!REditorSettings.SignatureHelpEnabled)
                return;

            var document = EditorDocument.FromTextBuffer(_textBuffer);
            Debug.Assert(document != null);

            if (document != null)
            {
                ITextSnapshot snapshot = _textBuffer.CurrentSnapshot;
                int position = session.GetTriggerPoint(_textBuffer).GetPosition(snapshot);

                string functionName = string.Empty;
                int signatureEnd = position;
                int parameterIndex = 0;

                // Retrieve parameter positions from the current text buffer snapshot
                if (SignatureHelp.GetParameterPositionsFromBuffer(document, position, out functionName, out parameterIndex, out signatureEnd))
                {
                    if (signatureEnd >= position)
                    {
                        ITrackingSpan applicableToSpan = snapshot.CreateTrackingSpan(position, signatureEnd - position, SpanTrackingMode.EdgeInclusive);

                        // Get collection of function signatures from documentation (parsed RD file)
                        EngineResponse response = RCompletionEngine.GetFunctionHelp(document.EditorTree.AstRoot, functionName).Result;
                        if (!response.IsReady)
                        {
                            response.DataReady += OnDataReady;
                            _textView = session.TextView;
                        }
                        else
                        {
                            IFunctionInfo functionInfo = response.Data as IFunctionInfo;

                            foreach (ISignatureInfo signatureInfo in functionInfo.Signatures)
                            {
                                var signature = CreateSignature(session, _textBuffer, functionInfo, signatureInfo, applicableToSpan, position);
                                signatures.Add(signature);
                            }
                        }
                    }
                }
            }
        }

        private void OnDataReady(object sender, object e)
        {
            ISignatureHelpBroker broker = EditorShell.ExportProvider.GetExport<ISignatureHelpBroker>().Value;
            broker.TriggerSignatureHelp(_textView);
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            if (session.Signatures.Count > 0)
            {
                ITrackingSpan applicableToSpan = session.Signatures[0].ApplicableToSpan;
                string text = applicableToSpan.GetText(applicableToSpan.TextBuffer.CurrentSnapshot);

                var typedText = text.Trim();
                foreach (var sig in session.Signatures)
                {
                    var jsSig = sig as SignatureHelp;

                    if (jsSig != null && jsSig.FunctionName.StartsWith(text))
                    {
                        return sig;
                    }
                }
            }

            return null;
        }
        #endregion

        private SignatureHelp CreateSignature(ISignatureHelpSession session, ITextBuffer textBuffer,
                                       IFunctionInfo functionInfo, ISignatureInfo signatureInfo,
                                       ITrackingSpan span, int position)
        {
            SignatureHelp sig = new SignatureHelp(session, textBuffer, functionInfo.Name, functionInfo.Description);
            List<IParameter> paramList = new List<IParameter>();

            sig.ApplicableToSpan = span;

            var locusPoints = new List<int>();
            sig.Content = signatureInfo.GetSignatureString(functionInfo.Name, locusPoints);

            Debug.Assert(locusPoints.Count == signatureInfo.Arguments.Count + 1);

            for (int i = 0; i < signatureInfo.Arguments.Count; i++)
            {
                IArgumentInfo p = signatureInfo.Arguments[i];
                if (p != null)
                {
                    var locusStart = locusPoints[i];
                    var locusLength = locusPoints[i + 1] - locusStart - 2;
                    var locus = new Span(locusStart, locusLength);

                    paramList.Add(new SignatureParameter(p.Description, locus, p.Name, sig));
                }
            }

            sig.Parameters = new ReadOnlyCollection<IParameter>(paramList);
            sig.ComputeCurrentParameter(position);

            return sig;
        }

        #region IDisposable
        public void Dispose()
        {
            _textBuffer = null;
        }
        #endregion
    }
}
