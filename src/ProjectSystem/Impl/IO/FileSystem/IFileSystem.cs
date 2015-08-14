using System.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.FileSystem
{
	public interface IFileSystem
	{
		IFileSystemWatcher CreateFileSystemWatcher(string directory, string filter);
		IDirectoryInfo GetDirectoryInfo(string directoryPath);
		bool FileExists(string fullPath);
		FileAttributes GetFileAttributes(string fullPath);
	}
}
