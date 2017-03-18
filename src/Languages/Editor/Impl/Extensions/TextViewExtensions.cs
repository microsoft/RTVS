// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor {
    public static class TextViewExtensions {
        public static bool IsStatementCompletionWindowActive(this ITextView textView, ICoreShell coreShell) {
            bool result = false;
            if (textView != null) {
                var completionBroker = coreShell.GetService<ICompletionBroker>();
                result = completionBroker.IsCompletionActive(textView);
            }
            return result;
        }

        public static SnapshotPoint? MapUpToView(this ITextView textView, SnapshotPoint position) {
            if (textView.BufferGraph == null) {
                // Unit test case
                return position;
            }
            return textView.BufferGraph.MapUpToBuffer(
                position,
                PointTrackingMode.Positive,
                PositionAffinity.Successor,
                textView.TextBuffer
             );
        }
    }
}
