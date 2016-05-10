// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Test.PackageManager {
    internal static class TestRepositories {
        public const string Repo = "Repo1";

        public static string GetRepoPath(TestFilesFixture testFiles) => Path.Combine(testFiles.ReposDestinationPath, Repo);

        public static Task SetLocalRepoAsync(IRSessionEvaluation eval, TestFilesFixture testFiles) => SetLocalRepoAsync(eval, GetRepoPath(testFiles));

        public static Task SetLocalRepoAsync(IRSessionEvaluation eval, string localRepoPath) {
            var code = $"options(repos=list(LOCAL=\"file:///{localRepoPath.ToRPath()}\"))";
            return eval.ExecuteAsync(code);
        }
    }
}
