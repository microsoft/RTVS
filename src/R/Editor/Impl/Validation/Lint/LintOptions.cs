// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Languages.Editor.Settings;
using static System.FormattableString;

namespace Microsoft.R.Editor.Validation.Lint {
    public sealed class LintOptions: ILintOptions {
        private readonly Func<IEditorSettingsStorage> _storageAccess;
        private IEditorSettingsStorage _storage;

        private IEditorSettingsStorage Storage => _storage ?? (_storage = _storageAccess());
        private IWritableEditorSettingsStorage WritableStorage => Storage as IWritableEditorSettingsStorage;

        public LintOptions(Func<IEditorSettingsStorage> storageAccess) {
            Check.ArgumentNull(nameof(storageAccess), storageAccess);
            _storageAccess = storageAccess; // Storage is delay-created after package loads
        }

        private string GetKey(string optionName) => Invariant($"LintR_{optionName}");

        /// <summary>
        /// Enable LintR-like checks
        /// </summary>
        public bool Enabled {
            get => Storage.Get(GetKey(nameof(Enabled)), false);
            set => WritableStorage?.Set(GetKey(nameof(Enabled)), value);
        }

        #region Naming
        /// <summary>
        /// Flag camel-case names
        /// </summary>
        public bool CamelCase {
            get => Storage.Get(GetKey(nameof(CamelCase)), false);
            set => WritableStorage?.Set(GetKey(nameof(CamelCase)), value);
        }

        /// <summary>
        /// Flag snake_case
        /// </summary>
        public bool SnakeCase {
            get => Storage.Get(GetKey(nameof(SnakeCase)), false);
            set => WritableStorage?.Set(GetKey(nameof(SnakeCase)), value);
        }

        /// <summary>
        /// Flag Pascal-case names
        /// </summary>
        public bool PascalCase {
            get => Storage.Get(GetKey(nameof(PascalCase)), true);
            set => WritableStorage?.Set(GetKey(nameof(PascalCase)), value);
        }

        /// <summary>
        /// Flag UPPERCASE names
        /// </summary>
        public bool UpperCase {
            get => Storage.Get(GetKey(nameof(UpperCase)), true);
            set => WritableStorage?.Set(GetKey(nameof(UpperCase)), value);
        }

        /// <summary>
        /// Flag names with.multiple.dots.
        /// </summary>
        public bool MultipleDots {
            get => Storage.Get(GetKey(nameof(MultipleDots)), true);
            set => WritableStorage?.Set(GetKey(nameof(MultipleDots)), value);
        }

        /// <summary>
        /// Verify that name lengths are below the limit
        /// </summary>
        public bool NameLength {
            get => Storage.Get(GetKey(nameof(NameLength)), false);
            set => WritableStorage?.Set(GetKey(nameof(NameLength)), value);
        }

        /// <summary>
        /// Max name length
        /// </summary>
        public int MaxNameLength {
            get => Storage.Get(GetKey(nameof(MaxNameLength)), 32);
            set => WritableStorage?.Set(GetKey(nameof(MaxNameLength)), value);
        }

        /// <summary>
        /// Flag 'T' or 'F' used instead of 'TRUE' and 'FALSE'
        /// </summary>
        public bool TrueFalseNames {
            get => Storage.Get(GetKey(nameof(TrueFalseNames)), true);
            set => WritableStorage?.Set(GetKey(nameof(TrueFalseNames)), value);
        }
        #endregion

        #region Assignment
        /// <summary>
        /// Check that ’&lt;-’ is always used for assignment
        /// </summary>
        public bool AssignmentType {
            get => Storage.Get(GetKey(nameof(AssignmentType)), true);
            set => WritableStorage?.Set(GetKey(nameof(AssignmentType)), value);
        }
        #endregion

        #region Spacing
        /// <summary>
        /// Comma should have space after and no space before unless 
        /// followed by another comma or closing brace. Space between
        /// command and ] or ]] is required.
        /// </summary>
        public bool SpacesAroundComma {
            get => Storage.Get(GetKey(nameof(SpacesAroundComma)), true);
            set => WritableStorage?.Set(GetKey(nameof(SpacesAroundComma)), value);
        }

        /// <summary>
        /// Checks that infix operators are surrounded by spaces unless
        /// it is a named parameter assignment
        /// </summary>
        public bool SpacesAroundOperators {
            get => Storage.Get(GetKey(nameof(SpacesAroundOperators)), true);
            set => WritableStorage?.Set(GetKey(nameof(SpacesAroundOperators)), value);
        }

