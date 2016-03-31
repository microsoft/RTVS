// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using TextChange = Microsoft.R.Editor.Tree.TextChange;

namespace Microsoft.R.Editor.Test.Tree {
    [ExcludeFromCodeCoverage]
    [Category.R.EditorTree]
    public class TextChangesTest {
        [Test]
        public void TextChange_Test() {
            TextChange tc = new TextChange();
            tc.IsEmpty.Should().BeTrue();
            tc.TextChangeType.Should().Be(TextChangeType.Trivial);

            string content = "23456789";
            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ITextSnapshot oldSnapshot = textBuffer.CurrentSnapshot;

            textBuffer.Insert(0, "1");
            ITextSnapshot newSnapshot1 = textBuffer.CurrentSnapshot;

            textBuffer.Insert(0, "0");
            ITextSnapshot newSnapshot2 = textBuffer.CurrentSnapshot;

            tc.OldTextProvider = new TextProvider(oldSnapshot);
            tc.NewTextProvider = new TextProvider(newSnapshot1);

            tc.OldRange = new TextRange(0, 0);
            tc.NewRange = new TextRange(0, 1);

            var tc1 = new TextChange(tc, new TextProvider(newSnapshot2));

            tc1.ShouldBeEquivalentTo(new {
                OldRange = new { Length = 0 },
                NewRange = new { Length = 2 },
                Version = 2,
                FullParseRequired = false,
                IsEmpty = false,
                IsSimpleChange = true
            }, o => o.ExcludingMissingMembers());

            var tc2 = tc1.Clone() as TextChange;

            tc2.ShouldBeEquivalentTo(new {
                OldRange = new { Length = 0 },
                NewRange = new { Length = 2 },
                Version = 2,
                FullParseRequired = false,
                IsEmpty = false,
                IsSimpleChange = true
            }, o => o.ExcludingMissingMembers());

            tc1.Clear();

            tc1.ShouldBeEquivalentTo(new {
                OldRange = new { Length = 0 },
                NewRange = new { Length = 0 },
                OldTextProvider = (ITextProvider)null,
                NewTextProvider = (ITextProvider)null,
                IsEmpty = true,
                IsSimpleChange = true
            }, o => o.ExcludingMissingMembers());

            tc2.ShouldBeEquivalentTo(new {
                OldRange = new { Length = 0 },
                NewRange = new { Length = 2 },
                Version = 2,
                FullParseRequired = false,
                IsEmpty = false,
                IsSimpleChange = true
            }, o => o.ExcludingMissingMembers());
        }
    }
}
