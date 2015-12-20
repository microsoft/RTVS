using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Tests.Utility;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.IO {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class OpenFilesTest: UnitTestBase
    {
        //[TestMethod]
        [TestCategory("Interactive")]
        public void OpenFile_R()
        {
            OpenFileInEditor("lsfit.r");
        }

        //[TestMethod]
        [TestCategory("Interactive")]
        public void OpenFile_RD()
        {
            OpenFileInEditor("01.rd");
        }

        /// <summary>
        /// Opens file in an editor window
        /// </summary>
        /// <param name="fileName">File name</param>
        void OpenFileInEditor(string fileName)
        {
            string text = TestFiles.LoadFile(this.TestContext, fileName);

            try
            {
                EditorWindow.Create(text, fileName);
                EditorWindow.DoIdle(1000);
            }
            finally
            {
                EditorWindow.Close();
            }
        }
    }
}
