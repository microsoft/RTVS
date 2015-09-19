using System.Collections.Generic;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.FileSystem
{
	public interface IDirectoryInfo : IFileSystemInfo
	{
		IEnumerable<IFileSystemInfo> EnumerateFileSystemInfos();
	}
}