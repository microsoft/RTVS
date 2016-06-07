// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    public interface IContainedLanguageHandler {
        /// <summary>
        /// Retrieves contained command target for a given location in the buffer.
        /// </summary>
        /// <param name="position">Position in the document buffer</param>
        /// <returns>Command target or null if location appears to be primary</returns>
        ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition);

        /// <summary>
        /// Locates contained language block for a given location.
        /// </summary>
        /// <returns>block range or null if no secondary block found</returns>
        ITextRange GetCodeBlockOfLocation(int bufferPosition);
    }
}
