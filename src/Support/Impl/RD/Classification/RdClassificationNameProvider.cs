using Microsoft.Languages.Editor.Classification;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.Language.StandardClassification;

namespace Microsoft.R.Support.RD.Classification
{
    internal sealed class RdClassificationNameProvider: IClassificationContextNameProvider<RdToken>
    {
        public string GetClassificationContextName(RdToken t)
        {
            switch (t.TokenType)
            {
                case RdTokenType.Comment:
                    return PredefinedClassificationTypeNames.Comment;
                case RdTokenType.Keyword:
                    return PredefinedClassificationTypeNames.Keyword;
                case RdTokenType.String:
                    return PredefinedClassificationTypeNames.String;

                case RdTokenType.Pragma:
                    return PredefinedClassificationTypeNames.PreprocessorKeyword;

                case RdTokenType.OpenBrace:
                case RdTokenType.CloseBrace:
                    return RdClassificationTypes.Braces;

                case RdTokenType.Argument:
                    return RdClassificationTypes.Argument;

                default:
                    return "Default";
            }
        }
    }
}
