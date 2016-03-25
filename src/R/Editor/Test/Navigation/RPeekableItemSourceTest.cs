// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Completion;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Navigation {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class RPeekableItemSourceTest {

        private void GetCompletions(string content, int position, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            AstRoot ast = RParser.Parse(content);

            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer, position);

            if (selectedRange != null) {
                textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, selectedRange.Start, selectedRange.Length), false);
            }

            CompletionSessionMock completionSession = new CompletionSessionMock(textView, completionSets, position);
            RCompletionSource completionSource = new RCompletionSource(textBuffer);

            completionSource.PopulateCompletionList(position, completionSession, completionSets, ast);
        }
    }
}
