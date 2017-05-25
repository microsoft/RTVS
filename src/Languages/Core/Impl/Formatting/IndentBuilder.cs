// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Common.Core;

namespace Microsoft.Languages.Core.Formatting {
    public sealed class IndentBuilder {
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

        public IndentBuilder(IndentType indentType, int indentSize, int tabSize, string baseIndent) {
            IndentType = indentType;
            IndentSize = indentSize;
            TabSize = tabSize;

            ResetBaseIndent(baseIndent);

            SingleIndentString = GetIndentString(indentSize);
        }

        public IndentBuilder(IndentType indentType, int indentSize, int tabSize)
            : this(indentType, indentSize, tabSize, String.Empty) {
        }

        public IndentState ResetBaseIndent(string baseIndent) {
            var indentState = new IndentState(IndentLevel, _indentStrings);
            IndentLevel = 0;

            _indentStrings = new List<string> {baseIndent};
            return indentState;
        }

        public void RestoreIndentState(IndentState indentState) {
            IndentLevel = indentState.IndentLevel;
            _indentStrings = indentState.IndentStrings;
        }

        public void NewIndentLevel() => IndentLevel++;

        public void CloseIndentLevel() {
            // Debug.Assert(_indentLevel > 0);

            if (IndentLevel > 0) {
                IndentLevel--;
            }
        }

        public void SetIndentLevel(int indentLevel) => IndentLevel = indentLevel;

        public void SetIndentLevelForSize(int indentSize) {
            var baseIndentString = _indentStrings[0];

            var baseIndentSize = TextIndentInSpaces(baseIndentString, TabSize);
            var newIndentLevel = (indentSize - baseIndentSize) / IndentSize;

            IndentLevel = Math.Max(newIndentLevel, 0);
        }

        public string GetIndentString(int size) => GetIndentString(size, IndentType, TabSize);

        /// <summary>
        /// Calculates indentation string given indent size in characters, 
        /// type of indent (tabs or spaces) and size of the tab,
        /// </summary>
        /// <param name="size">Desired indent size in characters</param>
        /// <param name="indentType">Type of indent</param>
        /// <param name="tabSize">Tab size</param>
        /// <returns></returns>
        public static string GetIndentString(int size, IndentType indentType, int tabSize) {
            var sb = new StringBuilder();
            size = Math.Max(size, 0);

            if (indentType == IndentType.Spaces) {
                sb.Append(' ', size);
            } else {
                if (tabSize > 0) {
                    var tabs = size / tabSize;
                    var spaces = size % tabSize;

                    if (tabs > 0) {
                        sb.Append('\t', tabs);
                    }

                    if (spaces > 0) {
                        sb.Append(' ', spaces);
                    }
                }
            }

            return sb.ToString();
        }

        public int IndentLevelSize => IndentLevel * IndentSize;

        /// <summary>
        /// Provides current indentation string
        /// </summary>
        /// <returns>String for the indent</returns>
        public string IndentLevelString {
            get {
                if (IndentLevel == 0) {
                    return _indentStrings[0];
                }

                if (IndentLevel >= _indentStrings.Count) {
                    var sb = new StringBuilder();
                    sb.Append(_indentStrings[_indentStrings.Count - 1]);

                    for (var i = _indentStrings.Count; i <= IndentLevel; i++) {
                        sb.Append(SingleIndentString);
                        _indentStrings.Add(sb.ToString());
                    }
                }

                return _indentStrings[IndentLevel];
            }
        }

        /// <summary>
        /// Returns a conversion of tabs or space to space count.
        /// You can't simply get the tab size from options, because spaces before tabs
        /// blend in with the tabs, while spaces after the tabs add up.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="spacesSoFar"></param>
        /// <param name="tabSize"></param>
        /// <returns></returns>
        public static int GetWhiteSpaceCharLength(char character, int spacesSoFar, int tabSize) {
            Debug.Assert(spacesSoFar >= 0, "number of spaces must be bigger than zero");

            if (character == '\t') {
                return tabSize - spacesSoFar % tabSize;
            }

            if (character.IsLineBreak()) {
                Debug.Fail("We don't expect any new lines here");
                return 1;
            }

            if (Char.IsWhiteSpace(character)) {
                return 1;
            }

            return 1;
        }

        /// <summary>
        /// Calculates length of text in spaces, converting tabs to spaces using specified tab size.
        /// </summary>
        public static int TextLengthInSpaces(string text, int tabSize) {
            var length = 0;
            var spaces = 0;

            for (var i = 0; i < text.Length; i++) {
                var ch = text[i];

                if (ch.IsLineBreak()) {
                    break;
                }

                length += IndentBuilder.GetWhiteSpaceCharLength(ch, spaces, tabSize);

                if (ch == ' ') {
                    spaces++;
                }
            }

            return length;
        }

        /// <summary>
        /// Given text string (typically content of a text buffer)
        /// calculates size of indentation (length of the leading
        /// whitespace in the line) in spaces.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tabSize"></param>
        /// <returns></returns>
        public static int TextIndentInSpaces(string text, int tabSize) {
            var spaces = 0;
            var indent = 0;

            for (var i = 0; i < text.Length; i++) {
                var ch = text[i];

                if (!Char.IsWhiteSpace(ch)) {
                    break;
                }

                if (ch.IsLineBreak()) {
                    break;
                }

                indent += IndentBuilder.GetWhiteSpaceCharLength(ch, spaces, tabSize);

                if (ch == ' ') {
                    spaces++;
                }
            }

            return indent;
        }

        /// <summary>
        /// Determines indentation based on the leading whitespace in the current line.
        /// </summary>
        public static int GetLineIndentSize(TextBuilder tb, int position, int tabSize) {
            for (var i = position - 1; i >= 0; i--) {
                if (CharExtensions.IsLineBreak(tb.Text[i])) {
                    return TextIndentInSpaces(tb.Text.Substring(i + 1), tabSize);
                }
            }
            return 0;
        }
    }
}
