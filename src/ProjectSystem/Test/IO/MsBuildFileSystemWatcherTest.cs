// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Test.StubFactories;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test.IO {
    public partial class MsBuildFileSystemWatcherTest {
        private const string ProjectDirectory = @"Z:\abc\";

        [Test]
        public void Start() {
            var filter = "*";
            var delay = 0;
            var fileSystem = Substitute.For<IFileSystem>();
            var fileSystemFilter = Substitute.For<IMsBuildFileSystemFilter>();

            MsBuildFileSystemWatcher watcher = new MsBuildFileSystemWatcher(ProjectDirectory, filter, delay, delay, fileSystem, fileSystemFilter, log: Substitute.For<IActionLog>());
            fileSystemFilter.Received().Seal();

            var fileSystemWatchers = new List<IFileSystemWatcher>();
            fileSystem.CreateFileSystemWatcher(ProjectDirectory, filter)
                .Returns(ci => {
                    var w = Substitute.For<IFileSystemWatcher>();
                    fileSystemWatchers.Add(w);
                    return w;
                });

            watcher.Start();
            watcher.Dispose();

            foreach (var fileSystemWatcher in fileSystemWatchers) {
                fileSystemWatcher.Received().Dispose();
            }
        }

        [CompositeTest]
        [InlineData(null, "*", 0, 0, true, true, typeof(ArgumentNullException))]
        [InlineData("", "*", 0, 0, true, true, typeof(ArgumentException))]
        [InlineData(" ", "*", 0, 0, true, true, typeof(ArgumentException))]

        [InlineData(@"Z:\abc\", null, 0, 0, true, true, typeof(ArgumentNullException))]
        [InlineData(@"Z:\abc\", "", 0, 0, true, true, typeof(ArgumentException))]
        [InlineData(@"Z:\abc\", " ", 0, 0, true, true, typeof(ArgumentException))]

        [InlineData(@"Z:\abc\", "*", -1, 0, true, true, typeof(ArgumentOutOfRangeException))]
        [InlineData(@"Z:\abc\", "*", 0, -1, true, true, typeof(ArgumentOutOfRangeException))]
        [InlineData(@"Z:\abc\", "*", 0, 0, false, true, typeof(ArgumentNullException))]
        [InlineData(@"Z:\abc\", "*", 0, 0, true, false, typeof(ArgumentNullException))]
        public void Ctor_ThrowArgumentException(string projectFolder, string filter, int delay, int recoveryDelay, bool hasFileSystem, bool hasFileSystemFilter, Type exceptionType) {
            var fileSystem = hasFileSystem ? Substitute.For<IFileSystem>() : null;
            var fileSystemFilter = hasFileSystemFilter ? Substitute.For<IMsBuildFileSystemFilter>() : null;

            Action ctor = () => new MsBuildFileSystemWatcher(projectFolder, filter, delay, recoveryDelay, fileSystem, fileSystemFilter, Substitute.For<IActionLog>());
            ctor.ShouldThrow(exceptionType);
        }

        private static void RaiseCreated(IFileSystemWatcher fileWatcher, IEnumerable<string> fullPaths) {
            foreach (var fullPath in fullPaths) {
                RaiseCreated(fileWatcher, fullPath);
            }
        }

        private static void RaiseDeleted(IFileSystemWatcher fileWatcher, IEnumerable<string> fullPaths) {
            foreach (var fullPath in fullPaths) {
                RaiseDeleted(fileWatcher, fullPath);
            }
        }

        private static void RaiseCreated(IFileSystemWatcher fileWatcher, string fullPath) {
            var directory = Path.GetDirectoryName(fullPath);
            var file = Path.GetFileName(fullPath);
            fileWatcher.Created += Raise.Event<FileSystemEventHandler>(fileWatcher, new FileSystemEventArgs(WatcherChangeTypes.Created, directory, file));
        }

        private static void RaiseDeleted(IFileSystemWatcher fileWatcher, string fullPath) {
            var directory = Path.GetDirectoryName(fullPath);
            var file = Path.GetFileName(fullPath);
            fileWatcher.Deleted += Raise.Event<FileSystemEventHandler>(fileWatcher, new FileSystemEventArgs(WatcherChangeTypes.Created, directory, file));
        }

        private static async Task InjectDirectoriesIntoWatcher(IFileSystemWatcher fileWatcher, IFileSystem fileSystem, IEnumerable<string> fullPaths, ControlledTaskScheduler taskScheduler) {
            var directories = fullPaths.AsList();
            foreach (var path in directories) {
                DirectoryInfoStubFactory.Create(fileSystem, path);
            }
            RaiseCreated(fileWatcher, directories);
            await taskScheduler;
            foreach (var path in directories) {
                DirectoryInfoStubFactory.Delete(fileSystem, path);
            }
        }

        private static async Task InjectFilesIntoWatcher(IFileSystemWatcher fileWatcher, IFileSystem fileSystem, IEnumerable<string> fullPaths, ControlledTaskScheduler taskScheduler) {
            var files = fullPaths.AsList();
            foreach (var path in files) {
                FileInfoStubFactory.Create(fileSystem, path);
            }
            RaiseCreated(fileWatcher, files);
            await taskScheduler;
            foreach (var path in files) {
                FileInfoStubFactory.Delete(fileSystem, path);
            }
        }

        private static Watchers GetWatchersFromMsBuildFileSystemWatcher(IFileSystem fileSystem) {
            var watchers = new Watchers();
            fileSystem.CreateFileSystemWatcher(ProjectDirectory, "*")
                    .Returns(ci => {
                        var watcher = Substitute.For<IFileSystemWatcher>();
                        watcher.NotifyFilter = Arg.Do<NotifyFilters>(nf => {
                            switch (nf) {
                                case NotifyFilters.FileName:
                                    watchers.FileWatcher = watcher;
                                    return;
                                case NotifyFilters.DirectoryName:
                                    watchers.DirectoryWatcher = watcher;
                                    return;
                                case NotifyFilters.Attributes:
                                    watchers.AttributesWatcher = watcher;
                                    return;
                                default:
                                    return;
                            }
                        });
                        return watcher;
                    });
            return watchers;
        }

        private class Watchers {
            public IFileSystemWatcher FileWatcher { get; set; }
            public IFileSystemWatcher DirectoryWatcher { get; set; }
            public IFileSystemWatcher AttributesWatcher { get; set; }
        }
    }
}
