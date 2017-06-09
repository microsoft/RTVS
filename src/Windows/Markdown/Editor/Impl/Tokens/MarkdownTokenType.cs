// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Markdown.Editor.Tokens {
    // https://guides.github.com/features/mastering-markdown/

    public enum MarkdownTokenType {
        /// <summary>
        /// Unrecognized token
        /// </summary>
        Unknown,

        /// <summary>
        /// === heading
        /// </summary>
        LineHeading,

        /// <summary>
        /// --- heading
        /// </summary>
        DashHeading,

        /// <summary>
        /// ### heading
        /// </summary>
        Heading,

        /// <summary>
        /// > text
        /// </summary>
        Blockquote,

        /// <summary>
        /// **text** or __text__
        /// </summary>
        Bold,

        /// <summary>
        /// *text* or _text_
        /// </summary>
        Italic,

        /// <summary>
        /// Bold inside italic range or the other way around
        /// </summary>
        BoldItalic,

        /// <summary>
        /// *item[LF] or' Num.' item
        /// </summary>
        ListItem,

        /// <summary>
        /// 'backtick' text 'backtick'
        /// </summary>
        Monospace,

        /// <summary>
        /// ```code```
        /// </summary>
        Code,
        
        /// <summary>
        /// (url)
        /// </summary>
        Link,

        /// <summary>
        /// ![text]
        /// </summary>
        AltText,

        /// <summary>
        /// Preudo-token indicating end of stream
        /// </summary>
        EndOfStream
    }
}
