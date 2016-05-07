// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Html.Core.Parser.Utility {
    internal class AttributeTable {
        private static string[] _attributes = new string[] {
            // Keep it sorted!
            "accesskey",
            "alt",
            "async",
            "autofocus",
            "checked",
            "cite",
            "class",
            "colspan",
            "datetime",
            "defer",
            "dir",
            "disabled",
            "for",
            "form",
            "height",
            "href",
            "hreflang",
            "id",
            "ismap",
            "lang",
            "longdesc",
            "media",
            "name",
            "nowrap",
            "onbeforeunload",
            "onclick",
            "ondblclick",
            "onerror",
            "onkeydown",
            "onkeypress",
            "onkeyup",
            "onload",
            "onmessage",
            "onmousedown",
            "onmouseup",
            "onmouseout",
            "onmouseover",
            "onoffline",
            "ononline",
            "onpagehide",
            "onpageshow",
            "onpopstate",
            "onresize",
            "onscroll",
            "onstorage",
            "onunload",
            "ping",
            "rel",
            "reversed",
            "rowspan",
            "scoped",
            "span",
            "src",
            "style",
            "tabindex",
            "target",
            "title",
            "type",
            "usemap",
            "value",
            "width",
        };

        public static bool IsKnownAttribute(string candidate) {
            int index = Array.BinarySearch(_attributes, candidate);
            return index >= 0;
        }
    }
}
