// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Completions;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    internal static class RCompletionTestUtilities {
        public static void GetCompletions(IServiceContainer services, string content, int lineNumber, int column, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            var textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            GetCompletions(services, content, line.Start + column, completionSets, selectedRange);
        }

        public static void GetCompletions(IServiceContainer services, string content, int caretPosition, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            var ast = RParser.Parse(content);

            var textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var textView = new TextViewMock(textBuffer, caretPosition);

            if (selectedRange != null) {
                textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, selectedRange.Start, selectedRange.Length), false);
            }

            var completionSession = new CompletionSessionMock(textView, completionSets, caretPosition);
            var completionSource = new RCompletionSource(textBuffer, services);

            completionSource.PopulateCompletionList(caretPosition, completionSession, completionSets, ast);
        }
    }
}
