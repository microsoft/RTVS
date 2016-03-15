// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Collections;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test.Assertions {
    internal class MsBuildFileSystemWatcherChangesetAssertions : ReferenceTypeAssertions<MsBuildFileSystemWatcher.Changeset, MsBuildFileSystemWatcherChangesetAssertions> {
        protected override string Context { get; } = "Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.MsBuildFileSystemWatcher.Changeset";
        private bool _haveAddedFiles;
        private bool _haveAddedDirectories;
        private bool _haveRenamedFiles;
        private bool _haveRenamedDirectories;
        private bool _haveRemovedFiles;
        private bool _haveRemovedDirectories;

        public MsBuildFileSystemWatcherChangesetAssertions(MsBuildFileSystemWatcher.Changeset token) {
            Subject = token;
        }

        public void NoOtherChanges(string because = "", params object[] reasonArgs) {
            var names = new List<string>()
                .AddIf(_haveAddedFiles, "added files")
                .AddIf(_haveAddedDirectories, "added directories")
                .AddIf(_haveRenamedFiles, "renamed files")
                .AddIf(_haveRenamedDirectories, "renamed directories")
                .AddIf(_haveRemovedFiles, "removed files")
                .AddIf(_haveRemovedDirectories, "removed directories");

            var assertionScope = Execute.Assertion
                .BecauseOf(because, reasonArgs);
            if (names.Count > 0) {
                assertionScope = assertionScope
                    .WithExpectation("Expected to have only {0}{reason}, ", string.Join(", ", names));
            } else {
                assertionScope = assertionScope
                    .WithExpectation("Expected to have no changes{reason}, ");
            }

            assertionScope
                .Given(() => Subject)
                .ForCondition(c => c.AddedFiles.Any() ^ !_haveAddedFiles)
                .FailWith("but {0} files were added.", c => c.AddedFiles)
                .Then
                .ForCondition(c => c.AddedDirectories.Any() ^ !_haveAddedDirectories)
                .FailWith("but {0} directories were added.", c => c.AddedDirectories)
                .Then
                .ForCondition(c => c.RemovedFiles.Any() ^ !_haveRemovedFiles)
                .FailWith("but {0} files were deleted.", c => c.RemovedFiles)
                .Then
                .ForCondition(c => c.RemovedDirectories.Any() ^ !_haveRemovedDirectories)
                .FailWith("but {0} directories were deleted.", c => c.RemovedDirectories)
                .Then
                .ForCondition(c => c.RenamedFiles.Any() ^ !_haveRenamedFiles)
                .FailWith("but {0} files were renamed.", c => c.RenamedFiles.Keys)
                .Then
                .ForCondition(c => c.RenamedDirectories.Any() ^ !_haveRenamedDirectories)
                .FailWith("but {0} directories were renamed.", c => c.RenamedDirectories.Keys);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveAddedFiles(params string[] expected) {
            return HaveAddedFiles((IEnumerable<string>)expected);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveAddedFiles(IEnumerable<string> expected, string because = "", params object[] reasonArgs) {
            var expectedArray = expected.AsArray();

            Execute.Assertion
                .BecauseOf(because, reasonArgs)
                .WithExpectation("Expected changeset to have {0} added files{reason}, ", (object)expectedArray)
                .Given(() => Subject.AddedFiles.AsEnumerable())
                .AssertCollectionIsNotEmpty(expectedArray.Any(), "but there are no added files.")
                .Then
                .AssertCollectionDoesNotMissItems(expectedArray, "but {0} files weren't added.")
                .Then
                .AssertDictionaryDoesNotHaveAdditionalItems(expectedArray, "but {0} files shoudn't be added.");

            _haveAddedFiles = expectedArray.Length > 0;
            return new AndConstraint<MsBuildFileSystemWatcherChangesetAssertions>(this);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveAddedDirectories(params string[] expected) {
            return HaveAddedDirectories((IEnumerable<string>)expected);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveAddedDirectories(IEnumerable<string> expected, string because = "", params object[] reasonArgs) {
            var expectedArray = expected.AsArray();

            Execute.Assertion
                .BecauseOf(because, reasonArgs)
                .WithExpectation("Expected changeset to have {0} added directories{reason}, ", (object)expectedArray)
                .Given(() => Subject.AddedDirectories.AsEnumerable())
                .AssertCollectionIsNotEmpty(expectedArray.Any(), "but there are no added directories.")
                .Then
                .AssertCollectionDoesNotMissItems(expectedArray, "but {0} directories weren't added.")
                .Then
                .AssertDictionaryDoesNotHaveAdditionalItems(expectedArray, "but {0} directories shoudn't be added.");

            _haveAddedDirectories = expectedArray.Length > 0;
            return new AndConstraint<MsBuildFileSystemWatcherChangesetAssertions>(this);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveRemovedFiles(params string[] expected) {
            return HaveRemovedFiles((IEnumerable<string>)expected);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveRemovedFiles(IEnumerable<string> expected, string because = "", params object[] reasonArgs) {
            var expectedArray = expected.AsArray();

            Execute.Assertion
                .BecauseOf(because, reasonArgs)
                .WithExpectation("Expected changeset to have {0} removed files{reason}, ", (object)expectedArray)
                .Given(() => Subject.RemovedFiles.AsEnumerable())
                .AssertCollectionIsNotEmpty(expectedArray.Any(), "but there are no removed files.")
                .Then
                .AssertCollectionDoesNotMissItems(expectedArray, "but {0} files weren't removed.")
                .Then
                .AssertDictionaryDoesNotHaveAdditionalItems(expectedArray, "but {0} files shoudn't be removed.");

            _haveRemovedFiles = expectedArray.Length > 0;
            return new AndConstraint<MsBuildFileSystemWatcherChangesetAssertions>(this);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveRemovedDirectories(params string[] expected) {
            return HaveRemovedDirectories((IEnumerable<string>)expected);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveRemovedDirectories(IEnumerable<string> expected, string because = "", params object[] reasonArgs) {
            var expectedArray = expected.AsArray();

            Execute.Assertion
                .BecauseOf(because, reasonArgs)
                .WithExpectation("Expected changeset to have {0} removed directories{reason}, ", (object)expectedArray)
                .Given(() => Subject.RemovedDirectories.AsEnumerable())
                .AssertCollectionIsNotEmpty(expectedArray.Any(), "but there are no removed directories.")
                .Then
                .AssertCollectionDoesNotMissItems(expectedArray, "but {0} directories weren't removed.")
                .Then
                .AssertDictionaryDoesNotHaveAdditionalItems(expectedArray, "but {0} directories shoudn't be removed.");

            _haveRemovedDirectories = expectedArray.Length > 0;
            return new AndConstraint<MsBuildFileSystemWatcherChangesetAssertions>(this);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveRenamedFiles(IReadOnlyList<string> from, IReadOnlyList<string> to, string because = "", params object[] reasonArgs) {
            return HaveRenamedFiles(ConvertFromToCollectionsToDictionary(from, to), because, reasonArgs);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveRenamedFiles(IDictionary<string, string> expected, string because = "", params object[] reasonArgs) {
            var expectedString = string.Join(", ", expected.Select(e => $"{e.Key} -> {e.Value}"));

            Execute.Assertion
                .BecauseOf(because, reasonArgs)
                .WithExpectation("Expected changeset to have {0} renamed files{reason}, ", expectedString)
                .Given(() => Subject.RenamedFiles)
                .AssertDictionaryIsNotEmpty(expected.Any(), "but there are no renamed files.")
                .Then
                .AssertDictionaryDoesNotMissKeys(expected, "but {0} files weren't renamed.")
                .Then
                .AssertDictionaryDoesNotHaveAdditionalKeys(expected, "but {0} files shoudn't be renamed.")
                .Then
                .AssertDictionaryHaveSameValues(expected);

            _haveRenamedFiles = expected.Count > 0;
            return new AndConstraint<MsBuildFileSystemWatcherChangesetAssertions>(this);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveRenamedDirectories(IReadOnlyList<string> from, IReadOnlyList<string> to, string because = "", params object[] reasonArgs) {
            return HaveRenamedDirectories(ConvertFromToCollectionsToDictionary(from, to), because, reasonArgs);
        }

        public AndConstraint<MsBuildFileSystemWatcherChangesetAssertions> HaveRenamedDirectories(IDictionary<string, string> expected, string because = "", params object[] reasonArgs) {
            var expectedString = string.Join(", ", expected.Select(e => $"{e.Key} -> {e.Value}"));

            Execute.Assertion
                .BecauseOf(because, reasonArgs)
                .WithExpectation("Expected changeset to have {0} renamed directories{reason}, ", expectedString)
                .Given(() => Subject.RenamedDirectories)
                .AssertDictionaryIsNotEmpty(expected.Any(), "but there are no renamed directories.")
                .Then
                .AssertDictionaryDoesNotMissKeys(expected, "but {0} directories weren't renamed.")
                .Then
                .AssertDictionaryDoesNotHaveAdditionalKeys(expected, "but {0} directories shoudn't be renamed.")
                .Then
                .AssertDictionaryHaveSameValues(expected);

            _haveRenamedDirectories = expected.Count > 0;
            return new AndConstraint<MsBuildFileSystemWatcherChangesetAssertions>(this);
        }

        private static IDictionary<string, string> ConvertFromToCollectionsToDictionary(IReadOnlyList<string> from, IReadOnlyList<string> to) {
            IDictionary<string, string> result = new Dictionary<string, string>();
            for (var i = 0; i < from.Count; i++) {
                result[from[i]] = to[i];
            }
            return result;
        }
    }
}