using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CurrentDirectoryTest {
        [TestMethod]
        [TestCategory("Repl")]
        public void CurrentDirectoryTest_DefaultDirectoryTest() {
            string actual = null;
            SequentialHostTestExecutor.ExecuteTest(() => {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                actual = cmd.GetRWorkingDirectoryAsync().Result;
            });

            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Assert.AreEqual(myDocs, actual);
        }

        [TestMethod]
        [TestCategory("Repl")]
        public void CurrentDirectoryTest_UserDirectoryTest() {
            string actual = null;
            SequentialHostTestExecutor.ExecuteTest(() => {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                actual = cmd.GetRUserDirectoryAsync().Result;
            });

            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Assert.AreEqual(myDocs, actual);
        }

        [TestMethod]
        [TestCategory("Repl")]
        public void CurrentDirectoryTest_SetDirectoryTest() {
            string dir = "c:\\";
            string actual = null;
            SequentialHostTestExecutor.ExecuteTest(() => {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                cmd.SetDirectory(dir).Wait();
                actual = cmd.GetRWorkingDirectoryAsync().Result;
            });

            Assert.AreEqual(dir, actual);
        }

        [TestMethod]
        [TestCategory("Repl")]
        public void CurrentDirectoryTest_GetFriendlyNameTest() {
            string actual = null;
            SequentialHostTestExecutor.ExecuteTest(() => {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                actual = cmd.GetFriendlyDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            });

            Assert.AreEqual("~", actual);
        }

        [TestMethod]
        [TestCategory("Repl")]
        public void CurrentDirectoryTest_GetFullPathNameTest() {
            string dir = null;

            SequentialHostTestExecutor.ExecuteTest(() => {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                dir = cmd.GetFullPathName("~");
            });

            string actual = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Assert.AreEqual(dir, actual);
        }
    }
}
