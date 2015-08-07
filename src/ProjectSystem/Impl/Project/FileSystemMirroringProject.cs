using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using MsBuildProject = Microsoft.Build.Evaluation.Project;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project
{
	public class FileSystemMirroringProject
	{
		private readonly static XProjDocument EmptyProject;

		private readonly UnconfiguredProject _unconfiguredProject;
		private readonly IProjectLockService _projectLockService;
		private readonly MsBuildFileSystemWatcher _fileSystemWatcher;
		private readonly CancellationToken _unloadCancellationToken;
		private readonly string _inMemoryImportFullPath;
		private readonly Dictionary<string, ProjectItemElement> _fileItems;
		private readonly Dictionary<string, ProjectItemElement> _directoryItems;

		private ProjectRootElement _inMemoryImport;

		static FileSystemMirroringProject()
		{
			EmptyProject = new XProjDocument(new XProject());
		}

		public FileSystemMirroringProject(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService, MsBuildFileSystemWatcher fileSystemWatcher)
		{
			_unconfiguredProject = unconfiguredProject;
			_projectLockService = projectLockService;
			_fileSystemWatcher = fileSystemWatcher;
			_unloadCancellationToken = _unconfiguredProject.Services.ProjectAsynchronousTasks.UnloadCancellationToken;
			_inMemoryImportFullPath = _unconfiguredProject.GetInMemoryTargetsFileFullPath();
			_fileItems = new Dictionary<string, ProjectItemElement>(StringComparer.OrdinalIgnoreCase);
			_directoryItems = new Dictionary<string, ProjectItemElement>(StringComparer.OrdinalIgnoreCase);

			var changesHandler = new Func<MsBuildFileSystemWatcher.Changeset, Task>(FileSystemChanged);
			_fileSystemWatcher.SourceBlock.LinkTo(new ActionBlock<MsBuildFileSystemWatcher.Changeset>(changesHandler));
		}

		public async Task CreateInMemoryImport()
		{
			if (_unloadCancellationToken.IsCancellationRequested)
			{
				return;
			}

			using (var access = await _projectLockService.WriteLockAsync(_unloadCancellationToken))
			{
				// A bit odd but we have to "check it out" prior to creating it to avoid some of the validations in chk CPS
				await access.CheckoutAsync(_inMemoryImportFullPath);

				// Now either open or create the in-memory file. Normally Create will happen, but in
				// a scenario where your project had previously failed to load for some reason, need to TryOpen
				// to avoid a new reason for a project load failure
				_inMemoryImport = ProjectRootElement.TryOpen(_inMemoryImportFullPath, access.ProjectCollection);
				if (_inMemoryImport != null)
				{
					// The file already exists. Scrape it out so we don’t add duplicate items.
					_inMemoryImport.RemoveAllChildren();
				}
				else
				{
					// The project didn’t already exist, so create it, and then mark the evaluated project dirty
					// so that MSBuild will notice. This step isn’t necessary if the project was already in memory.
					_inMemoryImport = CreateEmptyMsBuildProject(_inMemoryImportFullPath, access.ProjectCollection);

					// Note that we actually need to mark every project evaluation dirty that is already loaded.
					await ReevaluateLoadedConfiguredProjects(_unloadCancellationToken, access);
				}
			}
		}

		private async Task ReevaluateLoadedConfiguredProjects(CancellationToken cancellationToken, ProjectWriteLockReleaser access)
		{
			foreach (var configuredProject in _unconfiguredProject.LoadedConfiguredProjects)
			{
				try
				{
					MsBuildProject jsproj = await access.GetProjectAsync(configuredProject, cancellationToken);
					jsproj.ReevaluateIfNecessary();
				}
				catch (Exception ex)
				{
					Debug.Fail("We were unable to mark a configuration as dirty" + ex.Message, ex.StackTrace);
				}
			}
		}

		/// <summary>
		/// Helper used to create the empty project file.
		/// Note that we need to set the IsExplicitlyLoaded property on the ProjectRootElement to true to make sure
		/// it is not cleared from the ProjectRootElementCache. Unfortuantely, the constructure which creates a new
		/// empty project does not set this flag. However, the one which can be created from an XmlReader does. So we use
		/// that one to create the project file in memory and then set the path to make sure it is added correctly to the cache.
		/// </summary>
		private ProjectRootElement CreateEmptyMsBuildProject(string projectFilePath, ProjectCollection collection)
		{
			using (XmlReader reader = EmptyProject.CreateReader())
			{
				ProjectRootElement importFile = ProjectRootElement.Create(reader, collection);
				importFile.FullPath = projectFilePath;
				return importFile;
			}
		}

		private async Task FileSystemChanged(MsBuildFileSystemWatcher.Changeset changeset)
		{
			if (_unloadCancellationToken.IsCancellationRequested)
			{
				return;
			}

			using (var access = await _projectLockService.WriteLockAsync(_unloadCancellationToken))
			{
				await access.CheckoutAsync(_inMemoryImportFullPath);

				await RemoveFiles(changeset.RemovedFiles, access);
				await RemoveDirectories(changeset.RemovedDirectories, access);

				await RenameFiles(changeset.RenamedFiles, access);
				await RenameDirectories(changeset.RenamedDirectories, access);

				AddDirectories(changeset.AddedDirectories);
				AddFiles(changeset.AddedFiles);

				foreach (var configuredProject in _unconfiguredProject.LoadedConfiguredProjects)
				{
					try
					{
						MsBuildProject project = await access.GetProjectAsync(configuredProject, _unloadCancellationToken);
						project.ReevaluateIfNecessary();
                    }
					catch (Exception ex)
					{
						Debug.Fail("We were unable to mark a configuration as dirty" + ex.Message, ex.StackTrace);
					}
				}
			}
		}

		private Task RemoveFiles(HashSet<string> filesToRemove, ProjectWriteLockReleaser access)
		{
			return RemoveItems(_fileItems, filesToRemove, access);
		}

		private async Task RemoveDirectories(IReadOnlyCollection<string> directoriesToRemove, ProjectWriteLockReleaser access)
		{
			foreach (var directoryName in directoriesToRemove)
			{
				await RemoveItems(_fileItems, directoryName, access);
				await RemoveItems(_directoryItems, directoryName, access);
			}
		}

		private Task RemoveItems(Dictionary<string, ProjectItemElement> items, string directoryName, ProjectWriteLockReleaser access)
		{
			return RemoveItems(items, items.Keys.Where(f => f.StartsWith(directoryName, StringComparison.OrdinalIgnoreCase)).ToList(), access);
		}

		private async Task RemoveItems(Dictionary<string, ProjectItemElement> items, IReadOnlyCollection<string> itemsToRemove, ProjectWriteLockReleaser access)
		{
			await access.CheckoutAsync(itemsToRemove);
			foreach (var path in itemsToRemove)
			{
				RemoveItem(items, path);
			}
		}

		private void RemoveItem(Dictionary<string, ProjectItemElement> items, string path)
		{
			ProjectItemElement item;
			if (!items.TryGetValue(path, out item))
			{
				return;
			}

			ProjectElementContainer xmlItem = item;
			ProjectElementContainer xmlParent = xmlItem.Parent;
			while (xmlParent != null)
			{
				xmlParent.RemoveChild(xmlItem);
				if (xmlParent.Count > 0)
				{
					break;
				}

				xmlItem = xmlParent;
				xmlParent = xmlItem.Parent;
			}

			items.Remove(path);
		}

		private Task RenameFiles(IReadOnlyDictionary<string, string> filesToRename, ProjectWriteLockReleaser access)
		{
			return RenameItems(_fileItems, filesToRename, access);
		}

		private async Task RenameDirectories(IReadOnlyDictionary<string, string> directoriesToRename, ProjectWriteLockReleaser access)
		{
			foreach (var kvp in directoriesToRename)
			{
				await RenameItems(_fileItems, kvp.Key, kvp.Value, access);
				await RenameItems(_directoryItems, kvp.Key, kvp.Value, access);
			}
		}

		private Task RenameItems(Dictionary<string, ProjectItemElement> items, string oldDirectoryName, string newDirectoryName, ProjectWriteLockReleaser access)
		{
			var itemsToRename = items.Keys
				.Where(f => f.StartsWith(oldDirectoryName, StringComparison.OrdinalIgnoreCase))
				.ToDictionary(f => f, f => newDirectoryName + f.Substring(oldDirectoryName.Length));

			return RenameItems(items, itemsToRename, access);
		}

		private async Task RenameItems(Dictionary<string, ProjectItemElement> items, IReadOnlyDictionary<string, string> itemsToRename, ProjectWriteLockReleaser access)
		{
			await access.CheckoutAsync(itemsToRename.Keys);
			foreach (var kvp in itemsToRename)
			{
				ProjectItemElement item;
				if (items.TryGetValue(kvp.Key, out item))
				{
                    items.Remove(kvp.Key);
					item.Include = kvp.Value;
				    items[kvp.Value] = item;
				}
			}
		}

		private void AddDirectories(IReadOnlyCollection<string> directoriesToAdd)
		{
			foreach (string path in directoriesToAdd)
			{
				ProjectItemElement item = _inMemoryImport.AddItem("Folder", path, null);
				_directoryItems.Add(path, item);
			}
		}

		private void AddFiles(IReadOnlyCollection<string> filesToAdd)
		{
			// await InMemoryProjectSourceItemProviderExtension.CallListeners(this.SourceItemsAddingListeners, contexts, false);

			foreach (string path in filesToAdd)
			{
				ProjectItemElement item = _inMemoryImport.AddItem("Content", path, null);
				_fileItems.Add(path, item);
			}

			// await InMemoryProjectSourceItemProviderExtension.CallListeners(this.SourceItemsAddedListeners, contexts, false);
		}
	}
}
