using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project
{
	public static class FileSystemMirroringProjectUtilities
	{
		public static string GetProjectDirectory(this UnconfiguredProject unconfiguredProject)
		{
			return Path.GetDirectoryName(unconfiguredProject.FullPath);
		}

		public static string GetInMemoryTargetsFileFullPath(this UnconfiguredProject unconfiguredProject)
		{
			var projectPath = unconfiguredProject.FullPath;
            return Path.Combine(Path.GetDirectoryName(projectPath), GetInMemoryTargetsFileName(projectPath));
		}

		public static string GetInMemoryTargetsFileName(string cpsProjFileName)
		{
			return Path.GetFileNameWithoutExtension(cpsProjFileName) + ".InMemory.Targets";
		}
	}
}
