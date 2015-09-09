using Microsoft.Languages.Editor.Classification;
using Microsoft.R.Support.Markdown.Tokens;

namespace Microsoft.R.Support.Markdown.Classification
{
    internal sealed class MdClassificationNameProvider: IClassificationContextNameProvider<MdToken>
    {
        public string GetClassificationContextName(MdToken t)
        {
            switch (t.TokenType)
            {
                case MdTokenType.AltText:
                    return MdClassificationTypes.AltText;

                case MdTokenType.Heading:
                case MdTokenType.DashHeading:
                case MdTokenType.LineHeading:
                    return MdClassificationTypes.Heading;

                case MdTokenType.Blockquote:
                    return MdClassificationTypes.Blockquote;

                case MdTokenType.Bold:
                    return MdClassificationTypes.Bold;

                case MdTokenType.Italic:
                    return MdClassificationTypes.Italic;

                case MdTokenType.BoldItalic:
                    return MdClassificationTypes.BoldItalic;

                case MdTokenType.Code:
                    return MdClassificationTypes.Code;

                case MdTokenType.Monospace:
                    return MdClassificationTypes.Monospace;

                case MdTokenType.ListItem:
                    return MdClassificationTypes.ListItem;

                default:
                    return "Default";
            }
        }
    }
}
