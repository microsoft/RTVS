// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of files and folder in the current directory
    /// </summary>
    internal sealed class FilesCompletionProvider : IRCompletionListProvider {
        private enum Mode {
            WorkingDirectory,
            UserDirectory,
            Other
        }

        private readonly IFileSystem _fs;
        private readonly IImageService _imageService;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly string _enteredDirectory;
        private readonly bool _forceR; // for tests
        private readonly Task<string> _task;

        private Mode _mode = Mode.Other;
        private volatile string _rootDirectory;

        public FilesCompletionProvider(string directoryCandidate, IServiceContainer services, bool forceR = false) {
            Check.ArgumentNull(nameof(directoryCandidate), directoryCandidate);

            _fs = services.GetService<IFileSystem>();
            _workflow = services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            _imageService = services.GetService<IImageService>();
            _forceR = forceR;

            _enteredDirectory = ExtractDirectory(directoryCandidate);
            _task = GetRootDirectoryAsync(_enteredDirectory);
        }

        private Task<string> GetRootDirectoryAsync(string userProvidedDirectory) {
            if (userProvidedDirectory.Length == 0 || userProvidedDirectory.StartsWithOrdinal(".")) {
                _mode = Mode.WorkingDirectory;
                return Task.Run(async () => _rootDirectory = await _workflow.RSession.GetWorkingDirectoryAsync());
            }
            if (_enteredDirectory.StartsWithOrdinal("~" + Path.DirectorySeparatorChar)) {
                _mode = Mode.UserDirectory;
                return Task.Run(async () => _rootDirectory = await _workflow.RSession.GetRUserDirectoryAsync());
            }
            return Task.FromResult(string.Empty);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = false;

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null) {
            var completions = new List<ICompletionEntry>();
            var directory = _enteredDirectory;

            try {
                // If we are running async directory fetching, wait a bit
                _task?.Wait(500);
            } catch (OperationCanceledException) { }

            try {
                // If directory is set, then the async task did complete
                if (!string.IsNullOrEmpty(_rootDirectory)) {
                    if (_mode == Mode.WorkingDirectory) {
                        directory = Path.Combine(_rootDirectory, _enteredDirectory);
                    } else if (_mode == Mode.UserDirectory) {
                        var subDirectory = _enteredDirectory.Length > 1 ? _enteredDirectory.Substring(2) : _enteredDirectory;
                        directory = Path.Combine(_rootDirectory, subDirectory);
                    }
                }
            } catch (ArgumentException) { }

            try {
                if (!string.IsNullOrEmpty(directory)) {
                    IEnumerable<ICompletionEntry> entries;

                    if (_forceR || _workflow.RSession.IsRemote) {
                        var t = GetRemoteDirectoryItemsAsync(directory);
                        entries = t.WaitTimeout(_forceR ? 5000 : 1000);
                    } else {
                        entries = GetLocalDirectoryItems(directory);
                    }
                    completions.AddRange(entries);
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { } catch (ArgumentException) { } catch (TimeoutException) { }

            return completions;
        }
        #endregion

        private Task<List<ICompletionEntry>> GetRemoteDirectoryItemsAsync(string directory) {
            return Task.Run(async () => {
                var session = _workflow.RSession;
                var completions = new List<ICompletionEntry>();

                try {
                    var rPath = directory.ToRPath().ToRStringLiteral();
                    var files = await session.EvaluateAsync<JArray>(Invariant($"as.list(list.files(path = {rPath}, include.dirs = FALSE))"), REvaluationKind.Normal);
                    var dirs = await session.EvaluateAsync<JArray>(Invariant($"as.list(list.dirs(path = {rPath}, full.names = FALSE, recursive = FALSE))"), REvaluationKind.Normal);

                    var folderGlyph = _imageService.GetImage(ImageType.OpenFolder);
                    completions.AddRange(dirs.Select(d => new EditorCompletionEntry((string) d, (string) d + "/", string.Empty, folderGlyph)));
                    completions.AddRange(files.Select(f => new EditorCompletionEntry((string) f, (string) f, string.Empty, folderGlyph)));

                } catch (RException) { } catch (OperationCanceledException) { }

                return completions;
            });
        }

        private IEnumerable<ICompletionEntry> GetLocalDirectoryItems(string directory) {
            if (_fs.DirectoryExists(directory)) {
                var folderGlyph = _imageService.GetImage(ImageType.ClosedFolder);

                foreach (var dir in _fs.GetDirectories(directory)) {
                    var di = new DirectoryInfo(dir);
                    if (!di.Attributes.HasFlag(FileAttributes.Hidden) && !di.Attributes.HasFlag(FileAttributes.System)) {
                        var dirName = Path.GetFileName(dir);
                        yield return new EditorCompletionEntry(dirName, dirName + Path.DirectorySeparatorChar, string.Empty, folderGlyph);
                    }
                }

                foreach (var file in _fs.GetFiles(directory)) {
                    var di = new FileInfo(file);
                    if (!di.Attributes.HasFlag(FileAttributes.Hidden) && !di.Attributes.HasFlag(FileAttributes.System)) {
                        var fileGlyph = _imageService.GetFileIcon(file);
                        var fileName = Path.GetFileName(file);
                        yield return new EditorCompletionEntry(fileName, fileName, string.Empty, fileGlyph);
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
            return directory.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
