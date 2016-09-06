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
using Microsoft.R.Editor.Test.Utility;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    public class RCompletionSourceTestBase : FunctionIndexBasedTest {
        public RCompletionSourceTestBase(REditorMefCatalogFixture catalog) : base(catalog) { }

        protected void GetCompletions(string content, int lineNumber, int column, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            GetCompletions(content, line.Start + column, completionSets, selectedRange);
        }

        protected void GetCompletions(string content, int caretPosition, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            AstRoot ast = RParser.Parse(content);

            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer, caretPosition);

            if (selectedRange != null) {
                textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, selectedRange.Start, selectedRange.Length), false);
            }

            CompletionSessionMock completionSession = new CompletionSessionMock(textView, completionSets, caretPosition);
            RCompletionSource completionSource = new RCompletionSource(textBuffer, ExportProvider.GetExportedValue<ICoreShell>());

            completionSource.PopulateCompletionList(caretPosition, completionSession, completionSets, ast);
        }
    }
}
