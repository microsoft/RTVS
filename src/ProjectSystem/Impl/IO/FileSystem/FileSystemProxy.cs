using System.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.FileSystem
{
	public class FileSystemProxy : IFileSystem
	{
		public IFileSystemWatcher CreateFileSystemWatcher(string path, string filter)
		{
			return new FileSystemWatcherProxy(path, filter);
		}

		public IDirectoryInfo GetDirectoryInfo(string directoryPath)
		{
			return new DirectoryInfoProxy(directoryPath);
		}

		public bool FileExists(string path)
		{
			return File.Exists(path);
		}

		public FileAttributes GetFileAttributes(string path)
		{
			return File.GetAttributes(path);
		}
	}
}