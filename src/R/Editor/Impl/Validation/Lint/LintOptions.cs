// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Validation.Lint {
    public sealed class LintOptions {
        /// <summary>
        /// Enable LintR-like checks
        /// </summary>
        public bool Enabled { get; set; }

        #region Naming
        /// <summary>
        /// Flag camel-case names
        /// </summary>
        public bool CamelCase { get; set; } = true;

        /// <summary>
        /// Flag snake_case
        /// </summary>
        public bool SnakeCase { get; set; }

        /// <summary>
        /// Flag Pascal-case names
        /// </summary>
        public bool PascalCase { get; set; } = true;

        /// <summary>
        /// Flag UPPERCASE names
        /// </summary>
        public bool UpperCase { get; set; } = true;

        /// <summary>
        /// Flag names with.multiple.dots.
        /// </summary>
        public bool MultipleDots { get; set; } = true;

        /// <summary>
        /// Verify that name lengths are below the limit
        /// </summary>
        public bool NameLength { get; set; }

        /// <summary>
        /// Max name length
        /// </summary>
        public int MaxNameLength { get; set; } = 32;

        /// <summary>
        /// Flag 'T' or 'F' used instead of 'TRUE' and 'FALSE'
        /// </summary>
        public bool TrueFalseNames { get; set; } = true;
        #endregion

        #region Assignment
        /// <summary>
        /// Check that ’&lt;-’ is always used for assignment
        /// </summary>
        public bool AssignmentType { get; set; } = true;
        #endregion

        #region Spacing
        /// <summary>
        /// Comma should have space after and no space before unless 
        /// followed by another comma or closing brace. Space between
        /// command and ] or ]] is required.
        /// </summary>
        public bool SpacesAroundComma { get; set; } = true;

        /// <summary>
        /// Checks that infix operators are surrounded by spaces unless
        /// it is a named parameter assignment
        /// </summary>
        public bool SpacesAroundOperators { get; set; } = true;

        /// <summary>
        /// Check that } is on a separate line unless followed by 'else'
        /// </summary>
        public bool CloseCurlySeparateLine { get; set; } = true;

        /// <summary>
        /// Open brace must have space before it unless it is a function call.
        /// </summary>
        public bool SpaceBeforeOpenBrace { get; set; } = true;

        /// <summary>
        /// There should be no space after (, [ or [[ and no space before ), ] or ]]
        /// unless ] or ]] is preceded by a comma as in x[1, ]
        /// </summary>
        public bool SpacesInsideParenthesis { get; set; } = true;

        /// <summary>
        /// Verify that there is no space after the function name.
        /// </summary>
        public bool NoSpaceAfterFunctionName { get; set; } = true;

        /// <summary>
        /// Check that open curly brace is not on its own line
        /// and is followed by a new line.
        /// </summary>
        public bool OpenCurlyPosition { get; set; } = true;
        #endregion

        #region Whitespace
        /// <summary>
        /// Verify there are no tabs in the file
        /// </summary>
        public bool NoTabs { get; set; } = true;

        /// <summary>
        /// Check there is no trailing whitespace in lines
        /// </summary>
        public bool TrailingWhitespace { get; set; } = true;

        /// <summary>
        /// Verify there is no trailing blank lines in the file.
        /// </summary>
        public bool TrailingBlankLines { get; set; } = true;
        #endregion

        #region Quotes
        /// <summary>
        /// Verify that only double quotes are used around strings
        /// </summary>
        public bool DoubleQuotes { get; set; } = true;
        #endregion

        #region Text
        /// <summary>
        /// Check line lengths in the file
        /// </summary>
        public bool LineLength { get; set; }

        /// <summary>
        /// Max line length
        /// </summary>
        public int MaxLineLength { get; set; } = 80;
        #endregion

        #region Statements
        /// <summary>
        /// Flag semicolons in the file
        /// </summary>
        public bool Semicolons { get; set; }

        /// <summary>
        /// Flag multiple statements in the same line
        /// </summary>
        public bool MultipleStatements { get; set; } = true;
        #endregion
    }
}
