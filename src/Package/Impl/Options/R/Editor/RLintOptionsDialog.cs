// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Validation.Lint;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Editor {
    public class RLintOptionsDialog: DialogPage {
        private readonly LintOptions _options;

        public RLintOptionsDialog() {
            SettingsRegistryPath = @"UserSettings\R_Lint";
            _options = VsAppShell.Current.GetService<IWritableREditorSettings>().LintOptions;
        }

        [LocCategory("Settings_LintCategory_All")]
        [CustomLocDisplayName("Settings_LintEnabled")]
        [LocDescription("Settings_LintEnabled_Description")]
        [DefaultValue(false)]
        public bool EnableLint {
            get => _options.Enabled;
            set => _options.Enabled = value;
        }

        #region Naming
        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_CamelCase")]
        [LocDescription("Settings_Lint_CamelCase_Description")]
        [DefaultValue(true)]
        public bool CamelCase {
            get => _options.CamelCase;
            set => _options.CamelCase = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_SnakeCase")]
        [LocDescription("Settings_Lint_SnakeCase_Description")]
        [DefaultValue(false)]
        public bool SnakeCase {
            get => _options.SnakeCase;
            set => _options.SnakeCase = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_PascalCase")]
        [LocDescription("Settings_Lint_PascalCase_Description")]
        [DefaultValue(true)]
        public bool PascalCase {
            get => _options.PascalCase;
            set => _options.PascalCase = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_UpperCase")]
        [LocDescription("Settings_Lint_UpperCase_Description")]
        [DefaultValue(true)]
        public bool UpperCase {
            get => _options.UpperCase;
            set => _options.UpperCase = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_MultipleDots")]
        [LocDescription("Settings_Lint_MultipleDots_Description")]
        [DefaultValue(true)]
        public bool MultipleDots {
            get => _options.MultipleDots;
            set => _options.MultipleDots = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_NameLength")]
        [LocDescription("Settings_Lint_NameLength_Description")]
        [DefaultValue(false)]
        public bool NameLength {
            get => _options.NameLength;
            set => _options.NameLength = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_MaxNameLength")]
        [LocDescription("Settings_Lint_MaxNameLength_Description")]
        [DefaultValue(32)]
        public int MaxNameLength {
            get => _options.MaxNameLength;
            set => _options.MaxNameLength = value;
        }
        #endregion

        #region Assignnment
        [LocCategory("Settings_LintCategory_Assignment")]
        [CustomLocDisplayName("Settings_Lint_AssignmentType")]
        [LocDescription("Settings_Lint_AssignmentType_Description")]
        [DefaultValue(true)]
        public bool AssignmentType {
            get => _options.AssignmentType;
            set => _options.AssignmentType = value;
        }
        #endregion

        #region Spacing
        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_SpacesAroundComma")]
        [LocDescription("Settings_Lint_SpacesAroundComma_Description")]
        [DefaultValue(true)]
        public bool SpacesAroundComma {
            get => _options.SpacesAroundComma;
            set => _options.SpacesAroundComma = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_SpacesAroundOperators")]
        [LocDescription("Settings_Lint_SpacesAroundOperators_Description")]
        [DefaultValue(true)]
        public bool SpacesAroundOperators {
            get => _options.SpacesAroundOperators;
            set => _options.SpacesAroundOperators = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_CloseCurlySeparateLine")]
        [LocDescription("Settings_Lint_CloseCurlySeparateLine_Description")]
        [DefaultValue(true)]
        public bool CloseCurlySeparateLine {
            get => _options.CloseCurlySeparateLine;
            set => _options.CloseCurlySeparateLine = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_SpaceBeforeOpenBrace")]
        [LocDescription("Settings_Lint_SpaceBeforeOpenBrace_Description")]
        [DefaultValue(true)]
        public bool SpaceBeforeOpenBrace {
            get => _options.SpaceBeforeOpenBrace;
            set => _options.SpaceBeforeOpenBrace = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_SpacesInsideParenthesis")]
        [LocDescription("Settings_Lint_SpacesInsideParenthesis_Description")]
        [DefaultValue(true)]
        public bool SpacesInsideParenthesis {
            get => _options.SpacesInsideParenthesis;
            set => _options.SpacesInsideParenthesis = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_NoSpaceAfterFunctionName")]
        [LocDescription("Settings_Lint_NoSpaceAfterFunctionName_Description")]
        [DefaultValue(true)]
        public bool NoSpaceAfterFunctionName {
            get => _options.NoSpaceAfterFunctionName;
            set => _options.NoSpaceAfterFunctionName = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_OpenCurlyPosition")]
        [LocDescription("Settings_Lint_OpenCurlyPosition_Description")]
        [DefaultValue(true)]
        public bool OpenCurlyPosition {
            get => _options.OpenCurlyPosition;
            set => _options.OpenCurlyPosition = value;
        }
        #endregion

        #region Whitespace
        [LocCategory("Settings_LintCategory_Whitespace")]
        [CustomLocDisplayName("Settings_Lint_NoTabs")]
        [LocDescription("Settings_Lint_NoTabs_Description")]
        [DefaultValue(true)]
        public bool NoTabs {
            get => _options.NoTabs;
            set => _options.NoTabs = value;
        }

        [LocCategory("Settings_LintCategory_Whitespace")]
        [CustomLocDisplayName("Settings_Lint_TrailingWhitespace")]
        [LocDescription("Settings_Lint_TrailingWhitespace_Description")]
        [DefaultValue(true)]
        public bool TrailingWhitespace {
            get => _options.TrailingWhitespace;
            set => _options.TrailingWhitespace = value;
        }

        [LocCategory("Settings_LintCategory_Whitespace")]
        [CustomLocDisplayName("Settings_Lint_TrailingBlankLines")]
        [LocDescription("Settings_Lint_TrailingBlankLines_Description")]
        [DefaultValue(true)]
        public bool TrailingBlankLines {
            get => _options.TrailingBlankLines;
            set => _options.TrailingBlankLines = value;
        }
        #endregion

        #region Quotes
        [LocCategory("Settings_LintCategory_Whitespace")]
        [CustomLocDisplayName("Settings_Lint_DoubleQuotes")]
        [LocDescription("Settings_Lint_DoubleQuotes_Description")]
        [DefaultValue(true)]
        public bool DoubleQuotes {
            get => _options.DoubleQuotes;
            set => _options.DoubleQuotes = value;
        }
        #endregion

        #region Text
        [LocCategory("Settings_LintCategory_Text")]
        [CustomLocDisplayName("Settings_Lint_LineLength")]
        [LocDescription("Settings_Lint_LineLength_Description")]
        [DefaultValue(false)]
        public bool LineLength {
            get => _options.LineLength;
            set => _options.LineLength = value;
        }

        [LocCategory("Settings_LintCategory_Text")]
        [CustomLocDisplayName("Settings_Lint_MaxLineLength")]
        [LocDescription("Settings_Lint_MaxLineLength_Description")]
        [DefaultValue(80)]
        public int MaxLineLength {
            get => _options.MaxLineLength;
            set => _options.MaxLineLength = value;
        }
        #endregion

        #region Statements
        [LocCategory("Settings_LintCategory_Statements")]
        [CustomLocDisplayName("Settings_Lint_Semicolons")]
        [LocDescription("Settings_Lint_Semicolons_Description")]
        [DefaultValue(false)]
        public bool Semicolons {
            get => _options.LineLength;
            set => _options.LineLength = value;
        }

        [LocCategory("Settings_LintCategory_Statements")]
        [CustomLocDisplayName("Settings_Lint_MultipleStatements")]
        [LocDescription("Settings_Lint_MultipleStatements_Description")]
        [DefaultValue(true)]
        public bool MultipleStatements {
            get => _options.MultipleStatements;
            set => _options.MultipleStatements = value;
        }
        #endregion
    }
}
