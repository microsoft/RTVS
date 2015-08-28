using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using TextChange = Microsoft.R.Editor.Tree.TextChange;

namespace Microsoft.R.Editor.Test.Tree
{

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TextChangesTest
    {
        [TestMethod()]
        public void TextChange_Test()
        {
            TextChange tc = new TextChange();
            Assert.IsTrue(tc.IsEmpty);
            Assert.AreEqual(TextChangeType.Trivial, tc.TextChangeType);

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

            Assert.AreEqual(0, tc1.OldRange.Length);
            Assert.AreEqual(2, tc1.NewRange.Length);
            Assert.AreEqual(2, tc1.Version);
            Assert.IsFalse(tc1.FullParseRequired);
            Assert.IsFalse(tc1.IsEmpty);
            Assert.IsTrue(tc1.IsSimpleChange);

            var tc2 = tc1.Clone() as TextChange;

            Assert.AreEqual(0, tc2.OldRange.Length);
            Assert.AreEqual(2, tc2.NewRange.Length);
            Assert.AreEqual(2, tc2.Version);
            Assert.IsFalse(tc2.FullParseRequired);
            Assert.IsFalse(tc2.IsEmpty);
            Assert.IsTrue(tc2.IsSimpleChange);

            tc1.Clear();

            Assert.AreEqual(0, tc1.OldRange.Length);
            Assert.AreEqual(0, tc1.NewRange.Length);
            Assert.IsNull(tc1.OldTextProvider);
            Assert.IsNull(tc1.NewTextProvider);
            Assert.IsTrue(tc1.IsEmpty);
            Assert.IsTrue(tc1.IsSimpleChange);

            Assert.AreEqual(0, tc2.OldRange.Length);
            Assert.AreEqual(2, tc2.NewRange.Length);
            Assert.AreEqual(2, tc2.Version);
            Assert.IsFalse(tc2.FullParseRequired);
            Assert.IsFalse(tc2.IsEmpty);
            Assert.IsTrue(tc2.IsSimpleChange);
        }
    }
}
