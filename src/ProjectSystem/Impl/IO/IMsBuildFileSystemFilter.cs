using System.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO
{
	public interface IMsBuildFileSystemFilter
	{
		bool IsFileAllowed(string relativePath, FileAttributes attributes);
		bool IsDirectoryAllowed(string relativePath, FileAttributes attributes);
		void Seal();
	}
}