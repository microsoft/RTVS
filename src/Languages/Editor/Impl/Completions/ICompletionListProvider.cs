// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Languages.Editor.Completions {
    /// <summary>
    /// An interface implemented by a completion provider that supplies
    /// list of entries to intellisense.
    /// </summary>
    public interface ICompletionListProvider {
        /// <summary>
        /// Retrieves list of intellisense entries
        /// </summary>
        /// <param name="context">Completion context</param>
        /// <returns>List of completion entries</returns>
        IReadOnlyCollection<ICompletionEntry> GetEntries(ICompletionContext context);

        /// <summary>
        /// Allows or disallows sorting of the provided entries.
        /// For example, file list provider wants directories first 
        /// and files last similar to regular Windows Explorer display.
        /// </summary>
        bool AllowSorting { get; }
    }
}
