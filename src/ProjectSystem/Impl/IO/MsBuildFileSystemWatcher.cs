using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Common.Core.IO;
using Microsoft.R.Actions.Logging;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Logging;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO
{
    public sealed partial class MsBuildFileSystemWatcher : IDisposable
    {
        private readonly string _directory;
        private readonly string _filter;
        private readonly ConcurrentQueue<IFileSystemChange> _queue;
        private readonly int _delayMilliseconds;
        private readonly IFileSystem _fileSystem;
        private readonly IMsBuildFileSystemFilter _fileSystemFilter;
        private readonly TaskScheduler _taskScheduler;
        private readonly BroadcastBlock<Changeset> _broadcastBlock;
        private readonly IActionLog _log;
        private IFileSystemWatcher _fileWatcher;
        private IFileSystemWatcher _directoryWatcher;
        private IFileSystemWatcher _attributesWatcher;
        private int _consumerIsWorking;

        public IReceivableSourceBlock<Changeset> SourceBlock { get; }

        public MsBuildFileSystemWatcher(string directory, string filter, int delayMilliseconds, IFileSystem fileSystem, IMsBuildFileSystemFilter fileSystemFilter, TaskScheduler taskScheduler = null, IActionLog log = null) 
        {
            Requires.NotNullOrWhiteSpace(directory, nameof(directory));
            Requires.NotNullOrWhiteSpace(filter, nameof(filter));
            Requires.Range(delayMilliseconds >= 0, nameof(delayMilliseconds));
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNull(fileSystemFilter, nameof(fileSystemFilter));

            _directory = directory;
            _filter = filter;
            _delayMilliseconds = delayMilliseconds;
            _fileSystem = fileSystem;
            _fileSystemFilter = fileSystemFilter;
            _taskScheduler = taskScheduler ?? TaskScheduler.Default;
            _log = log ?? ProjectSystemActionLog.Default;

            _queue = new ConcurrentQueue<IFileSystemChange>();
            _broadcastBlock = new BroadcastBlock<Changeset>(b => b, new DataflowBlockOptions { TaskScheduler = _taskScheduler });
            SourceBlock = _broadcastBlock.SafePublicize();
            _fileSystemFilter.Seal();
        }

        public void Dispose()
        {
            _fileWatcher?.Dispose();
            _directoryWatcher?.Dispose();
            _attributesWatcher?.Dispose();
        }

        public void Start() {
            _log.WatcherStarting();

            Enqueue(new DirectoryCreated(_directory, _fileSystem, _fileSystemFilter, _directory));

            _fileWatcher = CreateFileSystemWatcher(NotifyFilters.FileName);
            _fileWatcher.Created += (sender, e) => Enqueue(new FileCreated(_directory, _fileSystem, _fileSystemFilter, e.FullPath));
            _fileWatcher.Deleted += (sender, e) => Enqueue(new FileDeleted(_directory, e.FullPath));
            _fileWatcher.Renamed += (sender, e) => Enqueue(new FileRenamed(_directory, _fileSystem, _fileSystemFilter, e.OldFullPath, e.FullPath));
            _fileWatcher.Error += (sender, e) => TraceError("File Watcher", e);

            _directoryWatcher = CreateFileSystemWatcher(NotifyFilters.DirectoryName);
            _directoryWatcher.Created += (sender, e) => Enqueue(new DirectoryCreated(_directory, _fileSystem, _fileSystemFilter, e.FullPath));
            _directoryWatcher.Deleted += (sender, e) => Enqueue(new DirectoryDeleted(_directory, e.FullPath));
            _directoryWatcher.Renamed += (sender, e) => Enqueue(new DirectoryRenamed(_directory, _fileSystem, _fileSystemFilter, e.OldFullPath, e.FullPath));
            _directoryWatcher.Error += (sender, e) => TraceError("Directory Watcher", e);

            _attributesWatcher = CreateFileSystemWatcher(NotifyFilters.Attributes);
            _attributesWatcher.Changed += (sender, e) => Enqueue(new AttributesChanged(e.Name, e.FullPath));
            _attributesWatcher.Error += (sender, e) => TraceError("Attributes Watcher", e);

            _log.WatcherStarted();
        }

        private void Enqueue(IFileSystemChange change)
        {
            _queue.Enqueue(change);
            StartConsumer();
        }

        private void StartConsumer()
        {
            if (Interlocked.Exchange(ref _consumerIsWorking, 1) == 0)
            {
                Task.Factory
                    .StartNew(async () => await ConsumeWaitPublish(), CancellationToken.None, Task.Factory.CreationOptions, _taskScheduler)
                    .Unwrap();
                _log.WatcherConsumeChangesScheduled();
            }
        }

        private async Task ConsumeWaitPublish()
        {
            _log.WatcherConsumeChangesStarted();

            try {
                var changeset = new Changeset();
                while (!_queue.IsEmpty) {
                    Consume(changeset);
                    await Task.Delay(_delayMilliseconds);
                }

                if (!changeset.IsEmpty()) {
                    _broadcastBlock.Post(changeset);
                    _log.WatcherChangesetSent(changeset);
                }
            } finally {
                _consumerIsWorking = 0;
                _log.WatcherConsumeChangesFinished();
                if (!_queue.IsEmpty) {
                    StartConsumer();
                }
            }
        }

        private void Consume(Changeset changeset)
        {
            IFileSystemChange change;
            while (_queue.TryDequeue(out change)) {
                try {
                    _log.WatcherHandleChange(change.ToString());
                    change.Apply(changeset);
                } catch (Exception e) {
                    Trace.Fail($"Failed to apply change {change}:\n{e}");
                    throw;
                }
            }
        }

        private IFileSystemWatcher CreateFileSystemWatcher(NotifyFilters notifyFilter)
        {
            var watcher = _fileSystem.CreateFileSystemWatcher(_directory, _filter);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.InternalBufferSize = 65536;
            watcher.NotifyFilter = notifyFilter;
            return watcher;
        }

        private static bool IsFileAllowed(string rootDirectory, string fullPath, IFileSystem fileSystem, IMsBuildFileSystemFilter filter, out string relativePath)
        {
            if (!fullPath.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = null;
                return false;
            }

            relativePath = PathHelper.MakeRelative(rootDirectory, fullPath);
            return fileSystem.FileExists(fullPath) && filter.IsFileAllowed(relativePath, fileSystem.GetFileAttributes(fullPath));
        }

        private interface IFileSystemChange
        {
            void Apply(Changeset changeset);
        }

        private void TraceError(string watcherName, ErrorEventArgs errorEventArgs) {
            Trace.Fail($"Error in {watcherName}:\n{errorEventArgs.GetException()}");
        }

        public class Changeset
        {
            public HashSet<string> AddedFiles { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> AddedDirectories { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> RemovedFiles { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> RemovedDirectories { get; } = new HashSet<string>(StringComparer.Ordinal);
            public Dictionary<string, string> RenamedFiles { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
            public Dictionary<string, string> RenamedDirectories { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

            public bool IsEmpty()
            {
                return AddedFiles.Count == 0
                    && AddedDirectories.Count == 0
                    && RemovedFiles.Count == 0
                    && RemovedDirectories.Count == 0
                    && RenamedFiles.Count == 0
                    && RenamedDirectories.Count == 0;
            }
        }

    }
}
