// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Tree.Builder {
    /// <summary>
    /// Default element closure information provider for HTML. Implements IHtmlClosureProviderTextRange 
    /// which allows passing of ITextProvider + TextRange instead of extracting strings for comparison
    /// This is a frequently used class and performance improvement is welcome.
    /// </summary>
    public class DefaultHtmlClosureProvider : IHtmlClosureProvider, IHtmlClosureProviderTextRange {
        // Constants for simple and fast length check
        // before doing actual string search
        static int _minCharCountImplicitClosing = 1;
        static int _maxCharCountImplicitClosing = 6;

        static int _minCharCountSelfClosing = 2;
        static int _maxCharCountSelfClosing = 8;

        static string[][] _voidElementNameIndex;
        static string[][] _implicitlyClosingElementNameIndex;

        #region Constructors
        public DefaultHtmlClosureProvider() {
            if (_voidElementNameIndex != null)
                return;

            _voidElementNameIndex = new string[(int)'z' - (int)'A' + 1][];
            _implicitlyClosingElementNameIndex = new string[(int)'z' - (int)'A' + 1][];

            // If you update the table, make sure you update min/max length as well
            for (int i = 0; i < _implicitlyClosingElementNameIndex.Length; i++)
                _implicitlyClosingElementNameIndex[i] = null;

            AddElements(_implicitlyClosingElementNameIndex, new string[] { "frame" });
            AddElements(_implicitlyClosingElementNameIndex, new string[] { "dd", "dt" });
            AddElements(_implicitlyClosingElementNameIndex, new string[] { "li" });
            AddElements(_implicitlyClosingElementNameIndex, new string[] { "option" });
            AddElements(_implicitlyClosingElementNameIndex, new string[] { "p" });
            AddElements(_implicitlyClosingElementNameIndex, new string[] { "tbody", "td", "tfoot", "th", "thead", "tr" });

            for (int i = 0; i < _voidElementNameIndex.Length; i++)
                _voidElementNameIndex[i] = null;

            // http://dev.w3.org/html5/markup/syntax.html#void-element
            AddElements(_voidElementNameIndex, new string[] { "area" });
            AddElements(_voidElementNameIndex, new string[] { "base", "basefont", "br" });
            AddElements(_voidElementNameIndex, new string[] { "col", "command" });
            AddElements(_voidElementNameIndex, new string[] { "embed" });
            AddElements(_voidElementNameIndex, new string[] { "hr" });
            AddElements(_voidElementNameIndex, new string[] { "img", "input", "isindex" });
            AddElements(_voidElementNameIndex, new string[] { "keygen" });
            AddElements(_voidElementNameIndex, new string[] { "link" });
            AddElements(_voidElementNameIndex, new string[] { "meta" });
            AddElements(_voidElementNameIndex, new string[] { "param" });
            AddElements(_voidElementNameIndex, new string[] { "source" });
            AddElements(_voidElementNameIndex, new string[] { "track" });
            AddElements(_voidElementNameIndex, new string[] { "wbr" });
        }
        #endregion

        #region IHtmlClosureProviderTextRange
        /// <summary>
        /// Determines if given element is self-closing (i.e. does not allow content)
        /// </summary>
        /// <param name="text">Text provider</param>
        /// <param name="nameRange">Element name</param>
        /// <returns>True if element does not allow content</returns>
        public bool IsSelfClosing(ITextProvider text, ITextRange nameRange) {
            if (nameRange.Length < _minCharCountSelfClosing || nameRange.Length > _maxCharCountSelfClosing)
                return false;

            return FindElementName(text, nameRange, _voidElementNameIndex, true);
        }

        /// <summary>
        /// Determines if given element can be implicitly closed like &lt;li&gt; or &lt;td&gt; in HTML"
        /// </summary>
        /// <param name="text">Text provider</param>
        /// <param name="name">Element name</param>
        /// <returns>True if element can be implictly closed</returns>
        public bool IsImplicitlyClosed(ITextProvider text, ITextRange name, out string[] containerElementNames) {
            containerElementNames = null;

            if (name.Length < _minCharCountImplicitClosing || name.Length > _maxCharCountImplicitClosing)
                return false;

            bool found = FindElementName(text, name, _implicitlyClosingElementNameIndex, ignoreCase: true);
            if (found) {
                string elementName = text.GetText(name);
                containerElementNames = GetContainerElements(elementName);
            }
            return found;
        }

        private static bool FindElementName(ITextProvider text, ITextRange name, string[][] indexArray, bool ignoreCase) {
            Char ch = text[name.Start];
            if (ch < 'A' || ch > 'z')
                return false;

            ch -= 'A';
            string[] elementNames = indexArray[ch];
            if (elementNames != null) {
                for (int i = 0; i < elementNames.Length; i++) {
                    if (elementNames[i].Length == name.Length && text.CompareTo(name.Start, name.Length, elementNames[i], ignoreCase))
                        return true;
                }
            }
            return false;
        }

        private static string[] GetContainerElements(string elementName) {
            if (String.IsNullOrEmpty(elementName))
                return new string[0];

            elementName = elementName.ToLower(CultureInfo.InvariantCulture);
            char firstCharLower = elementName[0];

            switch (firstCharLower) {
                case 't':
                    if (String.Compare(elementName, "td", StringComparison.Ordinal) == 0 || String.Compare(elementName, "th", StringComparison.Ordinal) == 0) {
                        return new string[] { "tr", "table" };
                    }

                    if (String.Compare(elementName, "tr", StringComparison.Ordinal) == 0) {
                        return new string[] { "table", "thead", "tfoot", "tbody" };
                    }

                    if (String.Compare(elementName, "thead", StringComparison.Ordinal) == 0 ||
                        String.Compare(elementName, "tfoot", StringComparison.Ordinal) == 0 || String.Compare(elementName, "tbody", StringComparison.Ordinal) == 0) {
                        return new string[] { "table" };
                    }
                    break;
                case 'd':
                    if (String.Compare(elementName, "dd", StringComparison.Ordinal) == 0 || String.Compare(elementName, "dt", StringComparison.Ordinal) == 0) {
                        return new string[] { "dl", "dialog" };
                    }
                    break;
                case 'l':
                    if (String.Compare(elementName, "li", StringComparison.Ordinal) == 0) {
                        return new string[] { "ol", "ul", "menu" };
                    }
                    break;
                case 'o':
                    if (String.Compare(elementName, "option", StringComparison.Ordinal) == 0) {
                        return new string[] { "select", "datalist", "optgroup" };
                    }
                    break;
                case 'f':
                    if (String.Compare(elementName, "frame", StringComparison.Ordinal) == 0) {
                        return new string[] { "frameset" };
                    }
                    break;
            }

            return new string[0];
        }
        #endregion

        private static void AddElements(string[][] addTo, string[] elementNames) {
            char ch = elementNames[0][0];

            Debug.Assert(Char.IsLower(ch));
            char chUpper = Char.ToUpperInvariant(ch);

            addTo[ch - 'A'] = elementNames;
            addTo[chUpper - 'A'] = elementNames;
        }

        #region IHtmlClosureProvider Members
        // Not implement by design, use IElementInfoProviderTextRange 
        bool IHtmlClosureProvider.IsSelfClosing(string name) {
            throw new NotImplementedException();
        }

        bool IHtmlClosureProvider.IsImplicitlyClosed(string name, out string[] containerElementNames) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
