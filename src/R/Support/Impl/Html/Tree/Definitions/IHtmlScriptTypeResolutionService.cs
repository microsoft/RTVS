// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Tree {
    /// <summary>
    /// A service that helps parser to determine if script is really script or
    /// is a markup such as in jQuery or Handlebars templates where script
    /// block can have type of 'text/handlebars' and contain markup.
    /// </summary>
    public interface IHtmlScriptTypeResolutionService {
        /// <summary>
        /// Determines if script block type should be treated as markup
        /// </summary>
        /// <param name="scriptType">Value of type attribute of the scring block</param>
        /// <returns>True if script block type should be treated as markup</returns>
        bool IsScriptContentMarkup(string scriptType);

        bool IsScriptContentJavaScript(string scriptType);
    }
}
