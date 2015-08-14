using System.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.FileSystem
{
	public interface IFileSystemInfo
	{
		bool Exists { get; }
		string FullName { get; }
		FileAttributes Attributes { get; }
	}
}