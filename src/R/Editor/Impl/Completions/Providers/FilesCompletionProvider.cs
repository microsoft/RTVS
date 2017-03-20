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
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of files and folder in the current directory
    /// </summary>
    internal sealed class FilesCompletionProvider : IRCompletionListProvider {
        enum Mode {
            WorkingDirectory,
            UserDirectory,
            Other
        }

        private readonly IImagesProvider _imagesProvider;
        private readonly ICoreShell _coreShell;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IGlyphService _glyphService;

        private readonly string _enteredDirectory;
        private readonly bool _forceR; // for tests

        private readonly Task<string> _task;
        private Mode _mode = Mode.Other;
        private volatile string _rootDirectory;

        public FilesCompletionProvider(string directoryCandidate, ICoreShell coreShell, bool forceR = false) {
            if (directoryCandidate == null) {
                throw new ArgumentNullException(nameof(directoryCandidate));
            }

            _coreShell = coreShell;
            _imagesProvider = _coreShell.GetService<IImagesProvider>();
            _workflow = _coreShell.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            _glyphService = _coreShell.GetService<IGlyphService>();
            _forceR = forceR;

            _enteredDirectory = ExtractDirectory(directoryCandidate);
            _task = GetRootDirectoryAsync(_enteredDirectory);
        }

        private Task<string> GetRootDirectoryAsync(string userProvidedDirectory) {
            if (userProvidedDirectory.Length == 0 || userProvidedDirectory.StartsWithOrdinal(".")) {
                _mode = Mode.WorkingDirectory;
                return Task.Run(async () => _rootDirectory = await _workflow.RSession.GetWorkingDirectoryAsync());
            } else if (_enteredDirectory.StartsWithOrdinal("~\\")) {
                _mode = Mode.UserDirectory;
                return Task.Run(async () => _rootDirectory = await _workflow.RSession.GetRUserDirectoryAsync());
            }
            return Task.FromResult(string.Empty);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = false;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            var completions = new List<RCompletion>();
            string directory = _enteredDirectory;

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
                    IEnumerable<RCompletion> entries = null;

                    if (_forceR || _workflow.RSession.IsRemote) {
                        var t = GetRemoteDirectoryItemsAsync(directory);
                        entries = t.WaitTimeout(_forceR ? 5000 : 1000);
                    } else {
                        entries = GetLocalDirectoryItems(directory);
                    }
                    completions.AddRange(entries);
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { } catch (ArgumentException) { }

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
                    foreach (var d in dirs) {
                        completions.Add(new RCompletion((string)d, (string)d + "/", string.Empty, folderGlyph));
                    }
                    foreach (var f in files) {
                        completions.Add(new RCompletion((string)f, (string)f, string.Empty, folderGlyph));
                    }

                } catch (RException) { } catch (OperationCanceledException) { }

                return completions;
            });
        }

        private IEnumerable<RCompletion> GetLocalDirectoryItems(string directory) {
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
