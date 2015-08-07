using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO
{
	public sealed partial class MsBuildFileSystemWatcher
	{
		private class FileRenamed : IFileSystemChange
		{
			private readonly string _rootDirectory;
			private readonly IMsBuildFileSystemFilter _fileSystemFilter;
			private readonly string _oldFullPath;
			private readonly string _fullPath;

			public FileRenamed(string rootDirectory, IMsBuildFileSystemFilter fileSystemFilter, string oldFullPath, string fullPath)
			{
				_rootDirectory = rootDirectory;
				_fileSystemFilter = fileSystemFilter;
				_oldFullPath = oldFullPath;
				_fullPath = fullPath;
			}

			public void Apply(Changeset changeset)
			{
				if (!_fullPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				var newFileInfo = new FileInfo(_fullPath);
				var newRelativePath = PathHelper.MakeRelative(_rootDirectory, _fullPath);
				if (!newFileInfo.Exists || !_fileSystemFilter.IsAllowedFile(newRelativePath, newFileInfo.Attributes))
				{
					return;
				}

				var oldRelativePath = PathHelper.MakeRelative(_rootDirectory, _oldFullPath);
				var isRename = true;

				// If file with the oldRelativePath was previously added, remove it from the AddedFiles and add newRelativePath
				if (changeset.AddedFiles.Remove(oldRelativePath))
				{
					changeset.AddedFiles.Add(newRelativePath);
					isRename = false;
				}

				// if file with the newRelativePath was previously deleted, remove it from the RemovedFiles and add oldRelativePath
				if (changeset.RemovedFiles.Remove(newRelativePath))
				{
					changeset.RemovedFiles.Add(oldRelativePath);
					isRename = false;
				}

			    if (!isRename)
			    {
			        return;
			    }

				// if there is a file that was renamed into oldRelativePath, rename it to newRelativePath instead
				// or remove from RenamedFiles if previouslyRenamedRelativePath equal to newRelativePath
				var previouslyRenamedRelativePath = changeset.RenamedFiles.GetFirstKeyByValueIgnoreCase(oldRelativePath);
				if (string.IsNullOrEmpty(previouslyRenamedRelativePath))
				{
					changeset.RenamedFiles[oldRelativePath] = newRelativePath;
				}
				else if (previouslyRenamedRelativePath.Equals(newRelativePath, StringComparison.OrdinalIgnoreCase))
				{
					changeset.RenamedFiles.Remove(previouslyRenamedRelativePath);
				}
				else
				{
					changeset.RenamedFiles[previouslyRenamedRelativePath] = newRelativePath;
				}
			}
		}

	}
}