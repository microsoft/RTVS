// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Margin {
    internal static class HtmlFormatter {
        private static readonly Dictionary<JTokenType, Func<JToken, string>> _formatters =
            new Dictionary<JTokenType, Func<JToken, string>> {
                {JTokenType.Array, ConvertArray},
            };

        public static string Format(REvaluationResult evalResult) {
            if (!string.IsNullOrEmpty(evalResult.Error)) {
                return FormatError(evalResult);
            }

            if (_formatters.TryGetValue(evalResult.Result.Type, out Func<JToken, string> formatter)) {
                return formatter(evalResult.Result);
            }

            return FormatDefault(evalResult.Result);
        }

        private static string FormatDefault(JToken token) {
            switch (token.Type) {
                case JTokenType.Boolean:
                    return (bool)token ? "TRUE" : "FALSE";
                case JTokenType.Float:
                    return ((float)token).ToString(CultureInfo.CurrentUICulture);
                case JTokenType.Integer:
                    return ((int)token).ToString(CultureInfo.CurrentUICulture);
                case JTokenType.Date:
                    return ((DateTime)token).ToString(CultureInfo.CurrentUICulture);
                case JTokenType.TimeSpan:
                    return ((TimeSpan)token).ToString();
            }
            return token.ToObject<string>();
        }

        private static string FormatError(REvaluationResult result) 
            => Invariant($"<div style='color:red'>{result.Error}</div>");

        private static string ConvertArray(JToken token) {
            Check.Argument(nameof(token), () => token.Type == JTokenType.Array);
            return token.ToObject<string>();
        }
    }
}
