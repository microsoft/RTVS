// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    public interface IContainedLanguageHandler {
        /// <summary>
        /// Collection of contained language blocks
        /// </summary>
        IReadOnlyTextRangeCollection<ITextRange> LanguageBlocks { get; }

        /// <summary>
        /// Retrieves contained command target for a given location in the buffer.
        /// </summary>
        /// <param name="textView">Document view</param>
        /// <param name="position">Position in the document buffer</param>
        /// <returns>Command target or null if location appears to be primary</returns>
        ICommandTarget GetCommandTargetOfLocation(ITextView textView, int position);

        /// <summary>
        /// Locates contained language block for a given location.
        /// </summary>
        /// <returns>block range or null if no secondary block found</returns>
        ITextRange GetCodeBlockOfLocation(int bufferPosition);
    }
}
