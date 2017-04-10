// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Snippets {
    /// <summary>
    /// Implemented at the application level and exported via MEF.
    /// In Visual Studio information source is typically an expansion 
    /// client or the expansion cache.
    /// </summary>
    public interface ISnippetInformationSourceProvider {
        /// <summary>
        /// Determines if given completion list item is a snippet
        /// </summary>
        ISnippetInformationSource InformationSource { get; }
    }
}
