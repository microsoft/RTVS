// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class CompletionUtilities {
        public static bool IsStatementCompletionWindowActive(ITextView textView) {
            bool result = false;

            Debug.Assert(textView != null, "Null Text View");
            if (textView != null) {
                ICompletionBroker completionBroker = Vsshell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
                Debug.Assert(completionBroker != null, "Null completion broker.");
                if (completionBroker != null) {
                    result = completionBroker.IsCompletionActive(textView);
                }
            }

            return result;
        }

        internal static bool IsItemSelectedInStatementCompletion(ITextView textView) {
            bool result = false;

            Debug.Assert(textView != null, "Null Text View");
            if (textView != null) {
                ICompletionBroker completionBroker = Vsshell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
                Debug.Assert(completionBroker != null, "Null completion broker.");
                if (completionBroker != null && completionBroker.IsCompletionActive(textView)) {
                    var completionSessions = completionBroker.GetSessions(textView);
                    foreach (ICompletionSession completionSession in completionSessions) {
                        CompletionSet completionSet = completionSession.SelectedCompletionSet;
                        if (completionSet != null) {
                            result = completionSet.SelectionStatus.IsSelected;
                            if (result)
                                break;
                        }
                    }
                }
            }

            return result;
        }

        public static int GetPositionsOfSpan(IVsTextBuffer textStream, TextSpan ts, out int startPos, out int endPos) {
            int hr;

            startPos = 0;
            endPos = 0;

            hr = textStream.GetPositionOfLineIndex(ts.iStartLine, ts.iStartIndex, out startPos);
            if (hr == VSConstants.S_OK) {
                hr = textStream.GetPositionOfLineIndex(ts.iEndLine, ts.iEndIndex, out endPos);
            }

            return hr;
        }
    }
}
