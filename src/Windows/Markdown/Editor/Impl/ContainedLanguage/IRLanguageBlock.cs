// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    internal interface IRLanguageBlock: ITextRange {
        /// <summary>
        /// The block is `inline` block as opposed to ```fenced``` block. 
        /// </summary>
        bool Inline { get; }

        /// <summary>
        /// Tells if chunk 'message' option is set
        /// </summary>
        bool Message { get; }

        /// <summary>
        /// Tells if chunk 'warning' option is set
        /// </summary>
        bool Warning { get; }

        /// <summary>
        /// Tells if chunk 'echo' option is set
        /// </summary>
        bool Echo { get; }

        /// <summary>
        /// Complete list of chunk options and values
        /// </summary>
        IReadOnlyDictionary<string, string> Options { get; }
    }
}
