using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification
{
    [ExcludeFromCodeCoverage]
    internal sealed class RdClassificationDefinitions
    {
        // https://help.github.com/articles/markdown-basics/

        /// <summary>
        /// # The largest heading (an <h1> tag)
        /// ## The second largest heading (an <h2> tag)
        /// </summary>
        [Export]
        [Name("Markdown Heading")]
        internal ClassificationTypeDefinition MdHeadingClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown Heading")]
        [Name("Markdown Heading")]
        internal sealed class MdHeadingClassificationFormat : ClassificationFormatDefinition
        {
            public MdHeadingClassificationFormat()
            {
                ForegroundColor = Colors.Blue;
                this.DisplayName = Resources.ColorName_MD_Heading;
            }
        }

        /// <summary>
        /// > Pardon my french
        /// </summary>
        [Export]
        [Name("Markdown Blockquote")]
        internal ClassificationTypeDefinition MdBlockquoteClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown Blockquote")]
        [Name("Markdown Blockquote")]
        internal sealed class MdBlockquoteClassificationFormat : ClassificationFormatDefinition
        {
            public MdBlockquoteClassificationFormat()
            {
                ForegroundColor = Colors.DarkGreen;
                this.DisplayName = Resources.ColorName_MD_Blockquote;
            }
        }

        /// <summary>
        /// **This text will be bold**
        /// </summary>
        [Export]
        [Name("Markdown Bold Text")]
        internal ClassificationTypeDefinition MdBoldClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown Bold Text")]
        [Name("Markdown Bold Text")]
        internal sealed class MdBoldClassificationFormat : ClassificationFormatDefinition
        {
            public MdBoldClassificationFormat()
            {
                ForegroundColor = Colors.Navy;
                this.IsBold = true;
                this.DisplayName = Resources.ColorName_MD_Bold;
            }
        }

        /// <summary>
        /// *This text will be italic*
        /// </summary>
        [Export]
        [Name("Markdown Italic Text")]
        internal ClassificationTypeDefinition MdItalicClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown Italic Text")]
        [Name("Markdown Italic Text")]
        internal sealed class MdItalicClassificationFormat : ClassificationFormatDefinition
        {
            public MdItalicClassificationFormat()
            {
                ForegroundColor = Colors.Navy;
                this.IsItalic = true;
                this.DisplayName = Resources.ColorName_MD_Italic;
            }
        }

        [Export]
        [Name("Markdown Bold Italic Text")]
        internal ClassificationTypeDefinition MdBoldItalicClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown Bold Italic Text")]
        [Name("Markdown Bold Italic Text")]
        internal sealed class MdBoldItalicClassificationFormat : ClassificationFormatDefinition
        {
            public MdBoldItalicClassificationFormat()
            {
                ForegroundColor = Colors.Navy;
                this.IsItalic = true;
                this.IsBold = true;
                this.DisplayName = Resources.ColorName_MD_BoldItalic;
            }
        }

        /// <summary>
        /// * Item [line break]
        /// - Item [line break]
        /// N. Item [line break]
        /// </summary>
        [Export]
        [Name("Markdown List Item")]
        internal ClassificationTypeDefinition MdListItemClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown List Item")]
        [Name("Markdown List Item")]
        internal sealed class MdListItemClassificationFormat : ClassificationFormatDefinition
        {
            public MdListItemClassificationFormat()
            {
                ForegroundColor = Colors.Maroon;
                this.DisplayName = Resources.ColorName_MD_ListItem;
            }
        }

        /// <summary>
        /// `monospace`
        /// </summary>
        [Export]
        [Name("Markdown Monospace")]
        internal ClassificationTypeDefinition MdMonospaceClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown Monospace")]
        [Name("Markdown Monospace")]
        internal sealed class MdMonospaceClassificationFormat : ClassificationFormatDefinition
        {
            public MdMonospaceClassificationFormat()
            {
                ForegroundColor = Color.FromArgb(0xFF, 0x60, 0x60, 0x60);
                this.DisplayName = Resources.ColorName_MD_Monospace;
            }
        }

        /// <summary>
        /// `monospace`
        /// </summary>
        [Export]
        [Name("Markdown Code")]
        internal ClassificationTypeDefinition MdCodeClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown Code")]
        [Name("Markdown Code")]
        internal sealed class MdCodeClassificationFormat : ClassificationFormatDefinition
        {
            public MdCodeClassificationFormat()
            {
                ForegroundColor = Colors.DarkGray;
                this.DisplayName = Resources.ColorName_MD_Code;
            }
        }

        /// <summary>
        /// `monospace`
        /// </summary>
        [Export]
        [Name("Markdown Alt Text")]
        internal ClassificationTypeDefinition MdAltTextClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = "Markdown Alt Text")]
        [Name("Markdown Alt Text")]
        internal sealed class MdAltTextClassificationFormat : ClassificationFormatDefinition
        {
            public MdAltTextClassificationFormat()
            {
                ForegroundColor = Colors.DarkMagenta;
                this.DisplayName = Resources.ColorName_MD_AltText;
            }
        }
    }
}
