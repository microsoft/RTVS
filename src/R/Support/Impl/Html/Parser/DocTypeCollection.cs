// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;

namespace Microsoft.Html.Core.Parser {
    public static class DocTypeSignatures {
        public static readonly string Html32 = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 3.2//EN\">";
        public static readonly string Html401Transitional = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
        public static readonly string Html401Strict = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">";
        public static readonly string Html401Frameset = "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01 Frameset//EN\" \"http://www.w3.org/TR/html4/frameset.dtd\">";
        public static readonly string Html5 = "<!DOCTYPE html>";
        public static readonly string Xhtml10Transitional = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
        public static readonly string Xhtml10Strict = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">";
        public static readonly string Xhtml10Frameset = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Frameset//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd\">";
        public static readonly string Xhtml11 = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">";
        public static readonly string Xhtml20 = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 2.0//EN\" \"http://www.w3.org/MarkUp/DTD/xhtml2.dtd\">";

        public static DocType GetDocType(string docTypeText) {
            // Unify quotes and replace whitespace with single spaces
            var sb = new StringBuilder();
            bool inWhitespace = false;

            for (int i = 0; i < docTypeText.Length; i++) {
                char ch = docTypeText[i];

                if (Char.IsWhiteSpace(ch)) {
                    if (!inWhitespace) {
                        sb.Append(' ');
                        inWhitespace = true;
                    }
                } else {
                    inWhitespace = false;

                    if (ch == '\'')
                        ch = '\"';

                    sb.Append(ch);
                }
            }

            string docType = sb.ToString();

            if (String.Compare(docType, Html32, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Html32;

            if (String.Compare(docType, Html401Transitional, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Html401Transitional;

            if (String.Compare(docType, Html401Strict, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Html401Strict;

            if (String.Compare(docType, Html401Frameset, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Html401Frameset;

            if (String.Compare(docType, Html5, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Html5;

            if (String.Compare(docType, Xhtml10Transitional, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Xhtml10Transitional;

            if (String.Compare(docType, Xhtml10Strict, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Xhtml10Strict;

            if (String.Compare(docType, Xhtml10Frameset, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Xhtml10Frameset;

            if (String.Compare(docType, Xhtml11, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Xhtml11;

            if (String.Compare(docType, Xhtml20, StringComparison.OrdinalIgnoreCase) == 0)
                return DocType.Xhtml20;

            return DocType.Unrecognized;
        }
    }

}
