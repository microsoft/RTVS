// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Projection;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    /// <summary>
    /// Represents object that given primary buffer content and 
    /// the collection of the secondary language blocks generates
    /// content for the embedded language text buffer.
    /// </summary>
    public interface IBufferGenerator {
        string GenerateContent(ITextSnapshot snapshot, IEnumerable<ITextRange> languageBlocks, out ProjectionMapping[] mappings);
    }
}
