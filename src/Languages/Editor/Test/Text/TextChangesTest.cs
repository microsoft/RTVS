using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Editor.Test.Text
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TextChangesTest
    {
        [TestMethod()]
        public void TextChanges_DeleteInMiddle()
        {
            IList<TextChange> changes = BuildChangeList("abc", "adc");
            Assert.AreEqual(changes.Count, 1);
            Assert.AreEqual(changes[0], new TextChange(1, 1, "d"));
        }

        [TestMethod()]
        public void TextChanges_DontBreakCRNL()
        {
            IList<TextChange> changes = BuildChangeList(" \n\n ", " \r\n ");
            Assert.AreEqual(changes.Count, 1);
            Assert.AreEqual(changes[0], new TextChange(1, 2, "\r\n"));
        }

        [TestMethod()]
        public void TextChanges_DeleteOnlyAtStart()
        {
            IList<TextChange> changes = BuildChangeList("abc", "bc");
            Assert.AreEqual(changes.Count, 1);
            Assert.AreEqual(changes[0], new TextChange(0, 1, ""));
        }

        private IList<TextChange> BuildChangeList(string oldText, string newText)
        {
            Func<char, bool> isDelimiter = ((c) => { return false; });
            int maxMilliseconds = Int32.MaxValue;

            return TextChanges.BuildChangeList(oldText, newText, maxMilliseconds, isDelimiter);
        }
    }
}
