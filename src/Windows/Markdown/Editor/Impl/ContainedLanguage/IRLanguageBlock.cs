// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    internal interface IRLanguageBlock: ITextRange {
        /// <summary>
        /// The block is `inline` block as opposed to ```fenced``` block. 
        /// </summary>
        bool Inline { get; }
    }
}
