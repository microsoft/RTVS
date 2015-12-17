using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Validation {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ErrorTagTest {
        [TestMethod]
        [TestCategory("Interactive")]
        public void R_ErrorTagsTest01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                // Force tagger creation
                var tagSpans = script.GetErrorTagSpans();

                script.Type("x <- {");
                script.Delete();
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                string errorTags = script.WriteErrorTags(tagSpans);
                Assert.AreEqual("[5 - 6] } expected\r\n", errorTags);

                script.Type("}");
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                Assert.AreEqual(0, tagSpans.Count);
            }
        }
    }
}
