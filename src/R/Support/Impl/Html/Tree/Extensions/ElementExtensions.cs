// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Html.Core.Tree.Nodes;

namespace Microsoft.Html.Core.Tree.Extensions {
    public static class ElementExtensions {
        /// <summary>
        /// Determines if element spans multiple lines. In case of implicitly 
        /// closed element (element is missing its end tag) trailing whitespace 
        /// does not count as separate lines.
        /// </summary>
        /// <param name="element">HTML element</param>
        public static bool SpansMultipleLines(this ElementNode element) {
            var text = element.Root.TextProvider.GetText(element.OuterRange);

            if (element.EndTag == null)
                text = text.TrimEnd();

            return text.IndexOfAny(new char[] { '\n', '\r' }) >= 0;
        }

        /// <summary>
        /// Determines if element's content spans multiple lines. In case of implicitly 
        /// closed element (element is missing its end tag) trailing whitespace 
        /// does not count as separate lines.
        /// </summary>
        /// <param name="element">HTML element</param>
        public static bool ContentSpansMultipleLines(this ElementNode element) {
            var text = element.Root.TextProvider.GetText(element.InnerRange);

            if (element.EndTag == null)
                text = text.TrimEnd();

            return text.IndexOfAny(new char[] { '\n', '\r' }) >= 0;
        }

        /// <summary>
        /// Determines if element contains external content such as CSS or script code
        /// </summary>
        public static bool ContainsExternalContent(this ElementNode element) {
            if (element.IsScriptBlock()) {
                var typeAttribute = element.GetAttribute("type", element.Root.Tree.IgnoreCase);
                if (typeAttribute != null && typeAttribute.HasValue()) {
                    var scriptTypeResolution = element.Root.Tree.ScriptTypeResolution;
                    if (scriptTypeResolution != null)
                        return !scriptTypeResolution.IsScriptContentMarkup(typeAttribute.Value);
                }

                return true;
            }

            return element.IsStyleBlock();
        }
    }
}
