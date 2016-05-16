// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    public interface IBufferGenerator {
        void GenerateContent(ITextBuffer primaryBuffer, IEnumerable<ITextRange> languageBlocks);
    }
}
