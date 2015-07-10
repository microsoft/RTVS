using System;

namespace Microsoft.Html.Editor.Tree
{
    enum RangeContentType
    {
        /// <summary>Range only contains whitespace</summary>
        Whitespace,
        /// <summary>Range contains plain text</summary>
        Text,
        /// <summary>Range contains markup characters</summary>
        Markup,
        /// <summary>Range contains quotes.</summary>
        Quotes
    }

    internal static class RangeContent
    {
        private static char[] _quoteChars = new char[] { '\'', '\"' };
        private static char[] _unsafeAttributeMarkupChars = new char[] { '<', '>', '/' };

        /// <summary>
        /// Determines type of the supplied text
        /// </summary>
        /// <param name="text">Text to analyze</param>
        /// <returns>Type of text content: whitespace, plain text, markup or quotes</returns>
        public static RangeContentType GetRangeContentType(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return RangeContentType.Whitespace;
            else if (text.IndexOfAny(_unsafeAttributeMarkupChars) >= 0)
                return RangeContentType.Markup;
            else if (text.IndexOfAny(_quoteChars) >= 0)
                return RangeContentType.Quotes;

            return RangeContentType.Text;
        }
    }
}
