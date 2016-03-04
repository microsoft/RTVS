// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Classification;
using Microsoft.Markdown.Editor.Tokens;

namespace Microsoft.Markdown.Editor.Classification {
    internal sealed class MarkdownClassificationNameProvider : IClassificationNameProvider<MarkdownToken> {
        public string GetClassificationName(MarkdownToken t) {
            switch (t.TokenType) {
                case MarkdownTokenType.AltText:
                    return MarkdownClassificationTypes.AltText;

                case MarkdownTokenType.Heading:
                case MarkdownTokenType.DashHeading:
                case MarkdownTokenType.LineHeading:
                    return MarkdownClassificationTypes.Heading;

                case MarkdownTokenType.Blockquote:
                    return MarkdownClassificationTypes.Blockquote;

                case MarkdownTokenType.Bold:
                    return MarkdownClassificationTypes.Bold;

                case MarkdownTokenType.Italic:
                    return MarkdownClassificationTypes.Italic;

                case MarkdownTokenType.BoldItalic:
                    return MarkdownClassificationTypes.BoldItalic;

                case MarkdownTokenType.CodeStart:
                case MarkdownTokenType.CodeContent:
                case MarkdownTokenType.CodeEnd:
                    return MarkdownClassificationTypes.Code;

                case MarkdownTokenType.Monospace:
                    return MarkdownClassificationTypes.Monospace;

                case MarkdownTokenType.ListItem:
                    return MarkdownClassificationTypes.ListItem;

                default:
                    return "Default";
            }
        }
    }
}
