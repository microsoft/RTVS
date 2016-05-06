// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Html.Core.Tree;

namespace Microsoft.Html.Core.Parser {
    public class DefaultScriptOrStyleTagNameService : IHtmlScriptOrStyleTagNamesService {
        public static IReadOnlyList<string> DefaultScriptTagNames { get; } = new string[] { "script" };
        public static IReadOnlyList<string> DefaultStyleTagNames { get; } = new string[] { "style" };

        public IReadOnlyList<string> GetScriptTagNames() {
            return DefaultScriptTagNames;
        }

        public IReadOnlyList<string> GetStyleTagNames() {
            return DefaultStyleTagNames;
        }
    }
}
