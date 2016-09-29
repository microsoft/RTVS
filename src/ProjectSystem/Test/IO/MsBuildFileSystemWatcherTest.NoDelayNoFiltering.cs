// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Test.StubFactories;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test.IO {
    public partial class MsBuildFileSystemWatcherTest {
        public class NoDelayNoFiltering : IAsyncLifetime {
            private readonly IFileSystem _fileSystem;
            private readonly ControlledTaskScheduler _taskScheduler;
            private readonly MsBuildFileSystemWatcher _fileSystemWatcher;
            private readonly IFileSystemWatcher _fileWatcher;
            private readonly IFileSystemWatcher _directoryWatcher;
            private readonly IFileSystemWatcher _attributesWatcher;

            private MsBuildFileSystemWatcher.Changeset _changeset;

            public NoDelayNoFiltering() {
                _fileSystem = Substitute.For<IFileSystem>();
                _fileSystem.GetFileAttributes(Arg.Any<string>()).Throws<FileNotFoundException>();
                _fileSystem.ToLongPath(Arg.Any<string>()).Returns(ci => ci[0]);
                _fileSystem.ToShortPath(Arg.Any<string>()).Returns(ci => ci[0]);
                var watchers = GetWatchersFromMsBuildFileSystemWatcher(_fileSystem);

                var fileSystemFilter = Substitute.For<IMsBuildFileSystemFilter>();
                fileSystemFilter.IsFileAllowed(Arg.Any<string>(), Arg.Any<FileAttributes>()).ReturnsForAnyArgs(true);
                fileSystemFilter.IsDirectoryAllowed(Arg.Any<string>(), Arg.Any<FileAttributes>()).ReturnsForAnyArgs(true);

                _taskScheduler = new ControlledTaskScheduler(SynchronizationContext.Current);

                DirectoryInfoStubFactory.Create(_fileSystem, ProjectDirectory);
                _fileSystemWatcher = new MsBuildFileSystemWatcher(ProjectDirectory, "*", 0, 0, _fileSystem, fileSystemFilter, Substitute.For<IActionLog>(), _taskScheduler);
                _fileSystemWatcher.Start();

                _fileWatcher = watchers.FileWatcher;
                _directoryWatcher = watchers.DirectoryWatcher;
                _attributesWatcher = watchers.AttributesWatcher;
            }

            public async Task InitializeAsync() {
                await _taskScheduler;
                _taskScheduler.Link(_fileSystemWatcher.SourceBlock, c => { _changeset = c; });
            }

            public Task DisposeAsync() {
                _fileSystemWatcher.Dispose();
                return Task.CompletedTask;
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\abc" }, new[] { "abc" })]
            [InlineData(new[] { @"Z:\abc\abc.cs" }, new[] { "abc.cs" })]
            [InlineData(new[] { @"Z:\abc\a\abc.cs", @"Z:\abc\a\def.cs", @"Z:\abc\a.cs" },
                        new[] { @"a\abc.cs", @"a\def.cs", "a.cs" })]
            public async Task FileAdded(string[] addedFiles, string[] expected) {
                using (_taskScheduler.Pause()) {
                    foreach (var path in addedFiles) {
                        FileInfoStubFactory.Create(_fileSystem, path);
                    }

                    RaiseCreated(_fileWatcher, addedFiles);
                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles(expected)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\a\abc.cs", @"Z:\abc\a\def.cs" },
                        new[] { @"Z:\abc\a\def.cs" },
                        new[] { @"a\def.cs" })]
            [InlineData(new[] { @"Z:\abc\a\abc.cs", @"Z:\abc\a\def.cs", @"Z:\abc\a.cs" },
                        new[] { @"Z:\abc\a\def.cs", @"Z:\abc\a.cs" },
                        new[] { @"a\def.cs", "a.cs" })]
            public async Task FileAdded_SomeFilesMissing(string[] addedFiles, string[] existingFiles, string[] expected) {
                foreach (var path in addedFiles) {
                    FileInfoStubFactory.Create(_fileSystem, path);
                }

                using (_taskScheduler.Pause()) {
                    foreach (var path in existingFiles) {
                        RaiseCreated(_fileWatcher, path);
                    }

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles(expected)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineArray(@"Z:\abc\abc")]
            [InlineArray(@"Z:\abc\abc.cs")]
            [InlineArray(@"Z:\abc\a\abc.cs", @"Z:\abc\a\def.cs", @"Z:\abc\a.cs")]
            public async Task FileAdded_AllFilesMissing(string[] addedFiles) {
                using (_taskScheduler.Pause()) {
                    foreach (var path in addedFiles) {
                        RaiseCreated(_fileWatcher, path);
                    }

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().BeNull();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\abc", @"Z:\abc\abc.cs" }, new[] { @"Z:\abc\abc" },
                        new[] { "abc.cs" }, new[] { @"abc" })]
            [InlineData(new[] { @"Z:\abc\a\x.r", @"Z:\abc\a\y.r" }, new[] { @"Z:\abc\a\z.r", @"Z:\abc\a\y.r" },
                        new[] { @"a\x.r" }, new[] { @"a\y.r", @"a\z.r" })]
            [InlineData(new[] { @"Z:\abc\a\x.r", @"Z:\abc\a\y.r" }, new[] { @"Z:\abc\a\z.r", @"Z:\abc\a\w.r" },
                        new[] { @"a\x.r", @"a\y.r" }, new[] { @"a\z.r", @"a\w.r" })]
            public async Task FileAdded_ThenRemoved(string[] createdFiles, string[] deletedFiles, string[] expectedAdded, string[] expectedRemoved) {
                await InjectFilesIntoWatcher(_fileWatcher, _fileSystem, deletedFiles, _taskScheduler);

                using (_taskScheduler.Pause()) {
                    foreach (var path in createdFiles) {
                        FileInfoStubFactory.Create(_fileSystem, path);
                        RaiseCreated(_fileWatcher, path);
                    }

                    foreach (var path in deletedFiles) {
                        FileInfoStubFactory.Delete(_fileSystem, path);
                        RaiseDeleted(_fileWatcher, path);
                    }

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles(expectedAdded)
                    .And.HaveRemovedFiles(expectedRemoved)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\abc", @"Z:\abc\abc.cs" }, new[] { @"Z:\abc\abc", @"Z:\abc\abc.cs" }, new[] { @"Z:\abc\def" },
                        new[] { @"abc", @"abc.cs" }, new[] { @"def\" })]
            [InlineData(new[] { @"Z:\abc\a\x.r", @"Z:\abc\b\y.r", @"Z:\abc\a\c\z.r", @"Z:\abc\b\d\w.r" }, new[] { @"Z:\abc\b\y.r" }, new[] { @"Z:\abc\a" },
                        new[] { @"b\y.r", }, new[] { @"a\" })]
            [InlineData(new[] { @"Z:\abc\a\x.r", @"Z:\abc\a\y.r", @"Z:\abc\a\b\z.r" }, new string[0], new[] { @"Z:\abc\a" },
                        new string[0], new[] { @"a\" })]
            public async Task FileAdded_DirectoryRemoved(string[] addedFiles, string[] existingFiles, string[] deletedDirectories, string[] expectedFiles, string[] expectedDirectories) {
                await InjectDirectoriesIntoWatcher(_directoryWatcher, _fileSystem, deletedDirectories, _taskScheduler);

                using (_taskScheduler.Pause()) {
                    foreach (var path in addedFiles) {
                        FileInfoStubFactory.Create(_fileSystem, path);
                    }

                    RaiseCreated(_fileWatcher, addedFiles);

                    foreach (var path in addedFiles) {
                        FileInfoStubFactory.Delete(_fileSystem, path);
                    }

                    foreach (var path in existingFiles) {
                        FileInfoStubFactory.Create(_fileSystem, path);
                    }

                    RaiseDeleted(_directoryWatcher, deletedDirectories);

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles(expectedFiles)
                    .And.HaveRemovedDirectories(expectedDirectories)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\abc" }, new[] { @"abc\" })]
            [InlineData(new[] { @"Z:\abc\abc.cs" }, new[] { @"abc.cs\" })]
            [InlineData(new[] { @"Z:\abc\a\abc", @"Z:\abc\b\abc", @"Z:\abc\a\bef" },
                        new[] { @"a\abc\", @"b\abc\", @"a\bef\" })]
            public async Task DirectoryAdded(string[] addedDirectories, string[] expected) {
                using (_taskScheduler.Pause()) {
                    foreach (var path in addedDirectories) {
                        DirectoryInfoStubFactory.Create(_fileSystem, path);
                        RaiseCreated(_directoryWatcher, path);
                    }

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;
                _changeset.Should().NotBeNull()
                    .And.HaveAddedDirectories(expected)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\a\abc.cs", @"Z:\abc\a\def" },
                        new[] { @"Z:\abc\a\def" },
                        new[] { @"a\def\" })]
            [InlineData(new[] { @"Z:\abc\a\abc", @"Z:\abc\b\abc", @"Z:\abc\a\bef" },
                        new[] { @"Z:\abc\b\abc", @"Z:\abc\a\bef" },
                        new[] { @"b\abc\", @"a\bef\" })]
            public async Task DirectoryAdded_SomeDirectoriesMissing(string[] addedDirectories, string[] existingDirectories, string[] expected) {
                foreach (var path in existingDirectories) {
                    DirectoryInfoStubFactory.Create(_fileSystem, path);
                }

                using (_taskScheduler.Pause()) {
                    foreach (var path in addedDirectories) {
                        RaiseCreated(_directoryWatcher, path);
                    }

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedDirectories(expected)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\a\abc", @"Z:\abc\a\def" },
                        new[] { @"a\def\", @"a\def\z.r\" },
                        new[] { @"a\def\x.r", @"a\def\y.r" })]
            [InlineData(new[] { @"Z:\abc\b\", @"Z:\abc\b\abc" },
                        new[] { @"b\", @"b\abc\" },
                        new[] { @"b\z.r" })]
            public async Task DirectoryAdded_DirectoryWithContent(string[] addedDirectories, string[] expectedDirectories, string[] expectedFiles) {
                const string projectDirectorySubtree = @"
[a]
  [def]
    x.r
    y.r
    [z.r]
[b]
  [abc]
  z.r
[c]
  [ghi]
  w.r
";
                DirectoryInfoStubFactory.FromIndentedString(_fileSystem, ProjectDirectory, projectDirectorySubtree);

                using (_taskScheduler.Pause()) {
                    RaiseCreated(_directoryWatcher, addedDirectories);

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles(expectedFiles)
                    .And.HaveAddedDirectories(expectedDirectories)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineArray(@"Z:\abc\abc")]
            [InlineArray(@"Z:\abc\abc.cs")]
            [InlineArray(@"Z:\abc\a\abc", @"Z:\abc\b\abc", @"Z:\abc\a\bef")]
            public async Task DirectoryAdded_AllDirectoriesMissing(string[] addedDirectories) {
                using (_taskScheduler.Pause()) {
                    RaiseCreated(_directoryWatcher, addedDirectories);

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().BeNull();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\a\b", @"Z:\abc\c" }, new[] { @"Z:\abc\a" },
                        new[] { @"c\" }, new[] { @"a\" })]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\a\b", @"Z:\abc\c" }, new[] { @"Z:\abc\a\b" },
                        new[] { @"a\", @"c\" }, new[] { @"a\b\" })]
            [InlineData(new[] { @"Z:\abc\a\b", @"Z:\abc\a\b\c" }, new[] { @"Z:\abc\a" },
                        new string[0], new[] { @"a\" })]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\a\b", @"Z:\abc\c" }, new[] { @"Z:\abc\a", @"Z:\abc\c" },
                        new string[0], new[] { @"a\", @"c\" })]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\a\b", @"Z:\abc\c" }, new[] { @"Z:\abc\c", @"Z:\abc\f" },
                        new[] { @"a\", @"a\b\" }, new[] { @"c\", @"f\" })]
            public async Task DirectoryAdded_ThenRemoved(string[] addedDirectories, string[] deletedDirectories, string[] expectedAdded, string[] expectedRemoved) {
                await InjectDirectoriesIntoWatcher(_directoryWatcher, _fileSystem, deletedDirectories, _taskScheduler);

                using (_taskScheduler.Pause()) {
                    foreach (var path in addedDirectories) {
                        DirectoryInfoStubFactory.Create(_fileSystem, path);
                    }

                    RaiseCreated(_directoryWatcher, addedDirectories);

                    foreach (var path in deletedDirectories) {
                        DirectoryInfoStubFactory.Delete(_fileSystem, path);
                    }

                    RaiseDeleted(_directoryWatcher, deletedDirectories);

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedDirectories(expectedAdded)
                    .And.HaveRemovedDirectories(expectedRemoved)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\a\b", @"Z:\abc\c" }, new[] { @"Z:\abc\a.r", @"Z:\abc\a\b.r" },
                        new[] { @"a\", @"a\b\", @"a\b\z.r\", @"c\", @"c\abc\" }, new[] { @"a.r", @"a\b.r" })]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\c" }, new[] { @"Z:\abc\c\a.r", @"Z:\abc\a\b.r" },
                        new[] { @"a\", @"a\b\", @"a\b\z.r\", @"c\", @"c\abc\" }, new[] { @"c\a.r", @"a\b.r" })]
            public async Task DirectoryAdded_FilesRemoved(string[] addedDirectories, string[] deletedFiles, string[] expectedAdded, string[] expectedRemoved) {
                const string projectDirectorySubtree = @"
[a]
  [b]
    x.r
    y.r
    [z.r]
[c]
  [abc]
  z.r";
                foreach (var path in deletedFiles) {
                    FileInfoStubFactory.Create(_fileSystem, path);
                }
                RaiseCreated(_fileWatcher, deletedFiles);
                await _taskScheduler;

                DirectoryInfoStubFactory.FromIndentedString(_fileSystem, ProjectDirectory, projectDirectorySubtree);
                using (_taskScheduler.Pause()) {
                    RaiseCreated(_directoryWatcher, addedDirectories);
                    RaiseDeleted(_fileWatcher, deletedFiles);

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedDirectories(expectedAdded)
                    .And.HaveRemovedFiles(expectedRemoved);
            }

            [Test]
            public async Task DirectoryAdded_SymlinkIgnored() {

                const string projectDirectorySubtree = @"
[a]
  [def]
    x.r
    y.r
    [z.r]
";
                // a\
                var a = (IDirectoryInfo)DirectoryInfoStubFactory.FromIndentedString(_fileSystem, ProjectDirectory, projectDirectorySubtree);
                // a\def\
                var def = (IDirectoryInfo)a.EnumerateFileSystemInfos().Last();
                // a\def\z.r\
                var z_r = (IDirectoryInfo)def.EnumerateFileSystemInfos().Last();
                z_r.Attributes.Returns(FileAttributes.Directory | FileAttributes.ReparsePoint);
                z_r.EnumerateFileSystemInfos().ThrowsForAnyArgs(new Exception());

                using (_taskScheduler.Pause()) {
                    RaiseCreated(_directoryWatcher, @"Z:\abc\a");
                }

                await _taskScheduler;

                var expectedFiles = new[] { @"a\def\x.r", @"a\def\y.r" };
                var expectedDirectories = new[] { @"a\", @"a\def\" };

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles(expectedFiles)
                    .And.HaveAddedDirectories(expectedDirectories)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\abc" }, new[] { "abc" })]
            [InlineData(new[] { @"Z:\abc\abc.cs" }, new[] { "abc.cs" })]
            [InlineData(new[] { @"Z:\abc\a\abc.cs", @"Z:\abc\a\def.cs", @"Z:\abc\a.cs" },
                        new[] { @"a\abc.cs", @"a\def.cs", "a.cs" })]
            public async Task FileRemoved(string[] deletedFiles, string[] expected) {
                await InjectFilesIntoWatcher(_fileWatcher, _fileSystem, deletedFiles, _taskScheduler);

                using (_taskScheduler.Pause()) {
                    foreach (var path in deletedFiles) {
                        RaiseDeleted(_fileWatcher, path);
                    }

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveRemovedFiles(expected)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\a\abc.cs", @"Z:\abc\a\def.cs" },
                        new[] { @"Z:\abc\a\def.cs" },
                        new[] { @"a\abc.cs", @"a\def.cs" })]
            [InlineData(new[] { @"Z:\abc\a\abc.cs", @"Z:\abc\a\def", @"Z:\abc\a.cs" },
                        new[] { @"Z:\abc\a\def", @"Z:\abc\a.cs" },
                        new[] { @"a\abc.cs", @"a\def", "a.cs" })]
            public async Task FileRemoved_SomeStillExists(string[] deletedFiles, string[] existingFiles, string[] expected) {
                await InjectFilesIntoWatcher(_fileWatcher, _fileSystem, deletedFiles, _taskScheduler);

                foreach (var path in existingFiles) {
                    FileInfoStubFactory.Create(_fileSystem, path);
                }

                using (_taskScheduler.Pause()) {
                    foreach (var path in deletedFiles) {
                        RaiseDeleted(_fileWatcher, path);
                    }

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveRemovedFiles(expected)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\a" }, new[] { @"a\" })]
            [InlineData(new[] { @"Z:\abc\abc.cs" }, new[] { @"abc.cs\" })]
            [InlineData(new[] { @"Z:\abc\a\b", @"Z:\abc\a\c.r", @"Z:\abc\a.cs" },
                        new[] { @"a\b\", @"a\c.r\", @"a.cs\" })]
            public async Task DirectoryRemoved(string[] deletedDirectories, string[] expected) {
                await InjectDirectoriesIntoWatcher(_directoryWatcher, _fileSystem, deletedDirectories, _taskScheduler);

                using (_taskScheduler.Pause()) {
                    RaiseDeleted(_directoryWatcher, deletedDirectories);
                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveRemovedDirectories(expected)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\a\b", @"Z:\abc\a" },
                        new[] { @"Z:\abc\a" },
                        new[] { @"a\b\", @"a\" })]
            [InlineData(new[] { @"Z:\abc\a\b.r", @"Z:\abc\a\def", @"Z:\abc\a.r" },
                        new[] { @"Z:\abc\a\def", @"Z:\abc\a.r" },
                        new[] { @"a\b.r\", @"a\def\", @"a.r\" })]
            public async Task DirectoryRemoved_SomeStillExists(string[] deletedDirectories, string[] existingDirectories, string[] expected) {
                await InjectDirectoriesIntoWatcher(_directoryWatcher, _fileSystem, deletedDirectories, _taskScheduler);
                foreach (var path in existingDirectories) {
                    DirectoryInfoStubFactory.Create(_fileSystem, path);
                }

                using (_taskScheduler.Pause()) {
                    RaiseDeleted(_directoryWatcher, deletedDirectories);
                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveRemovedDirectories(expected)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\b" }, new[] { @"Z:\abc\b", @"Z:\abc\abc.q" },
                        new string[0], new[] { @"b\abc\" })]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\c" }, new[] { @"Z:\abc\a", @"Z:\abc\a\b" },
                        new[] { @"c\" }, new[] { @"a\def\", @"a\def\z.r\" })]
            [InlineData(new[] { @"Z:\abc\a" }, new[] { @"Z:\abc\b" },
                        new[] { @"a\" }, new[] { @"b\", @"b\abc\" })]
            public async Task DirectoryRemoved_ThenAdded(string[] deletedDirectories, string[] createdDirectories, string[] expectedRemoved, string[] expectedAdded) {
                await InjectDirectoriesIntoWatcher(_directoryWatcher, _fileSystem, deletedDirectories, _taskScheduler);

                const string projectDirectorySubtree = @"
[a]
  [def]
    x.r
    y.r
    [z.r]
[b]
  [abc]
  z.r";
                using (_taskScheduler.Pause()) {
                    RaiseDeleted(_directoryWatcher, deletedDirectories);

                    DirectoryInfoStubFactory.FromIndentedString(_fileSystem, ProjectDirectory, projectDirectorySubtree);

                    RaiseCreated(_directoryWatcher, createdDirectories);

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedDirectories(expectedAdded)
                    .And.HaveRemovedDirectories(expectedRemoved);
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\abc" }, new[] { @"Z:\abc\abc", @"Z:\abc\abc.cs" },
                        new string[0], new[] { @"abc.cs" })]
            [InlineData(new[] { @"Z:\abc\a\x.r", @"Z:\abc\a\y.r" }, new[] { @"Z:\abc\a\z.r", @"Z:\abc\a\y.r" },
                        new[] { @"a\x.r" }, new[] { @"a\z.r" })]
            [InlineData(new[] { @"Z:\abc\a\x.r", @"Z:\abc\a\y.r" }, new[] { @"Z:\abc\b\x.r", @"Z:\abc\b\y.r" },
                        new[] { @"a\x.r", @"a\y.r" }, new[] { @"b\x.r", @"b\y.r" })]
            public async Task FileRemoved_ThenAdded(string[] deletedFiles, string[] createdFiles, string[] expectedRemoved, string[] expectedAdded) {
                await InjectFilesIntoWatcher(_fileWatcher, _fileSystem, deletedFiles, _taskScheduler);

                using (_taskScheduler.Pause()) {
                    RaiseDeleted(_fileWatcher, deletedFiles);

                    foreach (var path in createdFiles) {
                        FileInfoStubFactory.Create(_fileSystem, path);
                    }

                    RaiseCreated(_fileWatcher, createdFiles);

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles(expectedAdded)
                    .And.HaveRemovedFiles(expectedRemoved)
                    .And.NoOtherChanges();
            }

            [CompositeTest]
            [InlineData(new[] { @"Z:\abc\abc", @"Z:\abc\abc.cs" }, new[] { @"Z:\abc\a" },
                        new[] { @"abc", @"abc.cs" }, new[] { @"a\", @"a\def\", @"a\def\z.r\" }, new[] { @"a\def\x.r", @"a\def\y.r" })]
            [InlineData(new[] { @"Z:\abc\b\y.r", @"Z:\abc\b\z.r" }, new[] { @"Z:\abc\b" },
                        new[] { @"b\y.r" }, new[] { @"b\", @"b\abc\" }, new string[0])]
            public async Task FileRemoved_DirectoryAdded(string[] deletedFiles, string[] createdDirectories, string[] expectedRemovedFiles, string[] expectedAddedDirectories, string[] expectedAddedFiles) {
                const string projectDirectorySubtree = @"
[a]
  [def]
    x.r
    y.r
    [z.r]
[b]
  [abc]
  z.r";
                foreach (var path in deletedFiles) {
                    FileInfoStubFactory.Create(_fileSystem, path);
                }
                RaiseCreated(_fileWatcher, deletedFiles);
                await _taskScheduler;

                using (_taskScheduler.Pause()) {
                    RaiseDeleted(_fileWatcher, deletedFiles);
                    DirectoryInfoStubFactory.FromIndentedString(_fileSystem, ProjectDirectory, projectDirectorySubtree);
                    RaiseCreated(_directoryWatcher, createdDirectories);

                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles(expectedAddedFiles)
                    .And.HaveAddedDirectories(expectedAddedDirectories)
                    .And.HaveRemovedFiles(expectedRemovedFiles)
                    .And.NoOtherChanges();
            }

            [Test]
            public async Task FileWatcherError_Recovered() {
                var errorTask = EventTaskSources.MsBuildFileSystemWatcher.Error.Create(_fileSystemWatcher);
                using (_taskScheduler.Pause()) {
                    _fileWatcher.Error += Raise.Event<ErrorEventHandler>(_fileWatcher, new ErrorEventArgs(new InvalidOperationException()));
                }

                await _taskScheduler;
                errorTask.IsCompleted.Should().BeFalse();

                using (_taskScheduler.Pause()) {
                    FileInfoStubFactory.Create(_fileSystem, @"Z:\abc\abc.cs");
                    RaiseCreated(_fileWatcher, @"Z:\abc\abc.cs");
                }

                await _taskScheduler;

                _changeset.Should().NotBeNull()
                    .And.HaveAddedFiles("abc.cs")
                    .And.NoOtherChanges();
            }

            [Test]
            public async Task FileWatcherError_RaiseError() {
                _fileSystem.GetDirectoryInfo(@"Z:\abc\").Throws<InvalidOperationException>();
                var errorTask = EventTaskSources.MsBuildFileSystemWatcher.Error.Create(_fileSystemWatcher);

                using (_taskScheduler.Pause()) {
                    _fileWatcher.Error += Raise.Event<ErrorEventHandler>(_fileWatcher, new ErrorEventArgs(new InvalidOperationException()));
                }

                await _taskScheduler;
                errorTask.Status.Should().Be(TaskStatus.RanToCompletion);
            }
        }
    }
}
