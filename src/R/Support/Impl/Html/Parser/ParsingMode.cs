// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Parser {
    /// <summary>
    /// HTML parser mode
    /// </summary>
    public enum ParsingMode {
        /// <summary>
        /// Parse file as HTML: ignore case, treat &lt;style> and &lt;script> block as special
        /// </summary>
        Html,
        /// <summary>
        /// Parse file as HTML: case-sensitive, treat &lt;style> and &lt;script> block as special
        /// </summary>
        Xhtml,
        /// <summary>
        /// Parse file as XML: case sensitive, threat all elements as block elements
        /// </summary>
        Xml
    }
}
