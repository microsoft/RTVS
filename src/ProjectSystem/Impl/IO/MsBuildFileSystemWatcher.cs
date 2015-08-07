using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO
{
	public sealed partial class MsBuildFileSystemWatcher : IDisposable
	{
		private readonly string _directory;
		private readonly string _filter;
		private readonly ConcurrentQueue<IFileSystemChange> _queue;
		private readonly int _delayMilliseconds;
		private readonly IMsBuildFileSystemFilter _fileSystemFilter;
		private readonly BroadcastBlock<Changeset> _broadcastBlock;
		private FileSystemWatcher _fileWatcher;
		private FileSystemWatcher _directoryWatcher;
		private FileSystemWatcher _attributesWatcher;
		private int _consumerIsWorking;

		public IReceivableSourceBlock<Changeset> SourceBlock { get; }

		public MsBuildFileSystemWatcher(string directory, string filter, int delayMilliseconds, IMsBuildFileSystemFilter fileSystemFilter)
		{
			_directory = directory;
			_filter = filter;
			_delayMilliseconds = delayMilliseconds;
			_fileSystemFilter = fileSystemFilter;

			_queue = new ConcurrentQueue<IFileSystemChange>();
			_broadcastBlock = new BroadcastBlock<Changeset>(b => b);
			SourceBlock = _broadcastBlock.SafePublicize();
			_fileSystemFilter.Seal();
		}

		public void Dispose()
		{
			_fileWatcher?.Dispose();
			_directoryWatcher?.Dispose();
			_attributesWatcher?.Dispose();
        }

		public void Start()
		{
			Enqueue(new DirectoryCreated(_directory, _fileSystemFilter, _directory));

			_fileWatcher = CreateFileSystemWatcher(NotifyFilters.FileName);
			_fileWatcher.Created += (sender, e) => Enqueue(new FileCreated(_directory, _fileSystemFilter, e.FullPath));
			_fileWatcher.Deleted += (sender, e) => Enqueue(new FileDeleted(_directory, e.FullPath));
			_fileWatcher.Renamed += (sender, e) => Enqueue(new FileRenamed(_directory, _fileSystemFilter, e.OldFullPath, e.FullPath));

			_directoryWatcher = CreateFileSystemWatcher(NotifyFilters.DirectoryName);
			_directoryWatcher.Created += (sender, e) => Enqueue(new DirectoryCreated(_directory, _fileSystemFilter, e.FullPath));
			_directoryWatcher.Deleted += (sender, e) => Enqueue(new DirectoryDeleted(_directory, e.FullPath));
			_directoryWatcher.Renamed += (sender, e) => Enqueue(new DirectoryRenamed(_directory, _fileSystemFilter, e.OldFullPath, e.FullPath));

			_attributesWatcher = CreateFileSystemWatcher(NotifyFilters.Attributes);
			_attributesWatcher.Changed += (sender, e) => Enqueue(new AttributesChanged(e.Name, e.FullPath));
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
				Task.Run(async () => await ConsumeWaitPublish());
			}
		}

		private async Task ConsumeWaitPublish()
		{
			var changeset = new Changeset();
			while (!_queue.IsEmpty)
			{
				Consume(changeset);
				await Task.Delay(_delayMilliseconds);
			}

			_broadcastBlock.Post(changeset);
			_consumerIsWorking = 0;
			if (!_queue.IsEmpty)
			{
				StartConsumer();
			}
		}

		private void Consume(Changeset changeset)
		{
            IFileSystemChange change;
			while (_queue.TryDequeue(out change))
			{
				change.Apply(changeset);
			}
		}

		private FileSystemWatcher CreateFileSystemWatcher(NotifyFilters notifyFilter)
		{
			return new FileSystemWatcher(_directory, _filter)
			{
				EnableRaisingEvents = true,
				IncludeSubdirectories = true,
				InternalBufferSize = 65536,
				NotifyFilter = notifyFilter
			};
		}

		private interface IFileSystemChange
		{
			void Apply(Changeset changeset);
		}

		public class Changeset
		{
			public HashSet<string> AddedFiles { get; } = new HashSet<string>(StringComparer.Ordinal);
			public HashSet<string> AddedDirectories { get; } = new HashSet<string>(StringComparer.Ordinal);
			public HashSet<string> RemovedFiles { get; } = new HashSet<string>(StringComparer.Ordinal);
			public HashSet<string> RemovedDirectories { get; } = new HashSet<string>(StringComparer.Ordinal);
			public Dictionary<string, string> RenamedFiles { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
			public Dictionary<string, string> RenamedDirectories { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
		}
	
	}
}
