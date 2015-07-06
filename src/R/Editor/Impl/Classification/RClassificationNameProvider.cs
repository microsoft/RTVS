using Microsoft.Languages.Editor.Classification;
using Microsoft.R.Core.Classification;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Language.StandardClassification;

namespace Microsoft.R.Editor.Classification
{
    internal sealed class RClassificationNameProvider: IClassificationContextNameProvider<RToken>
    {
        public string GetClassificationContextName(RToken t)
        {
            switch (t.TokenType)
            {
                case RTokenType.Comment:
                    return PredefinedClassificationTypeNames.Comment;
                case RTokenType.Logical:
                case RTokenType.Missing:
                case RTokenType.Null:
                case RTokenType.Keyword:
                    return PredefinedClassificationTypeNames.Keyword;
                case RTokenType.String:
                    return PredefinedClassificationTypeNames.String;
                case RTokenType.Number:
                    return PredefinedClassificationTypeNames.Number;
                case RTokenType.Operator:
                    return PredefinedClassificationTypeNames.Operator;

                case RTokenType.Comma:
                case RTokenType.Semicolon:
                case RTokenType.OpenCurlyBrace:
                case RTokenType.CloseCurlyBrace:
                case RTokenType.OpenSquareBracket:
                case RTokenType.CloseSquareBracket:
                case RTokenType.OpenDoubleSquareBracket:
                case RTokenType.CloseDoubleSquareBracket:
                case RTokenType.OpenBrace:
                case RTokenType.CloseBrace:
                case RTokenType.Dot:
                case RTokenType.Ellipsis:
                    return RClassificationTypes.Punctuation;

                case RTokenType.Identifier:
                    if (t.SubType == RTokenSubType.BuiltinFunction || t.SubType == RTokenSubType.BuiltinConstant)
                    {
                        return RClassificationTypes.BuiltinFunction;
                    }
                    return "Default";

                default:
                    return "Default";
            }
        }
    }
}
