using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test.IO {
    public class MsBuildFileSystemWatcherEntriesTest {
        private readonly MsBuildFileSystemWatcherEntries _entries;

        public MsBuildFileSystemWatcherEntriesTest() {
            _entries = new MsBuildFileSystemWatcherEntries();
/*
Basic relative path structure
├─[A]
│  ├─[A]
│  │  ├─[A]
│  │  │  ├─a.x
│  │  │  └─a.y
│  │  ├─[B]
│  │  │  ├─a.x
│  │  │  └─a.y
│  │  ├─c.x
│  │  └─c.y
│  ├─[B]
│  │  ├─[A]
│  │  ├─b.x
│  │  └─b.y
│  │
│  ├─c.x
│  └─c.y
├─[B]
│  ├─[A]
│  │  ├─[A]
│  │  │  ├─a.x
│  │  │  └─a.y
│  │  ├─[B]
│  │  │  ├─a.x
│  │  │  └─a.y
│  │  ├─c.x
│  │  └─c.y
│  └─[B]
│     ├─[A]
│     ├─b.x
│     └─b.y
├─.x
└─.y
*/
            _entries.AddDirectory(@"A");
            _entries.AddDirectory(@"A\A");
            _entries.AddDirectory(@"A\A\A");
            _entries.AddDirectory(@"A\A\B");
            _entries.AddDirectory(@"A\B");
            _entries.AddDirectory(@"A\B\A");
            _entries.AddDirectory(@"B");
            _entries.AddDirectory(@"B\A");
            _entries.AddDirectory(@"B\A\A");
            _entries.AddDirectory(@"B\A\B");
            _entries.AddDirectory(@"B\B");
            _entries.AddDirectory(@"B\B\A");

            _entries.AddFile(@"A\A\A\a.x");
            _entries.AddFile(@"A\A\A\a.y");
            _entries.AddFile(@"A\A\B\a.x");
            _entries.AddFile(@"A\A\B\a.y");
            _entries.AddFile(@"A\A\c.x");
            _entries.AddFile(@"A\A\c.y");
            _entries.AddFile(@"A\B\b.x");
            _entries.AddFile(@"A\B\b.y");
            _entries.AddFile(@"A\c.x");
            _entries.AddFile(@"A\c.y");
            _entries.AddFile(@"B\A\A\a.x");
            _entries.AddFile(@"B\A\A\a.y");
            _entries.AddFile(@"B\A\B\a.x");
            _entries.AddFile(@"B\A\B\a.y");
            _entries.AddFile(@"B\A\c.x");
            _entries.AddFile(@"B\A\c.y");
            _entries.AddFile(@"B\B\b.x");
            _entries.AddFile(@"B\B\b.y");
            _entries.AddFile(@".x");
            _entries.AddFile(@".y");

            _entries.ProduceChangeset();
        }

        [CompositeTest]
        [InlineArray(@".x", @"A\D\b.y")]
        [InlineArray(@".x", @"a.x", @"A\D\b.y", @"A\D\.y")]
        public void FileRenameRoundtrip(string[] renames) {
            for (var i = 0; i < renames.Length - 1; i++) {
                _entries.RenameFile(renames[i], renames[i + 1]);
            }
            _entries.RenameFile(renames[renames.Length - 1], renames[0]);

            var changeset = _entries.ProduceChangeset();
            changeset.Should().NotBeNull().And.NoOtherChanges();
        }

        [CompositeTest]
        [InlineArray(@"A\", @"B\C\")]
        [InlineArray(@"A\B", @"C\")]
        [InlineArray(@"A\B", @"A\C", @"B\C")]
        public void DirectoryRenameRoundtrip(string[] renames) {
            for (var i = 0; i < renames.Length - 1; i++) {
                _entries.RenameDirectory(renames[i], renames[i + 1]);
            }
            _entries.RenameDirectory(renames[renames.Length - 1], renames[0]);

            var changeset = _entries.ProduceChangeset();
            changeset.Should().NotBeNull().And.NoOtherChanges();
        }

        [CompositeTest]
        [InlineData(new[] { @"A" }, 
                    new[] { @"C" },
                    new[] { @"A\", @"A\A\", @"A\A\A\", @"A\A\B\", @"A\B\", @"A\B\A\" },
                    new[] { @"C\", @"C\A\", @"C\A\A\", @"C\A\B\", @"C\B\", @"C\B\A\" },
                    new[] { @"A\A\A\a.x", @"A\A\A\a.y", @"A\A\B\a.x", @"A\A\B\a.y", @"A\A\c.x", @"A\A\c.y", @"A\B\b.x", @"A\B\b.y", @"A\c.x", @"A\c.y" },
                    new[] { @"C\A\A\a.x", @"C\A\A\a.y", @"C\A\B\a.x", @"C\A\B\a.y", @"C\A\c.x", @"C\A\c.y", @"C\B\b.x", @"C\B\b.y", @"C\c.x", @"C\c.y" })]
        [InlineData(new[] { @"A", @"C", @"D" },
                    new[] { @"C", @"D", @"E" },
                    new[] { @"A\", @"A\A\", @"A\A\A\", @"A\A\B\", @"A\B\", @"A\B\A\" },
                    new[] { @"E\", @"E\A\", @"E\A\A\", @"E\A\B\", @"E\B\", @"E\B\A\" },
                    new[] { @"A\A\A\a.x", @"A\A\A\a.y", @"A\A\B\a.x", @"A\A\B\a.y", @"A\A\c.x", @"A\A\c.y", @"A\B\b.x", @"A\B\b.y", @"A\c.x", @"A\c.y" },
                    new[] { @"E\A\A\a.x", @"E\A\A\a.y", @"E\A\B\a.x", @"E\A\B\a.y", @"E\A\c.x", @"E\A\c.y", @"E\B\b.x", @"E\B\b.y", @"E\c.x", @"E\c.y" })]
        [InlineData(new[] { @"A\B", @"A\A", @"A\B" },
                    new[] { @"A\A\C", @"A\B", @"D" },
                    new[] { @"A\A\", @"A\A\A\", @"A\A\B\", @"A\B\", @"A\B\A\"},
                    new[] { @"D\", @"D\A\", @"D\B\", @"D\C\", @"D\C\A\" },
                    new[] { @"A\A\A\a.x", @"A\A\A\a.y", @"A\A\B\a.x", @"A\A\B\a.y", @"A\B\b.x", @"A\B\b.y", @"A\A\c.x", @"A\A\c.y" },
                    new[] { @"D\A\a.x", @"D\A\a.y", @"D\B\a.x", @"D\B\a.y", @"D\C\b.x", @"D\C\b.y", @"D\c.x", @"D\c.y" })]
        public void RenameDirectoryMultipleTimes(string[] from, string[] to, string[] expectedFromDirectories, string[] expectedToDirectories, string[] expectedFromFiles, string[] expectedToFiles) {
            for (var i = 0; i < from.Length; i++) {
                _entries.RenameDirectory(from[i], to[i]);
            }

            var changeset = _entries.ProduceChangeset();
            changeset.Should().NotBeNull()
                .And.HaveRenamedFiles(expectedFromFiles, expectedToFiles)
                .And.HaveRenamedDirectories(expectedFromDirectories, expectedToDirectories)
                .And.NoOtherChanges();
        }

        [Test]
        public void Directory_RenameAddRenameRename() {
            _entries.RenameDirectory(@"B\B", @"C");
            _entries.AddDirectory(@"B\B");
            _entries.RenameDirectory(@"B\B", @"B\D");
            _entries.RenameDirectory(@"A\B", @"B\B");

            var changeset = _entries.ProduceChangeset();
            changeset.Should().NotBeNull()
                .And.HaveAddedDirectories(@"B\D\")
                .And.HaveRenamedDirectories(new[] {@"A\B\", @"A\B\A\", @"B\B\", @"B\B\A\"}, new[] { @"B\B\", @"B\B\A\", @"C\", @"C\A\"})
                .And.HaveRenamedFiles(new[] {@"A\B\b.x", @"A\B\b.y", @"B\B\b.x", @"B\B\b.y"}, new[] { @"B\B\b.x", @"B\B\b.y", @"C\b.x", @"C\b.y"})
                .And.NoOtherChanges();
        }

        [Test]
        public void TripleDirectoryRenameAdd() {
            _entries.RenameDirectory(@"B\B", @"B\C");
            _entries.AddDirectory(@"B\B");
            _entries.RenameDirectory(@"B\B", @"B\D");
            _entries.AddDirectory(@"B\B");
            _entries.RenameDirectory(@"B\C", @"A\C");
            _entries.AddDirectory(@"B\C");

            var changeset = _entries.ProduceChangeset();
            changeset.Should().NotBeNull()
                .And.HaveAddedDirectories(@"B\B\", @"B\C\", @"B\D\")
                .And.HaveRenamedDirectories(new[] {@"B\B\", @"B\B\A\"}, new[] {@"A\C\", @"A\C\A\"})
                .And.HaveRenamedFiles(new[] {@"B\B\b.x", @"B\B\b.y"}, new[] {@"A\C\b.x", @"A\C\b.y"})
                .And.NoOtherChanges();
        }

        [Test]
        public void File_DeleteAddRenameRenameAddRename() {
            _entries.DeleteFile(@".x");
            _entries.AddFile(@"C\a.x");
            _entries.RenameFile(@"C\a.x", @".x");
            _entries.RenameFile(@".x", @"D\a.x");
            _entries.AddFile(@"D\b.y");
            _entries.RenameFile(@"D\b.y", @".x");

            var changeset = _entries.ProduceChangeset();
            changeset.Should().NotBeNull()
                .And.HaveAddedFiles(@".x")
                .And.HaveRenamedFiles(new [] { @".x" }, new[] { @"D\a.x" })
                .And.NoOtherChanges();
        }
    }
}
