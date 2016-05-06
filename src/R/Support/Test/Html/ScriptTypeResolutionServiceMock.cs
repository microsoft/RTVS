// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Tree;

namespace Microsoft.Html.Core.Test.Tree {
    [ExcludeFromCodeCoverage]
    public class ScriptTypeResolutionServiceMock : IHtmlScriptTypeResolutionService {
        public bool IsScriptContentMarkup(string scriptType) {
            return !scriptType.Contains("script");
        }

        public bool IsScriptContentJavaScript(string scriptType) {
            return scriptType.Contains("jscript") || scriptType.Contains("javascript");
        }
    }
}
