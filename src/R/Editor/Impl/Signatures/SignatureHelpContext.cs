using System;
using System.Diagnostics;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Signatures {
    internal static class SignatureHelper {
        /// <summary>
        /// Determines if current caret position is in the same function
        /// argument list as before or is it a different one and signature 
        /// help session should be dismissed and re-triggered. This is helpful
        /// when user types nested function calls such as 'a(b(c(...), d(...)))'
        /// </summary>
        public static bool IsSameSignatureContext(ITextView textView, ITextBuffer subjectBuffer) {
            ISignatureHelpBroker signatureBroker = EditorShell.Current.ExportProvider.GetExportedValue<ISignatureHelpBroker>();
            var sessions = signatureBroker.GetSessions(textView);
            Debug.Assert(sessions.Count < 2);
            if (sessions.Count == 1) {
                IFunctionInfo sessionFunctionInfo = null;
                sessions[0].Properties.TryGetProperty<IFunctionInfo>("functionInfo", out sessionFunctionInfo);

                if (sessionFunctionInfo != null) {
                    try {
                        IREditorDocument document = REditorDocument.FromTextBuffer(textView.TextBuffer);
                        document.EditorTree.EnsureTreeReady();

                        ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(
                            document.EditorTree.AstRoot, subjectBuffer.CurrentSnapshot,
                            textView.Caret.Position.BufferPosition);

                        return parametersInfo != null && parametersInfo.FunctionName == sessionFunctionInfo.Name;
                    } catch (Exception) { }
                }
            }

            return false;
        }
    }
}
