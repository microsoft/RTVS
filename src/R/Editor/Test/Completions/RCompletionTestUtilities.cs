// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Completion;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    internal static class RCompletionTestUtilities {
        public static void GetCompletions(ICoreShell coreShell, string content, int lineNumber, int column, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            GetCompletions(coreShell, content, line.Start + column, completionSets, selectedRange);
        }

        public static void GetCompletions(ICoreShell coreShell, string content, int caretPosition, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            AstRoot ast = RParser.Parse(content);

            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer, caretPosition);

            if (selectedRange != null) {
                textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, selectedRange.Start, selectedRange.Length), false);
            }

            CompletionSessionMock completionSession = new CompletionSessionMock(textView, completionSets, caretPosition);
            RCompletionSource completionSource = new RCompletionSource(textBuffer, coreShell);

            completionSource.PopulateCompletionList(caretPosition, completionSession, completionSets, ast);
        }
    }
}
