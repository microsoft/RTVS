// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Parser.Tokens {
    /// <summary>
    /// Type of HTML parse token
    /// </summary>
    public enum HtmlTokenType {
        /// <summary>
        /// A simple text range type token
        /// </summary>
        Range,
        /// <summary>
        /// Token is a comment
        /// </summary>
        Comment,
        /// <summary>
        /// Token represents HTML element name
        /// </summary>
        ElementName,
        /// <summary>
        /// Token represents attribute name
        /// </summary>
        AttributeName,
        /// <summary>
        /// Token represents attribute value
        /// </summary>
        AttributeValue,
        /// <summary>
        /// Token represents a string
        /// </summary>
        String,
        /// <summary>
        /// Token is an artifact
        /// </summary>
        Artifact,
        /// <summary>
        /// Token represents a HTML entity
        /// </summary>
        Entity,
        /// <summary>
        /// A token that consists of other tokens
        /// </summary>
        Composite
    }
}
