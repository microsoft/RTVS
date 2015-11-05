using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FormatSelectionTest {
        [TestMethod]
        public void R_FormatSelection01() {
            string content = 
@"
while (TRUE) {
        if(x>1) {
   }
}";

            string expected =
@"
while (TRUE) {
    if (x > 1) {
    }
}";
            var script = new TestScript(content, RContentTypeDefinition.ContentType);

            try {
                script.Select(20, 21);
                script.Execute(VSConstants.VSStd2KCmdID.FORMATSELECTION, 50);
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }
    }
}
