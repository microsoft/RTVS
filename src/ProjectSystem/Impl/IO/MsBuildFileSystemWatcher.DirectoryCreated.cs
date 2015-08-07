using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO
{
	public sealed partial class MsBuildFileSystemWatcher
	{
		private class DirectoryCreated : IFileSystemChange
		{
			private readonly string _rootDirectory;
			private readonly IMsBuildFileSystemFilter _fileSystemFilter;
			private readonly string _directoryFullPath;

			public DirectoryCreated(string rootDirectory, IMsBuildFileSystemFilter fileSystemFilter, string directoryFullPath)
			{
				_rootDirectory = rootDirectory;
				_fileSystemFilter = fileSystemFilter;
				_directoryFullPath = directoryFullPath;
			}

			public void Apply(Changeset changeset)
			{
				if (!_directoryFullPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				Queue<string> directories = new Queue<string>();
				directories.Enqueue(_directoryFullPath);

				while (directories.Count > 0)
				{
					var directoryPath = directories.Dequeue();
					var directory = new DirectoryInfo(directoryPath);
					var relativeDirectoryPath = PathHelper.MakeRelative(_rootDirectory, directoryPath);

					if (!directory.Exists)
					{
						continue;
					}

					// We don't want to add root directory
					if (!string.IsNullOrEmpty(relativeDirectoryPath))
					{
						relativeDirectoryPath = PathHelper.EnsureTrailingSlash(relativeDirectoryPath);

						if (!_fileSystemFilter.IsAllowedDirectory(relativeDirectoryPath, directory.Attributes))
						{
							continue;
						}

						// When we add new directory, we don't remove it from RemovedDirectories, cause the content of the removed directory should be deleted anyway
						changeset.AddedDirectories.Add(relativeDirectoryPath);
					}

					foreach (var entry in directory.EnumerateFileSystemInfos())
					{
						if (entry is DirectoryInfo)
						{
							directories.Enqueue(entry.FullName);
						}
						else
						{
							var relativeFilePath = PathHelper.MakeRelative(_rootDirectory, entry.FullName);
							
							// If file with the same name was removed, just remove it from the RemovedFiles set
							if (_fileSystemFilter.IsAllowedFile(relativeFilePath, entry.Attributes) && !changeset.RemovedFiles.Remove(relativeFilePath))
							{
								changeset.AddedFiles.Add(relativeFilePath);
							}
						}
					}
				}
			}
		}
	}
}