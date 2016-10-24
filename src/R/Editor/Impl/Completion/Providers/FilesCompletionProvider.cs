// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Imaging;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Language.Intellisense;
using static System.FormattableString;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list of files and folder in the current directory
    /// </summary>
    public class FilesCompletionProvider : IRCompletionListProvider {
        private readonly ICoreShell _shell;
        private readonly IImagesProvider _imagesProvider;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        private Task<string> _userDirectoryFetchingTask;
        private string _directory;
        private string _cachedUserDirectory;

        public FilesCompletionProvider(string directoryCandidate, ICoreShell shell) {
            _shell = shell;
            if (directoryCandidate == null) {
                throw new ArgumentNullException(nameof(directoryCandidate));
            }

            _imagesProvider = shell.ExportProvider.GetExportedValueOrDefault<IImagesProvider>();
            _workflowProvider = shell.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            _directory = ExtractDirectory(directoryCandidate);

            if (_directory.StartsWithOrdinal("~\\")) {
                _directory = _directory.Substring(2);
                _userDirectoryFetchingTask = _workflowProvider.GetOrCreate().RSession.GetRUserDirectoryAsync();
            }
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = false;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
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
                }

                if (!string.IsNullOrEmpty(directory)) {
                    IEnumerable<RCompletion> entries = null;

                    if (_workflowProvider.GetOrCreate().RSession.IsRemote) {
                        var t = GetRemoteDirectoryItemsAsync(directory);
                        t.Wait(500);
                        entries = t.IsCompleted ? t.Result : new List<RCompletion>();
                    } else {
                        entries = GetLocalDirectoryItems(directory);
                    }
                    entries.ForEach(e => completions.Add(e));
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }

            return completions;
        }
        #endregion

        private async Task<IEnumerable<RCompletion>> GetRemoteDirectoryItemsAsync(string directory) {
            var session = _workflowProvider.GetOrCreate().RSession;
            var completions = new List<RCompletion>();

            try {
                var rPath = directory.ToRPath();
                var files = await session.EvaluateAsync<IEnumerable<string>>(Invariant($"list.files(path = {rPath})"), REvaluationKind.Normal);
                var dirs = await session.EvaluateAsync<IEnumerable<string>>(Invariant($"list.dirs(path = {rPath}, full.names = FALSE, recursive = FALSE)"), REvaluationKind.Normal);
                var folderGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphClosedFolder, StandardGlyphItem.GlyphItemPublic, _shell);

                dirs.ForEach(d => completions.Add(new RCompletion(d, d + "/", string.Empty, folderGlyph)));
                files.ForEach(f => completions.Add(new RCompletion(f, f, string.Empty, folderGlyph)));

            } catch (RException) { } catch (OperationCanceledException) { }

            return completions;
        }

        private IEnumerable<RCompletion> GetLocalDirectoryItems(string userDirectory) {
            string directory;

            if (!string.IsNullOrEmpty(userDirectory)) {
                _cachedUserDirectory = userDirectory;
                directory = Path.Combine(userDirectory, _directory);
            } else {
                directory = Path.Combine(RToolsSettings.Current.WorkingDirectory, _directory);
            }

            if (Directory.Exists(directory)) {
                var folderGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphClosedFolder, StandardGlyphItem.GlyphItemPublic, _shell);

                foreach (string dir in Directory.GetDirectories(directory)) {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    if (!di.Attributes.HasFlag(FileAttributes.Hidden) && !di.Attributes.HasFlag(FileAttributes.System)) {
                        string dirName = Path.GetFileName(dir);
                        yield return new RCompletion(dirName, dirName + "/", string.Empty, folderGlyph);
                    }
                }

                foreach (string file in Directory.GetFiles(directory)) {
                    FileInfo di = new FileInfo(file);
                    if (!di.Attributes.HasFlag(FileAttributes.Hidden) && !di.Attributes.HasFlag(FileAttributes.System)) {
                        ImageSource fileGlyph = _imagesProvider?.GetFileIcon(file);
                        string fileName = Path.GetFileName(file);
                        yield return new RCompletion(fileName, fileName, string.Empty, fileGlyph);
                    }
                }
            }
        }

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
