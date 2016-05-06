// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Extensions {
    public static class Whitespace {
        public static bool IsWhitespaceBeforeElement(this ElementNode element) {
            char charBefore = element.Start > 0 ? element.TextProvider[element.Start - 1] : 'x';
            return Char.IsWhiteSpace(charBefore);
        }

        public static bool IsWhitespaceBeforeStartTag(this ElementNode element) {
            char charBefore = CharBeforeStartTag(element);

            if (charBefore != '\0')
                return Char.IsWhiteSpace(charBefore);

            return false;
        }

        public static bool IsWhitespaceAfterStartTag(this ElementNode element) {
            char charAfter = CharAfterStartTag(element);

            if (charAfter != '\0')
                return Char.IsWhiteSpace(charAfter);

            return false;
        }

        public static char CharBeforeStartTag(this ElementNode element) {
            if (element.Start > 0) {
                var textProvider = element.TextProvider;
                int textLength = textProvider.Length;
                int startTagStart = element.Start;

                return (startTagStart > 0 && startTagStart < textLength) ? textProvider[startTagStart - 1] : '\0';
            }

            return '\0';
        }

        public static char CharAfterStartTag(this ElementNode element) {
            if (element.InnerRange.Length > 0) {
                var textProvider = element.TextProvider;
                int textLength = textProvider.Length;
                int startTagEnd = element.InnerRange.Start;

                return startTagEnd < textLength ? textProvider[startTagEnd] : '\0';
            }

            return '\0';
        }

        public static char CharBeforeEndTag(this ElementNode element) {
            if (element.EndTag != null) {
                var textProvider = element.TextProvider;
                int beforeEndTag = element.EndTag.Start - 1;

                return textProvider[beforeEndTag];
            }

            return '\0';
        }

        public static char CharAfterEndTag(this ElementNode element) {
            if (element.EndTag != null) {
                var textProvider = element.TextProvider;
                int afterEndTag = element.EndTag.End;

                return textProvider[afterEndTag];
            }

            return '\0';
        }

        /// <summary>
        /// Determines if there is whitespace character before end tag
        /// </summary>
        public static bool IsWhitespaceBeforeEndTag(this ElementNode element) {
            char charBefore = CharBeforeEndTag(element);

            if (charBefore != '\0')
                return Char.IsWhiteSpace(charBefore);

            return false;
        }

        /// <summary>
        /// Determines if start tag is followed by a whitespace
        /// </summary>
        public static bool IsWhitespaceAfterEndTag(this ElementNode element) {
            char charAfter = CharAfterEndTag(element);

            if (charAfter != '\0')
                return Char.IsWhiteSpace(charAfter);

            return false;
        }

        /// <summary>
        /// Determines if whitespace that follows start tag contains line breaks.
        /// </summary>
        public static bool IsLineBreakAfterStartTag(this ElementNode element) {
            if (element.IsShorthand())
                return false;

            return element.Root.TextProvider.LineBreaksAfterPosition(element.StartTag.End) > 0;
        }

        /// <summary>
        /// Determines if whitespace that precedes end tag contains line breaks.
        /// </summary>
        public static bool IsLineBreakBeforeEndTag(this ElementNode element) {
            if (element.IsShorthand() || element.EndTag == null)
                return false;

            return element.Root.TextProvider.LineBreaksBeforePosition(element.EndTag.Start) > 0;
        }

        /// <summary>
        /// Determines if element inner range excluding children is all empty
        /// </summary>
        public static bool IsInnerRangeEmpty(this ElementNode element) {
            int lengthWithoutChildren = element.InnerRange.Length;

            for (int i = 0; i < element.Children.Count; i++) {
                var child = element.Children[i];
                lengthWithoutChildren -= child.OuterRange.Length;
            }

            if (lengthWithoutChildren > 0)
                return false;

            for (int i = 0; i < element.Children.Count; i++) {
                var child = element.Children[i];
                if (!child.IsInnerRangeEmpty())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if element inner range only consists of whitespace characters
        /// </summary>
        public static bool IsInnerRangeWhitespace(this ElementNode element) {
            return String.IsNullOrWhiteSpace(element.TextProvider.GetText(element.InnerRange));
        }

        /// <summary>
        /// Determines if inner range including child elements starts with line break.
        /// For example, true in &lt;div>&lt;div>&lt;div>\n
        /// </summary>
        public static bool InnerRangeStartsWithWhitespace(this ElementNode element) {
            // Drill into children and find if any of the first children 
            // inner range starts with a whitespace or any of the last
            // children inner range ends with a whitespace.

            if (element.IsWhitespaceAfterStartTag())
                return true;

            if (element.Children.Count > 0)
                return InnerRangeStartsWithWhitespace(element.Children[0]);

            return false;
        }

        /// <summary>
        /// Determines if inner range including child elements ends with line break.
        /// For example, true in \r\n&lt;/div>&lt;/div>&lt;/div>
        /// </summary>
        public static bool InnerRangeEndsWithWhitespace(this ElementNode element) {
            // Drill into children and find if any of the first children 
            // inner range starts with a whitespace or any of the last
            // children inner range ends with a whitespace.

            if (element.IsWhitespaceBeforeEndTag())
                return true;

            if (element.Children.Count > 0)
                return InnerRangeEndsWithWhitespace(element.Children[element.Children.Count - 1]);

            return false;
        }

        public static bool IsNewLine(this char c) {
            return c == '\r' || c == '\n';
        }

        public static bool IsNewLine(this string s) {
            int len = s.Length;
            if (len == 1) {
                return IsNewLine(s[0]);
            } else if (len == 2) {
                return s[0] == '\r' && s[1] == '\n';
            }

            return false;
        }

        private static char[] _lineBreaks = { '\r', '\n' };
        public static int FirstLineBreakIndex(this string s) {
            return s.IndexOfAny(_lineBreaks);
        }

        public static bool ContainsLineBreak(this string s) {
            return FirstLineBreakIndex(s) >= 0;
        }

        public static string FirstLine(this string s) {
            int lineBreakIndex = s.FirstLineBreakIndex();
            if (lineBreakIndex < 0)
                return s;

            return s.Substring(0, lineBreakIndex);
        }

        public static bool IsAllWhitespace(this string s) {
            for (int i = 0; i < s.Length; i++) {
                if (!Char.IsWhiteSpace(s[i]))
                    return false;
            }

            return true;
        }

        public static string GetLeadingWhitespace(this string s) {
            string leadingWhitespace = s;
            for (int i = 0; i < s.Length; i++) {
                if (!Char.IsWhiteSpace(s[i])) {
                    leadingWhitespace = s.Substring(0, i);
                    break;
                }
            }

            return leadingWhitespace;
        }

        public static string TrimXmlNamespace(this string s) {
            int colonIndex = s.LastIndexOf(':');
            if (colonIndex >= 0)
                s = s.Substring(colonIndex + 1);

            return s;
        }

        // TODO: We should remove this once the razor v3 assembly is update to
        //  a version that no longer calls this.
        public static int GetMatchingPrefixLen(this string s1, string s2) {
            int matchingLen = 0;
            int maxMatchingLen = Math.Min(s1.Length, s2.Length);

            while (matchingLen < maxMatchingLen) {
                if (s1[matchingLen] != s2[matchingLen]) {
                    break;
                }

                matchingLen++;
            }

            return matchingLen;
        }

        public static int GetMatchingSuffixLen(this string s1, string s2) {
            int matchingLen = 0;
            int maxMatchingLen = Math.Min(s1.Length, s2.Length);

            int s1End = s1.Length - 1;
            int s2End = s2.Length - 1;

            while (matchingLen < maxMatchingLen) {
                if (s1[s1End - matchingLen] != s2[s2End - matchingLen]) {
                    break;
                }

                matchingLen++;
            }

            return matchingLen;
        }
    }
}
