// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.R.Editor.Document;
using Microsoft.R.Support.Help;
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
        public static bool IsSameSignatureContext(ITextView textView, ITextBuffer subjectBuffer, ISignatureHelpBroker signatureBroker) {
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
