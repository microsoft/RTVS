// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.Completions;

namespace Microsoft.R.Editor.Completions
{
    /// <summary>
    /// An interface implemented by R completion provider that supplies
    /// list of entries to intellisense. There may be more than one provider.
    /// Providers may be exported via MEF.
    /// </summary>
    public interface IRCompletionListProvider
    {
        /// <summary>
        /// Retrieves list of intellisense entries
        /// </summary>
        /// <param name="context">Completion context</param>
        /// <returns>List of completion entries</returns>
        IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null);

        /// <summary>
        /// Allows or disallows sorting of the provided entries.
        /// For example, file list provider wants directories first 
        /// and files last similar to regular Windows Explorer display.
        /// </summary>
        bool AllowSorting { get; }
    }
}
