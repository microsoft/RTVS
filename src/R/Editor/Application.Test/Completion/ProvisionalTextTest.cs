using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Completion
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public sealed class RProvisionalTextTest
    {
        [TestMethod]
        public void R_ProvisionalText1()
        {
            var script = new TestScript(RContentTypeDefinition.ContentType);

            try
            {
                script.Type("{");
                script.Type("(");
                script.Type("[");
                script.Type("\"");

                string expected = "{([\"\"])}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);

                REditorSettings.AutoFormat = false;

                script.Type("\"");
                script.Type("]");
                script.Type(")");
                script.Type("}");
                script.DoIdle(1000);

                expected = "{([\"\"])}";
                actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                script.Close();
            }
        }
    }
}