        /// <summary>
        /// Check that } is on a separate line unless followed by 'else'
        /// </summary>
        public bool CloseCurlySeparateLine {
            get => Storage.Get(GetKey(nameof(CloseCurlySeparateLine)), true);
            set => WritableStorage?.Set(GetKey(nameof(CloseCurlySeparateLine)), value);
        }

        /// <summary>
        /// Open brace must have space before it unless it is a function call.
        /// </summary>
        public bool SpaceBeforeOpenBrace {
            get => Storage.Get(GetKey(nameof(SpaceBeforeOpenBrace)), true);
            set => WritableStorage?.Set(GetKey(nameof(SpaceBeforeOpenBrace)), value);
        }

        /// <summary>
        /// There should be no space after (, [ or [[ and no space before ), ] or ]]
        /// unless ] or ]] is preceded by a comma as in x[1, ]
        /// </summary>
        public bool SpacesInsideParenthesis {
            get => Storage.Get(GetKey(nameof(SpacesInsideParenthesis)), true);
            set => WritableStorage?.Set(GetKey(nameof(SpacesInsideParenthesis)), value);
        }

        /// <summary>
        /// Verify that there is no space after the function name.
        /// </summary>
        public bool NoSpaceAfterFunctionName {
            get => Storage.Get(GetKey(nameof(NoSpaceAfterFunctionName)), true);
            set => WritableStorage?.Set(GetKey(nameof(NoSpaceAfterFunctionName)), value);
        }

        /// <summary>
        /// Check that open curly brace is not on its own line
        /// and is followed by a new line.
        /// </summary>
        public bool OpenCurlyPosition {
            get => Storage.Get(GetKey(nameof(OpenCurlyPosition)), true);
            set => WritableStorage?.Set(GetKey(nameof(OpenCurlyPosition)), value);
        }
        #endregion

        #region Whitespace
        /// <summary>
        /// Verify there are no tabs in the file
        /// </summary>
        public bool NoTabs {
            get => Storage.Get(GetKey(nameof(NoTabs)), true);
            set => WritableStorage?.Set(GetKey(nameof(NoTabs)), value);
        }

        /// <summary>
        /// Check there is no trailing whitespace in lines
        /// </summary>
        public bool TrailingWhitespace {
            get => Storage.Get(GetKey(nameof(TrailingWhitespace)), true);
            set => WritableStorage?.Set(GetKey(nameof(TrailingWhitespace)), value);
        }

        /// <summary>
        /// Verify there is no trailing blank lines in the file.
        /// </summary>
        public bool TrailingBlankLines {
            get => Storage.Get(GetKey(nameof(TrailingBlankLines)), true);
            set => WritableStorage?.Set(GetKey(nameof(TrailingBlankLines)), value);
        }
        #endregion

        #region Quotes
        /// <summary>
        /// Verify that only double quotes are used around strings
        /// </summary>
        public bool DoubleQuotes {
            get => Storage.Get(GetKey(nameof(DoubleQuotes)), true);
            set => WritableStorage?.Set(GetKey(nameof(DoubleQuotes)), value);
        }
        #endregion

        #region Text
        /// <summary>
        /// Check line lengths in the file
        /// </summary>
        public bool LineLength {
            get => Storage.Get(GetKey(nameof(LineLength)), false);
            set => WritableStorage?.Set(GetKey(nameof(LineLength)), value);
        }

        /// <summary>
        /// Max line length
        /// </summary>
        public int MaxLineLength {
            get => Storage.Get(GetKey(nameof(MaxLineLength)), 80);
            set => WritableStorage?.Set(GetKey(nameof(MaxLineLength)), value);
        }
        #endregion

        #region Statements
        /// <summary>
        /// Flag semicolons in the file
        /// </summary>
        public bool Semicolons {
            get => Storage.Get(GetKey(nameof(Semicolons)), true);
            set => WritableStorage?.Set(GetKey(nameof(Semicolons)), value);
        }

        /// <summary>
        /// Flag multiple statements in the same line
        /// </summary>
        public bool MultipleStatements {
            get => Storage.Get(GetKey(nameof(MultipleStatements)), true);
            set => WritableStorage?.Set(GetKey(nameof(MultipleStatements)), value);
        }
        #endregion
    }
}
