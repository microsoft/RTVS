using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class CurrentDirectoryTest {
        [Test]
        [Category.Repl]
        public void CurrentDirectoryTest_DefaultDirectoryTest() {
            string actual;
            using (new RHostScript()) {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                cmd.InitializationTask.Wait();
                actual = cmd.GetRWorkingDirectoryAsync().Result;
            };

            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            actual.Should().Be(myDocs);
        }

        [Test]
        [Category.Repl]
        public void CurrentDirectoryTest_SetDirectoryTest() {
            string dir = "c:\\";
            string actual;
            using (new RHostScript()) {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                cmd.InitializationTask.Wait();
                cmd.SetDirectory(dir).Wait();
                actual = cmd.GetRWorkingDirectoryAsync().Result;
            }

            actual.Should().Be(dir);
        }

        [Test]
        [Category.Repl]
        public void CurrentDirectoryTest_GetFriendlyNameTest() {
            string actual;
            using (new RHostScript()) {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                cmd.InitializationTask.Wait();
                actual = cmd.GetFriendlyDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            };

            actual.Should().Be("~");
        }

        [Test]
        [Category.Repl]
        public void CurrentDirectoryTest_GetFullPathNameTest() {
            string dir;
            using (new RHostScript()) {
                WorkingDirectoryCommand cmd = new WorkingDirectoryCommand();
                cmd.InitializationTask.Wait();
                dir = cmd.GetFullPathName("~");
            }

            string actual = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            actual.Should().Be(dir);
        }
    }
}
