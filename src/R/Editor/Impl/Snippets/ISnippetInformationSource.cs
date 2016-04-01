// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Editor.Snippets {
    /// <summary>
    /// Implemented at the application level and exported via MEF.
    /// In Visual Studio this is typically a expansion client or
    /// the expansion cache.
    /// </summary>
    public interface ISnippetInformationSource {
        /// <summary>
        /// Determines if given completion list item is a snippet
        /// </summary>
        bool IsSnippet(string name);

        IEnumerable<ISnippetInfo> Snippets { get; }
    }
}
