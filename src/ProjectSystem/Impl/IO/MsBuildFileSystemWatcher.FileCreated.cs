using System;
using System.IO;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO
{
	public sealed partial class MsBuildFileSystemWatcher
	{
		private class FileCreated : IFileSystemChange
		{
			private readonly string _rootDirectory;
			private readonly IMsBuildFileSystemFilter _fileSystemFilter;
			private readonly string _fullPath;

			public FileCreated(string rootDirectory, IMsBuildFileSystemFilter fileSystemFilter, string fullPath)
			{
				_rootDirectory = rootDirectory;
				_fileSystemFilter = fileSystemFilter;
				_fullPath = fullPath;
			}

			public void Apply(Changeset changeset)
			{
				if (!_fullPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				var relativePath = PathHelper.MakeRelative(_rootDirectory, _fullPath);
				if (!File.Exists(_fullPath) || !_fileSystemFilter.IsAllowedFile(relativePath, File.GetAttributes(_fullPath)))
				{
					return;
				}

				// If file with the same name was removed, just remove it from the RemovedFiles set
				if (changeset.RemovedFiles.Remove(relativePath))
				{
					return;
				}

				// If file had this name before renaming, remove it from RenamedFiles and add to AddedFiles instead
				string renamedFile;
				if (changeset.RenamedFiles.TryGetValue(relativePath, out renamedFile))
				{
					changeset.RenamedFiles.Remove(relativePath);
					changeset.AddedFiles.Add(renamedFile);
                    return;
				}

				changeset.AddedFiles.Add(relativePath);
			}
		}
	}
}