// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Imaging;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list of files and folder in the current directory
    /// </summary>
    public class FilesCompletionProvider : IRCompletionListProvider {
        private readonly ICoreShell _shell;

        [Import(AllowDefault = true)]
        private IImagesProvider ImagesProvider { get; set; }

        [Import]
        private IRInteractiveWorkflow Workflow { get; set; }

        private Task<string> _userDirectoryFetchingTask;
        private string _directory;
        private string _cachedUserDirectory;

        public FilesCompletionProvider(string directoryCandidate, ICoreShell shell) {
            _shell = shell;
            if(directoryCandidate == null) {
                throw new ArgumentNullException(nameof(directoryCandidate));
            }

            ImagesProvider = _shell.ExportProvider.GetExportedValueOrDefault<IImagesProvider>();
            Workflow = _shell.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _directory = ExtractDirectory(directoryCandidate);

            if (_directory.StartsWithOrdinal("~\\")) {
                _directory = _directory.Substring(2);
                _userDirectoryFetchingTask = Workflow.RSession.GetRUserDirectoryAsync();
            }
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = false;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource folderGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphClosedFolder, StandardGlyphItem.GlyphItemPublic, _shell);
            string directory = null;
            string userDirectory = null;

            if (_userDirectoryFetchingTask != null) {
                _userDirectoryFetchingTask.Wait(500);
                userDirectory = _userDirectoryFetchingTask.IsCompleted ? _userDirectoryFetchingTask.Result : null;
                userDirectory = userDirectory ?? _cachedUserDirectory;
            }

            try {
                if (!string.IsNullOrEmpty(userDirectory)) {
                    _cachedUserDirectory = userDirectory;
                    directory = Path.Combine(userDirectory, _directory);
                } else {
                    directory = Path.Combine(RToolsSettings.Current.WorkingDirectory, _directory);
                }

                if (Directory.Exists(directory)) {
                    foreach (string dir in Directory.GetDirectories(directory)) {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        if (!di.Attributes.HasFlag(FileAttributes.Hidden) && !di.Attributes.HasFlag(FileAttributes.System)) {
                            string dirName = Path.GetFileName(dir);
                            completions.Add(new RCompletion(dirName, dirName + "/", string.Empty, folderGlyph));
                        }
                    }

                    foreach (string file in Directory.GetFiles(directory)) {
                        FileInfo di = new FileInfo(file);
                        if (!di.Attributes.HasFlag(FileAttributes.Hidden) && !di.Attributes.HasFlag(FileAttributes.System)) {
                            ImageSource fileGlyph = ImagesProvider?.GetFileIcon(file);
                            string fileName = Path.GetFileName(file);
                            completions.Add(new RCompletion(fileName, fileName, string.Empty, fileGlyph));
                        }
                    }
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }

            return completions;
        }
        #endregion

        private string ExtractDirectory(string directory) {
            if (directory.Length > 0) {
                if (directory[0] == '\"' || directory[0] == '\'') {
                    directory = directory.Substring(1);
                }
                if (directory[directory.Length - 1] == '\"' || directory[directory.Length - 1] == '\'') {
                    directory = directory.Substring(0, directory.Length - 1);
                }
            }
            return directory.Replace('/', '\\');
        }
    }
}
