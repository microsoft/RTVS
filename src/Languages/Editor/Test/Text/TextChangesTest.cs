using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Languages.Editor.Tests.Text {
    public class TextChangesTest {
        [Fact]
        [Trait("Languages.Editor", "")]
        public void TextChanges_DeleteInMiddle() {
            IList<TextChange> changes = BuildChangeList("abc", "adc");
            Assert.Equal(1, changes.Count);
            Assert.Equal(new TextChange(1, 1, "d"), changes[0]);
        }

        [Fact]
        [Trait("Languages.Editor", "")]
        public void TextChanges_DontBreakCRNL() {
            IList<TextChange> changes = BuildChangeList(" \n\n ", " \r\n ");
            Assert.Equal(1, changes.Count);
            Assert.Equal(new TextChange(1, 2, "\r\n"), changes[0]);
        }

        [Fact]
        [Trait("Languages.Editor", "")]
        public void TextChanges_DeleteOnlyAtStart() {
            IList<TextChange> changes = BuildChangeList("abc", "bc");
            Assert.Equal(1, changes.Count);
            Assert.Equal(new TextChange(0, 1, ""), changes[0]);
        }

        private IList<TextChange> BuildChangeList(string oldText, string newText) {
            return TextChanges.BuildChangeList(oldText, newText, Int32.MaxValue);
        }
    }
}
