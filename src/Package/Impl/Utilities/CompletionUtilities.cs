using System.Diagnostics;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class CompletionUtilities {
        public static IVsExpansionManager GetExpansionManager() {
            IVsExpansionManager expansionManager = null;

            IVsTextManager2 textManager2 = VsAppShell.Current.GetGlobalService<IVsTextManager2>(typeof(SVsTextManager));

            Debug.Assert(textManager2 != null, "Null text manager in ExpansionClient");
            if (textManager2 != null) {
                int expansionManagerResult = textManager2.GetExpansionManager(out expansionManager);
                Debug.Assert((expansionManagerResult == VSConstants.S_OK) && (expansionManager != null),
                    "Error getting ExpansionManager in ExpansionClient");
            }

            return expansionManager;
        }

        public static bool IsStatementCompletionWindowActive(ITextView textView) {
            bool result = false;

            Debug.Assert(textView != null, "Null Text View");
            if (textView != null) {
                ICompletionBroker completionBroker = VsAppShell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
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
                ICompletionBroker completionBroker = VsAppShell.Current.ExportProvider.GetExport<ICompletionBroker>().Value;
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
