using System;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO
{
	public sealed partial class MsBuildFileSystemWatcher
	{
		private class DirectoryDeleted : IFileSystemChange
		{
			private readonly string _rootDirectory;
			private readonly string _fullPath;

			public DirectoryDeleted(string rootDirectory, string fullPath)
			{
				_rootDirectory = rootDirectory;
				_fullPath = fullPath;
			}

			public void Apply(Changeset changeset)
			{
				if (!_fullPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				var relativePath = PathHelper.MakeRelative(_rootDirectory, _fullPath);

				// If directory was previously added to AddedDirectories, we need to remove all its content as well
				if (changeset.AddedDirectories.Remove(relativePath))
				{
					changeset.AddedDirectories.RemoveWhere(d => d.StartsWith(relativePath, StringComparison.OrdinalIgnoreCase));
					changeset.AddedFiles.RemoveWhere(f => f.StartsWith(relativePath, StringComparison.OrdinalIgnoreCase));
					return;
				}

				// If directory was renamed into relativePath, put the oldRelativePath into RemovedFiles instead.
				var oldRelativePath = changeset.RenamedFiles.GetFirstKeyByValueIgnoreCase(relativePath);
				if (oldRelativePath != null)
				{
					changeset.RenamedDirectories.Remove(oldRelativePath);
					changeset.RemovedDirectories.Add(oldRelativePath);
					return;
				}

				changeset.RemovedDirectories.Add(relativePath);
			}
		}
	}
}