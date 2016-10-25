// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Imaging;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.Language.Intellisense;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list of files and folder in the current directory
    /// </summary>
    internal sealed class FilesCompletionProvider : IRCompletionListProvider {
        private readonly IImagesProvider _imagesProvider;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IGlyphService _glyphService;

        private Task<string> _userDirectoryFetchingTask;
        private string _directory;
        private string _cachedUserDirectory;
        private bool _forceR; // for tests

        public FilesCompletionProvider(string directoryCandidate, IRInteractiveWorkflow workflow, IImagesProvider imagesProvider, IGlyphService glyphService, bool forceR = false) {
            if (directoryCandidate == null) {
                throw new ArgumentNullException(nameof(directoryCandidate));
            }

            _imagesProvider = imagesProvider;
            _workflow = workflow;
            _glyphService = glyphService;
            _forceR = forceR;

            _directory = ExtractDirectory(directoryCandidate);

            if (_directory.StartsWithOrdinal("~\\")) {
                _directory = _directory.Substring(2);
                _userDirectoryFetchingTask = _workflow.RSession.GetRUserDirectoryAsync();
            }
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = false;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            string directory = null;
            string userDirectory = null;

            if (_userDirectoryFetchingTask != null) {
                userDirectory = _userDirectoryFetchingTask.WaitTimeout(500);
                userDirectory = userDirectory ?? _cachedUserDirectory;
            }

            try {
                if (!string.IsNullOrEmpty(userDirectory)) {
                    _cachedUserDirectory = userDirectory;
                    directory = Path.Combine(userDirectory, _directory);
                } else {
                    directory = _directory;
                }

                if (!string.IsNullOrEmpty(directory)) {
                    IEnumerable<RCompletion> entries = null;

                    if (_forceR || _workflow.RSession.IsRemote) {
                        var t = GetRemoteDirectoryItemsAsync(directory);
                        entries = t.WaitTimeout(_forceR ? 5000 : 1000);
                    } else {
                        entries = GetLocalDirectoryItems(directory);
                    }
                    completions.AddRange(entries);
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }

            return completions;
        }
        #endregion

        private Task<List<RCompletion>> GetRemoteDirectoryItemsAsync(string directory) {
            return Task.Run(async () => {
                var session = _workflow.RSession;
                var completions = new List<RCompletion>();

                try {
                    var rPath = directory.ToRPath().ToRStringLiteral();
                    var files = await session.EvaluateAsync<JArray>(Invariant($"as.list(list.files(path = {rPath}, include.dirs = FALSE))"), REvaluationKind.Normal);
                    var dirs = await session.EvaluateAsync<JArray>(Invariant($"as.list(list.dirs(path = {rPath}, full.names = FALSE, recursive = FALSE))"), REvaluationKind.Normal);

                    var folderGlyph = _glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphClosedFolder, StandardGlyphItem.GlyphItemPublic);
                    dirs.ForEach(d => completions.Add(new RCompletion((string)d, (string)d + "/", string.Empty, folderGlyph)));
                    files.ForEach(f => completions.Add(new RCompletion((string)f, (string)f, string.Empty, folderGlyph)));

                } catch (RException) { } catch (OperationCanceledException) { }

                return completions;
            });
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
                var folderGlyph = _glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphClosedFolder, StandardGlyphItem.GlyphItemPublic);

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
