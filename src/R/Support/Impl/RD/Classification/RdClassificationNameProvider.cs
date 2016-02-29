// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Classification;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.Language.StandardClassification;

namespace Microsoft.R.Support.RD.Classification {
    internal sealed class RdClassificationNameProvider : IClassificationNameProvider<RdToken> {
        public string GetClassificationName(RdToken t) {
            switch (t.TokenType) {
                case RdTokenType.Comment:
                    return PredefinedClassificationTypeNames.Comment;
                case RdTokenType.Keyword:
                    return PredefinedClassificationTypeNames.Keyword;
                case RdTokenType.String:
                    return PredefinedClassificationTypeNames.String;
                case RdTokenType.Number:
                    return PredefinedClassificationTypeNames.Number;

                case RdTokenType.Pragma:
                    return PredefinedClassificationTypeNames.PreprocessorKeyword;

                case RdTokenType.OpenCurlyBrace:
                case RdTokenType.CloseCurlyBrace:
                case RdTokenType.OpenSquareBracket:
                case RdTokenType.CloseSquareBracket:
                    return RdClassificationTypes.Braces;

                default:
                    return "Default";
            }
        }
    }
}
