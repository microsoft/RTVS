using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.MsBuild;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;
using MsBuildProject = Microsoft.Build.Evaluation.Project;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project
{
	public class FileSystemMirroringProjectLoader
	{
		private bool NotAllItemsAdded { get; set; }
		private readonly static XProjDocument EmptyProject;

		private readonly UnconfiguredProject _unconfiguredProject;
		private readonly IProjectLockService _projectLockService;
		private readonly IThreadHandling _threadHandling;

		static FileSystemMirroringProjectLoader()
		{
			EmptyProject = new XProjDocument(new XProject());
		}

		public FileSystemMirroringProjectLoader(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService, IThreadHandling threadHandling)
		{
			_unconfiguredProject = unconfiguredProject;
			_projectLockService = projectLockService;
			_threadHandling = threadHandling;
		}

        public Task InitializeProjectFromDiskAsync()
        {
			return this.InitializeProjectFromDiskAsync(this._unconfiguredProject.Services.ProjectAsynchronousTasks.UnloadCancellationToken);
		}

		/// <summary>
		/// IInMemoryProject
		/// Creates a new project file if one hasn't already been created and populates it from the contents of the
		/// project folder.
		/// </summary>
		private async Task InitializeProjectFromDiskAsync(CancellationToken cancellationToken)
		{
			try
			{
				// If cancelled, we abort here because it's better to get out of the way and let the real initialization task go ahead.
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				string projectDirectory = Path.GetDirectoryName(_unconfiguredProject.FullPath);
				string projectFilePath = Path.Combine(projectDirectory, FileSystemMirroringProjectFileGenerator.GetInMemoryTargetsFileName(_unconfiguredProject.FullPath));

				var projectItems = GetProjectItems(projectDirectory);
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				// Now create the project file.
				using (ProjectWriteLockReleaser access = await _projectLockService.WriteLockAsync(cancellationToken))
				{
					// A bit odd but we have to "check it out" prior to creating it to avoid some of the validations in chk CPS
					await access.CheckoutAsync(projectFilePath);

					// Now either open or create the in-memory file. Normally Create will happen, but in
					// a scenario where your project had previously failed to load for some reason, need to TryOpen
					// to avoid a new reason for a project load failure
					ProjectRootElement importFile = ProjectRootElement.TryOpen(projectFilePath, access.ProjectCollection);
					if (importFile != null)
					{
						// The file already exists. Scrape it out so we don’t add duplicate items.
						importFile.RemoveAllChildren();
					}
					else
					{
						// The project didn’t already exist, so create it, and then mark the evaluated project dirty
						// so that MSBuild will notice. This step isn’t necessary if the project was already in memory.
						importFile = this.CreateEmptyMsBuildProject(projectFilePath, access.ProjectCollection);

						// Note that we actually need to mark every project evaluation dirty that is already loaded.
						foreach (var configuredProject in _unconfiguredProject.LoadedConfiguredProjects)
						{
							try
							{
								MsBuildProject jsproj = await access.GetProjectAsync(configuredProject, cancellationToken);
								jsproj.MarkDirty();
							}
							catch (Exception ex)
							{
								Debug.Fail("We were unable to mark a configuration as dirty, possibly due to an ObjectDisposedException:" + ex.Message);
							}
						}
					}

					await access.CheckoutAsync(projectFilePath);
					// Now populate the file (passing in null for the schema which we can't get yet)
					PopulateInMemoryFileFromData(projectDirectory, importFile, projectItems);

					// Finally, indicate it is ready to be used. This will change the state of our global property which will
					// cause CPS to re-evaluate and should pick up the file.
					//this.GlobalPropertiesProvider.ProjectFileIsReady = true;

				}

				// Now that the project file is created and stable, we can start the directory tree watcher
				// this.ProjectFileDirectorySync.Initialize();

				if (this.NotAllItemsAdded)
				{
					string message = $"The project {Path.GetFileNameWithoutExtension(this._unconfiguredProject.FullPath)} contains one more files in folders that that are too long.These files will not appear in the project..";
					this._threadHandling.Fork(() =>
					{
						//VsShellUtilities.ShowMessageBox(this.ServiceProvider,
						//message,
						//null,
						//OLEMSGICON.OLEMSGICON_WARNING,
						//OLEMSGBUTTON.OLEMSGBUTTON_OK,
						//OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

						return TplExtensions.CompletedTask;
					}, options: ForkOptions.StartOnMainThread);
				}
			}
			catch (Exception ex)
			{
				//throw new Exception(string.Format("The following error occurred during discovery of project files on disk. {0}.", ex.GetDisplayableMessage()), ex);
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

		private List<FileSystemInfo> GetProjectItems(string folderPath)
		{
			var projectRoot = new DirectoryInfo(folderPath);
			return new List<FileSystemInfo>(projectRoot.GetFileSystemInfos("*", SearchOption.AllDirectories));
		}

		/// <summary>
		/// Walks the list of items we picked up from disk to create msbuild source items for each file and folder. 
		/// </summary>
		private void PopulateInMemoryFileFromData(string projectDirectory, ProjectRootElement rootElement, List<FileSystemInfo> fileData)
		{
			foreach (var item in fileData)
			{
				try
				{
					var isFolder = item is DirectoryInfo;
					var itemType = isFolder ? "Folder" : "Content";
					var path = isFolder
						? PathHelper.EnsureTrailingSlash(PathHelper.MakeRelative(projectDirectory, item.FullName))
						: PathHelper.MakeRelative(projectDirectory, item.FullName);
                    rootElement.AddItem(itemType, path);
				}
				catch (Exception)
				{
					this.NotAllItemsAdded = true;
				}
			}
		}
	}
}
