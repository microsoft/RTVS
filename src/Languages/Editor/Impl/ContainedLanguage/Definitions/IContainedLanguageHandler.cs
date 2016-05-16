// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    public interface IContainedLanguageHandler {
        /// <summary>
        /// Retrieves content type of a given location in a text buffer
        /// </summary>
        /// <param name="position">Position in the document buffer</param>
        /// <returns>Location content type</returns>
        IContentType GetContentTypeOfLocation(int bufferPosition);

        /// <summary>
        /// Retrieves command target handling command for a given location in the buffer.
        /// This may be contained (secondary) language target.
        /// </summary>
        /// <param name="position">Position in the document buffer</param>
        /// <returns>Command target</returns>
        ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition);
    }
}
