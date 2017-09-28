// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Validation.Lint {
    public interface ILintOptions {
        bool Enabled { get; set; }

        #region Naming
        /// <summary>
        /// Flag camel-case names
        /// </summary>
        bool CamelCase { get; set; }

        /// <summary>
        /// Flag snake_case
        /// </summary>
        bool SnakeCase { get; set; }

        /// <summary>
        /// Flag Pascal-case names
        /// </summary>
        bool PascalCase { get; set; }

        /// <summary>
        /// Flag UPPERCASE names
        /// </summary>
        bool UpperCase { get; set; }

        /// <summary>
        /// Flag names with.multiple.dots.
        /// </summary>
        bool MultipleDots { get; set; }

        /// <summary>
        /// Verify that name lengths are below the limit
        /// </summary>
        bool NameLength { get; set; }

        /// <summary>
        /// Max name length
        /// </summary>
        int MaxNameLength { get; set; }

        /// <summary>
        /// Flag 'T' or 'F' used instead of 'TRUE' and 'FALSE'
        /// </summary>
        bool TrueFalseNames { get; set; }
        #endregion

        #region Assignment
        /// <summary>
        /// Check that ’&lt;-’ is always used for assignment
        /// </summary>
        bool AssignmentType { get; set; }
        #endregion

        #region Spacing
        /// <summary>
        /// Comma should have space after and no space before unless 
        /// followed by another comma or closing brace. Space between
        /// command and ] or ]] is required.
        /// </summary>
        bool SpacesAroundComma { get; set; }

        /// <summary>
        /// Checks that infix operators are surrounded by spaces unless
        /// it is a named parameter assignment
        /// </summary>
        bool SpacesAroundOperators { get; set; }

        /// <summary>
        /// Check that } is on a separate line unless followed by 'else'
        /// </summary>
        bool CloseCurlySeparateLine { get; set; }

        /// <summary>
        /// Open brace must have space before it unless it is a function call.
        /// </summary>
        bool SpaceBeforeOpenBrace { get; set; }

        /// <summary>
        /// There should be no space after (, [ or [[ and no space before ), ] or ]]
        /// unless ] or ]] is preceded by a comma as in x[1, ]
        /// </summary>
        bool SpacesInsideParenthesis { get; set; }

        /// <summary>
        /// Verify that there is no space after the function name.
        /// </summary>
        bool NoSpaceAfterFunctionName { get; set; }

        /// <summary>
        /// Check that open curly brace is not on its own line
        /// and is followed by a new line.
        /// </summary>
        bool OpenCurlyPosition { get; set; }
        #endregion

        #region Whitespace
        /// <summary>
        /// Verify there are no tabs in the file
        /// </summary>
        bool NoTabs { get; set; }

        /// <summary>
        /// Check there is no trailing whitespace in lines
        /// </summary>
        bool TrailingWhitespace { get; set; }

        /// <summary>
        /// Verify there is no trailing blank lines in the file.
        /// </summary>
        bool TrailingBlankLines { get; set; }
        #endregion

        #region Quotes
        /// <summary>
        /// Verify that only double quotes are used around strings
        /// </summary>
        bool DoubleQuotes { get; set; }
        #endregion

        #region Text
        /// <summary>
        /// Check line lengths in the file
        /// </summary>
        bool LineLength { get; set; }

        /// <summary>
        /// Max line length
        /// </summary>
        int MaxLineLength { get; set; }
        #endregion

        #region Statements
        /// <summary>
        /// Flag semicolons in the file
        /// </summary>
        bool Semicolons { get; set; }

        /// <summary>
        /// Flag multiple statements in the same line
        /// </summary>
        bool MultipleStatements { get; set; }
        #endregion
    }
}
