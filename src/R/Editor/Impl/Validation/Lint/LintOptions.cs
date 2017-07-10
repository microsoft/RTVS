// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Common.Core.UI;

namespace Microsoft.R.Editor.Validation.Lint {
    public sealed class LintOptions: BindableBase {
        private bool _enabled;
        private bool _camelCase = true;
        private bool _snakeCase;
        private bool _pascalCase = true;
        private bool _upperCase = true;
        private bool _multipleDots = true;
        private bool _nameLength;
        private int _maxNameLength = 32;

        private bool _trueFalseNames = true;
        private bool _assignmentType = true;

        private bool _spacesAroundComma = true;
        private bool _spacesAroundOperators = true;
        private bool _closeCurlySeparateLine = true;
        private bool _spaceBeforeOpenBrace = true;
        private bool _spacesInsideParenthesis = true;
        private bool _noSpaceAfterFunctionName = true;
        private bool _openCurlyPosition = true;

        private bool _noTabs = true;
        private bool _trailingWhitespace = true;
        private bool _trailingBlankLines = true;

        private bool _doubleQuotes = true;
        private bool _lineLength;
        private int _maxLineLength = 80;

        private bool _semicolons = true;
        private bool _multipleStatements = true;

        /// <summary>
        /// Enable LintR-like checks
        /// </summary>
        public bool Enabled {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }

        #region Naming
        /// <summary>
        /// Flag camel-case names
        /// </summary>
        public bool CamelCase {
            get => _camelCase;
            set => SetProperty(ref _camelCase, value);
        }

        /// <summary>
        /// Flag snake_case
        /// </summary>
        public bool SnakeCase {
            get => _snakeCase;
            set => SetProperty(ref _snakeCase, value);
        }

        /// <summary>
        /// Flag Pascal-case names
        /// </summary>
        public bool PascalCase {
            get => _pascalCase;
            set => SetProperty(ref _pascalCase, value);
        }

        /// <summary>
        /// Flag UPPERCASE names
        /// </summary>
        public bool UpperCase {
            get => _upperCase;
            set => SetProperty(ref _upperCase, value);
        }

        /// <summary>
        /// Flag names with.multiple.dots.
        /// </summary>
        public bool MultipleDots {
            get => _multipleDots;
            set => SetProperty(ref _multipleDots, value);
        }

        /// <summary>
        /// Verify that name lengths are below the limit
        /// </summary>
        public bool NameLength {
            get => _nameLength;
            set => SetProperty(ref _nameLength, value);
        }

        /// <summary>
        /// Max name length
        /// </summary>
        public int MaxNameLength {
            get => _maxNameLength;
            set => SetProperty(ref _maxNameLength, value);
        }

        /// <summary>
        /// Flag 'T' or 'F' used instead of 'TRUE' and 'FALSE'
        /// </summary>
        public bool TrueFalseNames {
            get => _trueFalseNames;
            set => SetProperty(ref _trueFalseNames, value);
        }
        #endregion

        #region Assignment
        /// <summary>
        /// Check that ’&lt;-’ is always used for assignment
        /// </summary>
        public bool AssignmentType {
            get => _assignmentType;
            set => SetProperty(ref _assignmentType, value);
        }
        #endregion

        #region Spacing
        /// <summary>
        /// Comma should have space after and no space before unless 
        /// followed by another comma or closing brace. Space between
        /// command and ] or ]] is required.
        /// </summary>
        public bool SpacesAroundComma {
            get => _spacesAroundComma;
            set => SetProperty(ref _spacesAroundComma, value);
        }

        /// <summary>
        /// Checks that infix operators are surrounded by spaces unless
        /// it is a named parameter assignment
        /// </summary>
        public bool SpacesAroundOperators {
            get => _spacesAroundOperators;
            set => SetProperty(ref _spacesAroundOperators, value);
        }

        /// <summary>
        /// Check that } is on a separate line unless followed by 'else'
        /// </summary>
        public bool CloseCurlySeparateLine {
            get => _closeCurlySeparateLine;
            set => SetProperty(ref _closeCurlySeparateLine, value);
        }

        /// <summary>
        /// Open brace must have space before it unless it is a function call.
        /// </summary>
        public bool SpaceBeforeOpenBrace {
            get => _spaceBeforeOpenBrace;
            set => SetProperty(ref _spaceBeforeOpenBrace, value);
        }

        /// <summary>
        /// There should be no space after (, [ or [[ and no space before ), ] or ]]
        /// unless ] or ]] is preceded by a comma as in x[1, ]
        /// </summary>
        public bool SpacesInsideParenthesis {
            get => _spacesInsideParenthesis;
            set => SetProperty(ref _spacesInsideParenthesis, value);
        }

        /// <summary>
        /// Verify that there is no space after the function name.
        /// </summary>
        public bool NoSpaceAfterFunctionName {
            get => _noSpaceAfterFunctionName;
            set => SetProperty(ref _noSpaceAfterFunctionName, value);
        }

        /// <summary>
        /// Check that open curly brace is not on its own line
        /// and is followed by a new line.
        /// </summary>
        public bool OpenCurlyPosition {
            get => _openCurlyPosition;
            set => SetProperty(ref _openCurlyPosition, value);
        }
        #endregion

        #region Whitespace
        /// <summary>
        /// Verify there are no tabs in the file
        /// </summary>
        public bool NoTabs {
            get => _noTabs;
            set => SetProperty(ref _noTabs, value);
        }

        /// <summary>
        /// Check there is no trailing whitespace in lines
        /// </summary>
        public bool TrailingWhitespace {
            get => _trailingWhitespace;
            set => SetProperty(ref _trailingWhitespace, value);
        }

        /// <summary>
        /// Verify there is no trailing blank lines in the file.
        /// </summary>
        public bool TrailingBlankLines {
            get => _trailingBlankLines;
            set => SetProperty(ref _trailingBlankLines, value);
        }
        #endregion

        #region Quotes
        /// <summary>
        /// Verify that only double quotes are used around strings
        /// </summary>
        public bool DoubleQuotes {
            get => _doubleQuotes;
            set => SetProperty(ref _doubleQuotes, value);
        }
        #endregion

        #region Text
        /// <summary>
        /// Check line lengths in the file
        /// </summary>
        public bool LineLength {
            get => _lineLength;
            set => SetProperty(ref _lineLength, value);
        }

        /// <summary>
        /// Max line length
        /// </summary>
        public int MaxLineLength {
            get => _maxLineLength;
            set => SetProperty(ref _maxLineLength, value);
        }
        #endregion

        #region Statements
        /// <summary>
        /// Flag semicolons in the file
        /// </summary>
        public bool Semicolons {
            get => _semicolons;
            set => SetProperty(ref _semicolons, value);
        }

        /// <summary>
        /// Flag multiple statements in the same line
        /// </summary>
        public bool MultipleStatements {
            get => _multipleStatements;
            set => SetProperty(ref _multipleStatements, value);
        }
        #endregion
    }
}
