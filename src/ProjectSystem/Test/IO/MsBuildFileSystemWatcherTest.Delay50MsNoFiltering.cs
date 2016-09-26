// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
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
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test.IO {
    [ExcludeFromCodeCoverage]
    [Category.Project.FileSystemMirror]
    public partial class MsBuildFileSystemWatcherTest {
        public class Delay50MsNoFiltering : IAsyncLifetime {
            private readonly IFileSystem _fileSystem;
            private readonly IMsBuildFileSystemFilter _fileSystemFilter;
            private readonly ControlledTaskScheduler _taskScheduler;
            private readonly MsBuildFileSystemWatcher _fileSystemWatcher;
            private readonly IFileSystemWatcher _fileWatcher;
            private readonly IFileSystemWatcher _directoryWatcher;
            private readonly IFileSystemWatcher _attributesWatcher;
            private MsBuildFileSystemWatcher.Changeset _changeset;

            public Delay50MsNoFiltering() {
                _fileSystem = Substitute.For<IFileSystem>();
                var watchers = GetWatchersFromMsBuildFileSystemWatcher(_fileSystem);

                _fileSystemFilter = Substitute.For<IMsBuildFileSystemFilter>();
                _fileSystemFilter.IsFileAllowed(Arg.Any<string>(), Arg.Any<FileAttributes>()).ReturnsForAnyArgs(true);
                _fileSystemFilter.IsDirectoryAllowed(Arg.Any<string>(), Arg.Any<FileAttributes>()).ReturnsForAnyArgs(true);

                _taskScheduler = new ControlledTaskScheduler(SynchronizationContext.Current);

                _fileSystemWatcher = new MsBuildFileSystemWatcher(ProjectDirectory, "*", 50, 50, _fileSystem, _fileSystemFilter, Substitute.For<IActionLog>(), _taskScheduler);
                _taskScheduler.Link(_fileSystemWatcher.SourceBlock, c => { _changeset = c; });

                _fileSystemWatcher.Start();
                _fileWatcher = watchers.FileWatcher;
                _directoryWatcher = watchers.DirectoryWatcher;
                _attributesWatcher = watchers.AttributesWatcher;
            }

            public async Task InitializeAsync() {
                await _taskScheduler;
                _changeset = null;
            }

            public Task DisposeAsync() {
                _fileSystemWatcher.Dispose();
                return Task.CompletedTask;
            }

            [CompositeTest(Skip = "await Task.Delay has side effects, requires different approach")]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\a\b", @"Z:\abc\c" }, new[] { @"Z:\abc\a" },
                        new[] { @"c\" }, new[] { @"a\" })]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\a\b", @"Z:\abc\c" }, new[] { @"Z:\abc\a\b" },
                        new[] { @"a\", @"c\" }, new[] { @"a\b\" })]
            [InlineData(new[] { @"Z:\abc\a\b", @"Z:\abc\a\b\c" }, new[] { @"Z:\abc\a" },
                        new string[0], new[] { @"a\" })]
            [InlineData(new[] { @"Z:\abc\a", @"Z:\abc\a\b", @"Z:\abc\c" }, new[] { @"Z:\abc\a", @"Z:\abc\c" },
                        new string[0], new[] { @"a\", @"c\" })]
            public async Task DirectoryAdded_ThenRemoved(string[] addedDirectories, string[] deletedDirectories, string[] expectedAdded, string[] expectedRemoved) {
                using (_taskScheduler.Pause()) {
                    foreach (var path in addedDirectories) {
                        DirectoryInfoStubFactory.Create(_fileSystem, path);
                    }

                    RaiseCreated(_directoryWatcher, addedDirectories);
                    _taskScheduler.ScheduledTasksCount.Should().Be(1);
                }

                await Task.Delay(20);

                foreach (var path in deletedDirectories) {
                    DirectoryInfoStubFactory.Delete(_fileSystem, path);
                }

                RaiseDeleted(_directoryWatcher, deletedDirectories);

                await _taskScheduler;

                _changeset.Should().NotBeNull();
                _changeset.AddedDirectories.Should().Equal(expectedAdded);
                _changeset.RemovedDirectories.Should().Equal(expectedRemoved);
            }
        }
    }
}
