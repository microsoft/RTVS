// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification {
    [ExcludeFromCodeCoverage]
    internal sealed class MarkdownClassificationDefinitions {
        // https://help.github.com/articles/markdown-basics/

        /// <summary>
        /// # The largest heading (an <h1> tag)
        /// ## The second largest heading (an <h2> tag)
        /// </summary>
        [Export]
        [Name(MarkdownClassificationTypes.Heading)]
        internal ClassificationTypeDefinition MdHeadingClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.Heading)]
        [Name(MarkdownClassificationTypes.Heading)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdHeadingClassificationFormat : ClassificationFormatDefinition {
            public MdHeadingClassificationFormat() {
                DisplayName = Resources.ColorName_MD_Heading;
            }
        }

        /// <summary>
        /// > Pardon my french
        /// </summary>
        [Export]
        [Name(MarkdownClassificationTypes.Blockquote)]
        internal ClassificationTypeDefinition MdBlockquoteClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.Blockquote)]
        [Name(MarkdownClassificationTypes.Blockquote)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdBlockquoteClassificationFormat : ClassificationFormatDefinition {
            public MdBlockquoteClassificationFormat() {
                ForegroundColor = Colors.DarkGreen;
                DisplayName = Resources.ColorName_MD_Blockquote;
            }
        }

        /// <summary>
        /// **This text will be bold**
        /// </summary>
        [Export]
        [Name(MarkdownClassificationTypes.Bold)]
        internal ClassificationTypeDefinition MdBoldClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.Bold)]
        [Name(MarkdownClassificationTypes.Bold)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdBoldClassificationFormat : ClassificationFormatDefinition {
            public MdBoldClassificationFormat() {
                ForegroundColor = Colors.Magenta;
                IsBold = true;
                DisplayName = Resources.ColorName_MD_Bold;
            }
        }

        /// <summary>
        /// *This text will be italic*
        /// </summary>
        [Export]
        [Name(MarkdownClassificationTypes.Italic)]
        internal ClassificationTypeDefinition MdItalicClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.Italic)]
        [Name(MarkdownClassificationTypes.Italic)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdItalicClassificationFormat : ClassificationFormatDefinition {
            public MdItalicClassificationFormat() {
                ForegroundColor = Colors.Magenta;
                IsItalic = true;
                DisplayName = Resources.ColorName_MD_Italic;
            }
        }

        [Export]
        [Name(MarkdownClassificationTypes.BoldItalic)]
        internal ClassificationTypeDefinition MdBoldItalicClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.BoldItalic)]
        [Name(MarkdownClassificationTypes.BoldItalic)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdBoldItalicClassificationFormat : ClassificationFormatDefinition {
            public MdBoldItalicClassificationFormat() {
                ForegroundColor = Colors.Magenta;
                IsItalic = true;
                IsBold = true;
                DisplayName = Resources.ColorName_MD_BoldItalic;
            }
        }

        /// <summary>
        /// * Item [line break]
        /// - Item [line break]
        /// N. Item [line break]
        /// </summary>
        [Export]
        [Name(MarkdownClassificationTypes.ListItem)]
        internal ClassificationTypeDefinition MdListItemClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.ListItem)]
        [Name(MarkdownClassificationTypes.ListItem)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdListItemClassificationFormat : ClassificationFormatDefinition {
            public MdListItemClassificationFormat() {
                ForegroundColor = Colors.YellowGreen;
                DisplayName = Resources.ColorName_MD_ListItem;
            }
        }

        /// <summary>
        /// `monospace`
        /// </summary>
        [Export]
        [Name(MarkdownClassificationTypes.Monospace)]
        internal ClassificationTypeDefinition MdMonospaceClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.Monospace)]
        [Name(MarkdownClassificationTypes.Monospace)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdMonospaceClassificationFormat : ClassificationFormatDefinition {
            public MdMonospaceClassificationFormat() {
                 DisplayName = Resources.ColorName_MD_Monospace;
            }
        }

        /// <summary>
        /// `monospace`
        /// </summary>
        [Export]
        [Name(MarkdownClassificationTypes.AltText)]
        internal ClassificationTypeDefinition MdAltTextClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.AltText)]
        [Name(MarkdownClassificationTypes.AltText)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdAltTextClassificationFormat : ClassificationFormatDefinition {
            public MdAltTextClassificationFormat() {
                DisplayName = Resources.ColorName_MD_AltText;
            }
        }

        /// <summary>
        /// ```{r} ... ``` block background
        /// </summary>
        [Export]
        [Name(MarkdownClassificationTypes.CodeBackground)]
        internal ClassificationTypeDefinition MdCodeBackgroundClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = MarkdownClassificationTypes.CodeBackground)]
        [Name(MarkdownClassificationTypes.CodeBackground)]
        [ExcludeFromCodeCoverage]
        internal sealed class MdCodeBackgroundClassificationFormat : ClassificationFormatDefinition {
            public MdCodeBackgroundClassificationFormat() {
                BackgroundColor = Colors.WhiteSmoke;
                DisplayName = Resources.ColorName_MD_CodeBackground;
            }
        }
    }
}
