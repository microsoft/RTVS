// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Classification;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Classification {
    [Export(typeof(IClassificationNameProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class RClassificationNameProvider : IClassificationNameProvider<RToken>, IClassificationNameProvider {
        public string GetClassificationName(object o, out ITextRange range) {
            var token = (RToken)o;
            range = token;
            return GetClassificationName(token);
        }

        public string GetClassificationName(RToken t) {
            switch (t.TokenType) {
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
                case RTokenType.Ellipsis:
                    return "Punctuation";

                case RTokenType.Identifier:
                    if (t.SubType == RTokenSubType.BuiltinConstant ||
                        t.SubType == RTokenSubType.BuiltinFunction) {
                        return PredefinedClassificationTypeNames.Keyword;
                    } else if (t.SubType == RTokenSubType.TypeFunction) {
                        return RClassificationTypes.TypeFunction;
                    }
                    break;
            }

            return "Default";
        }
    }
}
