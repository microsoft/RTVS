using System;
using System.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.FileSystem
{
	public interface IFileSystemWatcher : IDisposable
	{
		bool EnableRaisingEvents { get; set; }
		bool IncludeSubdirectories { get; set; }
		int InternalBufferSize { get; set; }
		NotifyFilters NotifyFilter { get; set; }

		event FileSystemEventHandler Changed;
		event FileSystemEventHandler Created;
		event FileSystemEventHandler Deleted;
		event RenamedEventHandler Renamed;
		event ErrorEventHandler Error;
	}
}