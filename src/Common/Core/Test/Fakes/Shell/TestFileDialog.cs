// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.UI;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public class TestFileDialog : IFileDialog {
        public string OpenFilePath { get; set; }
        public string BrowseDirectoryPath { get; set; }
        public string SaveFilePath { get; set; }

        public string ShowOpenFileDialog(string filter, string initialPath = null, string title = null) => OpenFilePath;

        public string ShowBrowseDirectoryDialog(string initialPath = null, string title = null) => BrowseDirectoryPath;

        public string ShowSaveFileDialog(string filter, string initialPath = null, string title = null) => SaveFilePath;
    }
}