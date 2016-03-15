// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using static Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.MsBuildFileSystemWatcherEntries.EntryState;
using static Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.MsBuildFileSystemWatcherEntries.EntryType;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public class MsBuildFileSystemWatcherEntries {
        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

        public bool RescanRequired { get; private set; }

        public void AddFile(string relativeFilePath) {
            AddEntry(relativeFilePath, File);
        }

        public void DeleteFile(string relativeFilePath) {
            try {
                Entry entry;
                if (_entries.TryGetValue(relativeFilePath, out entry)) {
                    DeleteEntry(entry);
                }
            } catch (InvalidStateException) {
                RescanRequired = true;
            }
        }

        public void RenameFile(string previousRelativePath, string relativeFilePath) {
            try {
                RenameEntry(previousRelativePath, relativeFilePath, File);
            } catch (InvalidStateException) {
                RescanRequired = true;
            }
        }

        public void AddDirectory(string relativePath) {
            relativePath = PathHelper.EnsureTrailingSlash(relativePath);
            AddEntry(relativePath, Directory);
        }

        public void DeleteDirectory(string relativePath) {
            try {
                relativePath = PathHelper.EnsureTrailingSlash(relativePath);
                foreach (var entry in _entries.Values.Where(v => v.RelativePath.StartsWithIgnoreCase(relativePath)).ToList()) {
                    DeleteEntry(entry);
                }
            } catch (InvalidStateException) {
                RescanRequired = true;
            }
        }

        public ISet<string> RenameDirectory(string previousRelativePath, string relativePath) {
            try {
                previousRelativePath = PathHelper.EnsureTrailingSlash(previousRelativePath);
                relativePath = PathHelper.EnsureTrailingSlash(relativePath);
                var newPaths = new HashSet<string>();
                var entriesToRename = _entries.Values
                    .Where(v => v.State == Unchanged || v.State == Added || v.State == RenamedThenAdded)
                    .Where(v => v.RelativePath.StartsWithIgnoreCase(previousRelativePath)).ToList();
                foreach (var entry in entriesToRename) {
                    var newEntryPath = entry.RelativePath.Replace(previousRelativePath, relativePath, 0, previousRelativePath.Length);
                    RenameEntry(entry.RelativePath, newEntryPath, entry.Type);
                    newPaths.Add(newEntryPath);
                }
                return newPaths;
            } catch (InvalidStateException) {
                RescanRequired = true;
                return null;
            }
        }

        public void MarkAllDeleted() {
            foreach (var entry in _entries.Values) {
                entry.PreviousRelativePath = null;
                switch (entry.State) {
                    case Unchanged:
                    case Renamed:
                    case RenamedThenAdded:
                        entry.State = Deleted;
                        break;
                    case Added:
                        _entries.Remove(entry.RelativePath);
                        break;
                    case Deleted:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            RescanRequired = false;
        }

        private Entry AddEntry(string relativeFilePath, EntryType type) {
            Entry entry;
            if (!_entries.TryGetValue(relativeFilePath, out entry)) {
                entry = new Entry(relativeFilePath, type) {
                    State = Added
                };

                _entries[relativeFilePath] = entry;
                return entry;
            }

            switch (entry.State) {
                case Unchanged:
                case RenamedThenAdded:
                case Added:
                    break;
                case Deleted:
                    entry.State = Unchanged;
                    break;
                case Renamed:
                    entry.State = RenamedThenAdded;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return entry;
        }

        private void DeleteEntry(Entry entry) {
            switch (entry.State) {
                case Unchanged:
                    entry.State = Deleted;
                    return;
                case Added:
                    _entries.Remove(entry.RelativePath);
                    UpdateRenamedEntryOnDelete(entry);
                    return;
                case Deleted:
                    return;
                case RenamedThenAdded:
                    entry.State = Renamed;
                    UpdateRenamedEntryOnDelete(entry);
                    return;
                case Renamed:
                    throw new InvalidStateException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RenameEntry(string previousRelativePath, string relativePath, EntryType type) {
            Entry renamedEntry;
            if (_entries.TryGetValue(previousRelativePath, out renamedEntry)) {
                switch (renamedEntry.State) {
                    case Unchanged:
                        renamedEntry.State = Renamed;
                        var entry = AddEntry(relativePath, type);
                        entry.PreviousRelativePath = previousRelativePath;
                        return;
                    case Added:
                        _entries.Remove(previousRelativePath);
                        if (renamedEntry.PreviousRelativePath == null) {
                            AddEntry(relativePath, type);
                        } else {
                            UpdateRenamingChain(renamedEntry, relativePath, type);
                        }
                        return;
                    case RenamedThenAdded:
                        renamedEntry.State = Renamed;
                        if (renamedEntry.PreviousRelativePath == null) {
                            AddEntry(relativePath, type);
                        } else {
                            UpdateRenamingChain(renamedEntry, relativePath, type);
                            renamedEntry.PreviousRelativePath = null;
                        }
                        return;
                    case Deleted:
                    case Renamed:
                        throw new InvalidStateException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void UpdateRenamingChain(Entry renamedEntry, string relativePath, EntryType type) {
            var previouslyRenamedEntryPath = renamedEntry.PreviousRelativePath;
            Entry previouslyRenamedEntry;
            if (!_entries.TryGetValue(previouslyRenamedEntryPath, out previouslyRenamedEntry)) {
                return;
            }

            if (previouslyRenamedEntryPath == relativePath) {
                switch (previouslyRenamedEntry.State) {
                    case Renamed:
                        previouslyRenamedEntry.State = Unchanged;
                        return;
                    case RenamedThenAdded:
                    case Unchanged:
                    case Added:
                    case Deleted:
                        throw new InvalidStateException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var entry = AddEntry(relativePath, type);
            entry.PreviousRelativePath = previouslyRenamedEntryPath;
        }

        private void UpdateRenamedEntryOnDelete(Entry entry) {
            if (entry.PreviousRelativePath == null) {
                return;
            }

            Entry renamedEntry;
            if (!_entries.TryGetValue(entry.PreviousRelativePath, out renamedEntry)) {
                return;
            }

            switch (renamedEntry.State) {
                case Renamed:
                    renamedEntry.State = Deleted;
                    return;
                case RenamedThenAdded:
                    renamedEntry.State = Added;
                    return;
                case Unchanged:
                case Added:
                case Deleted:
                    throw new InvalidStateException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public MsBuildFileSystemWatcher.Changeset ProduceChangeset() {
            var changeset = new MsBuildFileSystemWatcher.Changeset();
            foreach (var entry in _entries.Values.ToList()) {
                switch (entry.State) {
                    case Added:
                    case RenamedThenAdded:
                        if (entry.PreviousRelativePath != null) {
                            switch (entry.Type) {
                                case File:
                                    changeset.RenamedFiles.Add(entry.PreviousRelativePath, entry.RelativePath);
                                    break;
                                case Directory:
                                    changeset.RenamedDirectories.Add(entry.PreviousRelativePath, entry.RelativePath);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        } else {
                            switch (entry.Type) {
                                case File:
                                    changeset.AddedFiles.Add(entry.RelativePath);
                                    break;
                                case Directory:
                                    changeset.AddedDirectories.Add(entry.RelativePath);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        entry.State = Unchanged;
                        break;
                    case Deleted:
                        switch (entry.Type) {
                            case File:
                                changeset.RemovedFiles.Add(entry.RelativePath);
                                break;
                            case Directory:
                                changeset.RemovedDirectories.Add(entry.RelativePath);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        _entries.Remove(entry.RelativePath);
                        break;
                    case Renamed:
                        _entries.Remove(entry.RelativePath);
                        break;
                    default:
                        entry.State = Unchanged;
                        break;
                }
            }

            return changeset;
        }

        [DebuggerDisplay("{Type} {PreviousRelativePath == null ? RelativePath : PreviousRelativePath + \" -> \" + RelativePath}, {State}")]
        private class Entry {
            public Entry(string relativePath, EntryType type) {
                RelativePath = relativePath;
                Type = type;
            }

            public string RelativePath { get; }
            public EntryType Type { get; }

            public string PreviousRelativePath { get; set; }
            public EntryState State { get; set; } = Unchanged;
        }

        private class InvalidStateException : Exception {
             
        }

        internal enum EntryState {
            Unchanged,
            Added,
            Deleted,
            Renamed,
            // This state marks special case when file/directory was first renamed and then another file/directory with the same name was added
            RenamedThenAdded
        }

        internal enum EntryType {
            File,
            Directory
        }
    }
}