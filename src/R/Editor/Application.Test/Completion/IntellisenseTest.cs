using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Completion {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class IntellisenseTest {
        [TestMethod]
        public void R_KeywordIntellisense() {
            var script = new TestScript(RContentTypeDefinition.ContentType);

            try {
                script.Type("funct");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "function";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }

        [TestMethod]
        public void R_LibraryIntellisense() {
            var script = new TestScript(RContentTypeDefinition.ContentType);

            try {
                script.Type("library(ut");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "library(utils)";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }

        [TestMethod]
        public void R_RequireIntellisense() {
            var script = new TestScript(RContentTypeDefinition.ContentType);

            try {
                script.Type("require(uti");
                script.DoIdle(100);
                script.Type("{TAB}");

                string expected = "require(utils)";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }
    }
}
