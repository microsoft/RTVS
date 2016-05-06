// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Builder {
    internal class HtmlClosureProvider {
        // URI to provider map
        Dictionary<string, IHtmlClosureProvider> _providers = new Dictionary<string, IHtmlClosureProvider>(StringComparer.Ordinal);
        DefaultHtmlClosureProvider _defaultProvider = new DefaultHtmlClosureProvider();

        /// <summary>
        /// Registers closure provides for a particular element prefix (namespace)
        /// </summary>
        /// <param name="prefix">Namespace prefix</param>
        /// <param name="provider">Closure information provider</param>
        /// <returns>True if registration was successful</returns>
        public bool RegisterProvider(string prefix, IHtmlClosureProvider provider) {
            if (String.IsNullOrEmpty(prefix))
                return false; // can't replace default

            _providers.Add(prefix, provider);
            return true;
        }

        /// <summary>
        /// Determines if element is a self-closing element (i.e. like &lt;br /&gt;
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="prefixRange">Text range of the element prefix</param>
        /// <param name="nameRange">Text range of the element name</param>
        /// <returns>True if element is a self-closing element.</returns>
        public bool IsSelfClosing(ITextProvider textProvider, ITextRange prefixRange, ITextRange nameRange) {
            if (nameRange.Length == 0)
                return false;

            string name = textProvider.GetText(nameRange);
            if (name[0] == '!')
                return true; // bang tags are always self-closing

            if (prefixRange.Length == 0)
                return _defaultProvider.IsSelfClosing(textProvider, nameRange);

            string prefix = textProvider.GetText(prefixRange);

            IHtmlClosureProvider provider; ;
            _providers.TryGetValue(prefix, out provider);

            var textRangeProvider = provider as IHtmlClosureProviderTextRange;
            if (textRangeProvider != null)
                return textRangeProvider.IsSelfClosing(textProvider, nameRange);

            if (provider != null)
                return provider.IsSelfClosing(name);

            return false;
        }

        /// <summary>
        /// Detemines of element can be implicitly closed (i.e. element does not require closing tag) like &lt;td&gt;
        /// Only applies to regular HTML elements. Does not apply to elements with prefixes or namespaces.
        /// </summary>
        /// <param name="textProvider">Text provider</param>
        /// <param name="nameRange">Range of the element name</param>
        /// <returns>True if element can be implicitly closed</returns>
        public bool IsImplicitlyClosed(ITextProvider textProvider, ITextRange nameRange, out string[] containerNames) {
            Debug.Assert(nameRange.Length > 0);

            // Only HTML elements can be implicitly closed
            return _defaultProvider.IsImplicitlyClosed(textProvider, nameRange, out containerNames);
        }

        // TODO: implement namespace providers on schemas
    }
}
