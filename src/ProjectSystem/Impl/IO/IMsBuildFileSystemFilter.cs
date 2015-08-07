using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO
{
	public interface IMsBuildFileSystemFilter
	{
		bool IsAllowedFile(string relativePath, FileAttributes attributes);
		bool IsAllowedDirectory(string relativePath, FileAttributes attributes);
		void Seal();
	}
}