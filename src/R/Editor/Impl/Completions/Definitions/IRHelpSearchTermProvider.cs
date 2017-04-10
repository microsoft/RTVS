// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// An interface implemented by R completion provider that supplies
    /// list of entries to help search. Help terms are similar to the editor
    /// completion items but exclude things like file names or workspace variables.
    /// Providers are exported via MEF.
    /// </summary>
    public interface IRHelpSearchTermProvider
    {
        /// <summary>
        /// Retrieves list of search terms
        /// </summary>
        /// <returns>List of completion entries</returns>
        IReadOnlyCollection<string> GetEntries();
    }
}
