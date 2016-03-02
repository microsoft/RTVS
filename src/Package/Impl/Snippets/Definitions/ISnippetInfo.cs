// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.Snippets.Definitions {
    public interface ISnippetInfo {
        string Title { get; }
        string Description { get; }
        string Key { get; }
        string Path { get; }
        string Shortcut { get; }
        bool ShouldFormat { get; }
    }
}