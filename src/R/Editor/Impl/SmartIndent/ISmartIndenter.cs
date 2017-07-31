// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.Editor.SmartIndent {
    /// <summary>
    /// Provides methods that compute the desired indentation for a line.
    /// </summary>
    public interface ISmartIndenter {
        /// <summary>
        /// Gets the desired indentation of editor line.
        /// </summary>
        /// <returns>The number of spaces to place at the start of the line, 
        /// or null if there is no desired indentation.</returns>
        int? GetDesiredIndentation(IEditorLine line);
    }
}
