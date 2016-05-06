// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser {
    /// <summary>
    /// A token that represents attribute value.
    /// </summary>
    public interface IHtmlAttributeValueToken : IHtmlToken {
        /// <summary>
        /// True if attribute value is client script
        /// </summary>
        bool IsScript { get; }

        /// <summary>
        /// Type of attribute value opening quote
        /// </summary>
        char OpenQuote { get; }

        /// <summary>
        /// Type of attribute value closing quote
        /// </summary>
        char CloseQuote { get; }

        /// <summary>
        /// Collection of tokens that make the attribute value
        /// </summary>
        ReadOnlyTextRangeCollection<IHtmlToken> Tokens { get; }
    }
}
