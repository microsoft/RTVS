using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Languages.Core.Formatting
{
    public sealed class IndentBuilder
    {
        /// <summary>
        /// Whitespace string that represents a single level indentation
        /// </summary>
        public string SingleIndentString { get; private set; }

        /// <summary>
        /// Type of indentation (spaces or tabs)
        /// </summary>
        public IndentType IndentType { get; private set; }

        /// <summary>
        /// Since of indent in characters
        /// </summary>
        public int IndentSize { get; private set; }

        /// <summary>
        /// Size of tab in characters (spaces)
        /// </summary>
        public int TabSize { get; private set; }

        /// <summary>
        /// Current indent level
        /// </summary>
        public int IndentLevel { get; private set; }

        private List<string> _indentStrings;

        public IndentBuilder(IndentType indentType, int indentSize, int tabSize, string baseIndent)
        {
            IndentType = indentType;
            IndentSize = indentSize;
            TabSize = tabSize;

            ResetBaseIndent(baseIndent);

            SingleIndentString = GetIndentString(indentSize);
        }

        public IndentBuilder(IndentType indentType, int indentSize, int tabSize)
            : this(indentType, indentSize, tabSize, String.Empty)
        {
        }

        public IndentState ResetBaseIndent(string baseIndent)
        {
            IndentState indentState = new IndentState(IndentLevel, _indentStrings);

            IndentLevel = 0;

            _indentStrings = new List<string>();
            _indentStrings.Add(baseIndent);

            return indentState;
        }

        public void RestoreIndentState(IndentState indentState)
        {
            IndentLevel = indentState.IndentLevel;
            _indentStrings = indentState.IndentStrings;
        }

        public void NewIndentLevel()
        {
            IndentLevel++;
        }

        public void CloseIndentLevel()
        {
            // Debug.Assert(_indentLevel > 0);

            if (IndentLevel > 0)
                IndentLevel--;
        }

        public void SetIndentLevel(int indentLevel)
        {
            IndentLevel = indentLevel;
        }

        public void SetIndentLevelForSize(int indentSize)
        {
            string baseIndentString = _indentStrings[0];

            int baseIndentSize = TextIndentInSpaces(baseIndentString, TabSize);
            int newIndentLevel = (indentSize - baseIndentSize) / IndentSize;
            
            IndentLevel = Math.Max(newIndentLevel, 0);
        }

        public string GetIndentString(int size)
        {
            return GetIndentString(size, IndentType, TabSize);
        }

        public static string GetIndentString(int size, IndentType indentType, int tabSize)
        {
            StringBuilder sb = new StringBuilder();
            size = Math.Max(size, 0);

            if (indentType == IndentType.Spaces)
            {
                sb.Append(' ', size);
            }
            else
            {
                if (tabSize > 0)
                {
                    int tabs = size / tabSize;
                    int spaces = size % tabSize;

                    if (tabs > 0)
                    {
                        sb.Append('\t', tabs);
                    }

                    if (spaces > 0)
                    {
                        sb.Append(' ', spaces);
                    }
                }
            }

            return sb.ToString();
        }

        public int IndentLevelSize
        {
            get { return IndentLevel * IndentSize; }
        }

        /// <summary>
        /// Provides current indentation string
        /// </summary>
        /// <returns>String for the indent</returns>
        public string IndentLevelString
        {
            get
            {
                if (IndentLevel == 0)
                    return _indentStrings[0];

                if (IndentLevel >= _indentStrings.Count)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(_indentStrings[_indentStrings.Count - 1]);

                    for (int i = _indentStrings.Count; i <= IndentLevel; i++)
                    {
                        sb.Append(SingleIndentString);
                        _indentStrings.Add(sb.ToString());
                    }
                }

                return _indentStrings[IndentLevel];
            }
        }

        ///// <summary>
        ///// Converts a string of spaces and tabs to number of spaces based on the tabsize.
        ///// It assumes this string starts at the absolute beginning of the line to convert
        ///// the tabs correctly
        ///// </summary>
        ///// <param name="indentString"></param>
        ///// <param name="tabSize"></param>
        ///// <returns></returns>
        //public static int ConvertStringToNumberOfSpaces(string indentString, int tabSize, bool fromStartOfLine)
        //{
        //    if (String.IsNullOrEmpty(indentString))
        //    {
        //        return 0;
        //    }

        //    int count = 0;

        //    for (int i = 0; i < indentString.Length; i++)
        //    {
        //        char currentChar = indentString[i];

        //        if (currentChar == '\t')
        //        {
        //            int tabAdder = tabSize;

        //            if (fromStartOfLine)
        //            {
        //                tabAdder -= (count % tabSize);
        //            }

        //            count += tabAdder;
        //        }
        //        else if (currentChar == '\r' || currentChar == '\n')
        //        {
        //            Debug.Fail("We don't expect any new lines in the indent string");
        //        }
        //        else if (Char.IsWhiteSpace(currentChar))
        //        {
        //            count++;
        //        }
        //        else
        //        {
        //            Debug.Fail("We don't expect any non whitespace or tabs in the indent string");
        //        }
        //    }

        //    return count;
        //}

        /// <summary>
        /// Returns a conversion of tabs or space to space count.
        /// You can't simply get the tab size from options, because spaces before tabs
        /// blend in with the tabs, while spaces after the tabs add up.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="spacesSoFar"></param>
        /// <param name="tabSize"></param>
        /// <returns></returns>
        public static int GetWhiteSpaceCharLength(char character, int spacesSoFar, int tabSize)
        {
            Debug.Assert(spacesSoFar >= 0, "number of spaces must be bigger than zero");

            if (character == '\t')
            {
                return tabSize - spacesSoFar % tabSize;
            }

            if (character == '\r' || character == '\n')
            {
                Debug.Fail("We don't expect any new lines here");
                return 1;
            }

            if (Char.IsWhiteSpace(character))
            {
                return 1;
            }

            return 1;
        }

        /// <summary>
        /// Calculates length of text in spaces, converting tabs to spaces using specified tab size.
        /// </summary>
        public static int TextLengthInSpaces(string text, int tabSize)
        {
            int length = 0;
            int spaces = 0;

            for (int i = 0; i < text.Length; i++ )
            {
                char ch = text[i];

                if (ch == '\r' || ch == '\n')
                    break;

                length += IndentBuilder.GetWhiteSpaceCharLength(ch, spaces, tabSize);

                if (ch == ' ')
                    spaces++;
            }

            return length;
        }

        public static int TextIndentInSpaces(string text, int tabSize)
        {
            int spaces = 0;
            int indent = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                if (!Char.IsWhiteSpace(ch))
                    break;

                if (ch == '\r' || ch == '\n')
                    break;

                indent += IndentBuilder.GetWhiteSpaceCharLength(ch, spaces, tabSize);

                if (ch == ' ')
                    spaces++;
            }

            return indent;
        }
    }
}
