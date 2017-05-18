// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Editor;
using Microsoft.VisualStudio.R.Package.Options.Attributes;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R.Editor {
    public class RLintOptionsDialog: DialogPage {
        private readonly IWritableREditorSettings _settings;

        public RLintOptionsDialog() {
            SettingsRegistryPath = @"UserSettings\R_Lint";
            _settings = VsAppShell.Current.GetService<IWritableREditorSettings>();
        }

        [LocCategory("Settings_LintCategory_All")]
        [CustomLocDisplayName("Settings_LintEnabled")]
        [LocDescription("Settings_LintEnabled_Description")]
        [DefaultValue(true)]
        public bool EnableLint {
            get => _settings.LintOptions.Enabled;
            set => _settings.LintOptions.Enabled = value;
        }

        #region Naming
        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_CamelCase")]
        [LocDescription("Settings_Lint_CamelCase_Description")]
        [DefaultValue(true)]
        public bool CamelCase {
            get => _settings.LintOptions.CamelCase;
            set => _settings.LintOptions.CamelCase = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_SnakeCase")]
        [LocDescription("Settings_Lint_SnakeCase_Description")]
        [DefaultValue(false)]
        public bool SnakeCase {
            get => _settings.LintOptions.SnakeCase;
            set => _settings.LintOptions.SnakeCase = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_PascalCase")]
        [LocDescription("Settings_Lint_PascalCase_Description")]
        [DefaultValue(true)]
        public bool PascalCase {
            get => _settings.LintOptions.PascalCase;
            set => _settings.LintOptions.PascalCase = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_UpperCase")]
        [LocDescription("Settings_Lint_UpperCase_Description")]
        [DefaultValue(true)]
        public bool UpperCase {
            get => _settings.LintOptions.UpperCase;
            set => _settings.LintOptions.UpperCase = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_MultipleDots")]
        [LocDescription("Settings_Lint_MultipleDots_Description")]
        [DefaultValue(true)]
        public bool MultipleDots {
            get => _settings.LintOptions.MultipleDots;
            set => _settings.LintOptions.MultipleDots = value;
        }

        [LocCategory("Settings_LintCategory_Naming")]
        [CustomLocDisplayName("Settings_Lint_NameLength")]
        [LocDescription("Settings_Lint_NameLength_Description")]
        [DefaultValue(true)]
        public bool NameLength {
            get => _settings.LintOptions.NameLength;
            set => _settings.LintOptions.NameLength = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_MaxNameLength")]
        [LocDescription("Settings_Lint_MaxNameLength_Description")]
        [DefaultValue(32)]
        public int MaxNameLength {
            get => _settings.LintOptions.MaxNameLength;
            set => _settings.LintOptions.MaxNameLength = value;
        }
        #endregion

        #region Assignnment
        [LocCategory("Settings_LintCategory_Assignment")]
        [CustomLocDisplayName("Settings_Lint_AssignmentType")]
        [LocDescription("Settings_Lint_AssignmentType_Description")]
        [DefaultValue(true)]
        public bool AssignmentType {
            get => _settings.LintOptions.AssignmentType;
            set => _settings.LintOptions.AssignmentType = value;
        }
        #endregion

        #region Spacing
        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_SpacesAroundComma")]
        [LocDescription("Settings_Lint_SpacesAroundComma_Description")]
        [DefaultValue(true)]
        public bool SpacesAroundComma {
            get => _settings.LintOptions.SpacesAroundComma;
            set => _settings.LintOptions.SpacesAroundComma = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_SpacesAroundOperators")]
        [LocDescription("Settings_Lint_SpacesAroundOperators_Description")]
        [DefaultValue(true)]
        public bool SpacesAroundOperators {
            get => _settings.LintOptions.SpacesAroundOperators;
            set => _settings.LintOptions.SpacesAroundOperators = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_CloseCurlySeparateLine")]
        [LocDescription("Settings_Lint_CloseCurlySeparateLine_Description")]
        [DefaultValue(true)]
        public bool CloseCurlySeparateLine {
            get => _settings.LintOptions.CloseCurlySeparateLine;
            set => _settings.LintOptions.CloseCurlySeparateLine = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_SpaceBeforeOpenBrace")]
        [LocDescription("Settings_Lint_SpaceBeforeOpenBrace_Description")]
        [DefaultValue(true)]
        public bool SpaceBeforeOpenBrace {
            get => _settings.LintOptions.SpaceBeforeOpenBrace;
            set => _settings.LintOptions.SpaceBeforeOpenBrace = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_SpacesInsideParenthesis")]
        [LocDescription("Settings_Lint_SpacesInsideParenthesis_Description")]
        [DefaultValue(true)]
        public bool SpacesInsideParenthesis {
            get => _settings.LintOptions.SpacesInsideParenthesis;
            set => _settings.LintOptions.SpacesInsideParenthesis = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_NoSpaceAfterFunctionName")]
        [LocDescription("Settings_Lint_NoSpaceAfterFunctionName_Description")]
        [DefaultValue(true)]
        public bool NoSpaceAfterFunctionName {
            get => _settings.LintOptions.NoSpaceAfterFunctionName;
            set => _settings.LintOptions.NoSpaceAfterFunctionName = value;
        }

        [LocCategory("Settings_LintCategory_Spacing")]
        [CustomLocDisplayName("Settings_Lint_OpenCurlyPosition")]
        [LocDescription("Settings_Lint_OpenCurlyPosition_Description")]
        [DefaultValue(true)]
        public bool OpenCurlyPosition {
            get => _settings.LintOptions.OpenCurlyPosition;
            set => _settings.LintOptions.OpenCurlyPosition = value;
        }
        #endregion

        #region Whitespace
        [LocCategory("Settings_LintCategory_Whitespace")]
        [CustomLocDisplayName("Settings_Lint_NoTabs")]
        [LocDescription("Settings_Lint_NoTabs_Description")]
        [DefaultValue(true)]
        public bool NoTabs {
            get => _settings.LintOptions.NoTabs;
            set => _settings.LintOptions.NoTabs = value;
        }

        [LocCategory("Settings_LintCategory_Whitespace")]
        [CustomLocDisplayName("Settings_Lint_TrailingWhitespace")]
        [LocDescription("Settings_Lint_TrailingWhitespace_Description")]
        [DefaultValue(true)]
        public bool TrailingWhitespace {
            get => _settings.LintOptions.TrailingWhitespace;
            set => _settings.LintOptions.TrailingWhitespace = value;
        }

        [LocCategory("Settings_LintCategory_Whitespace")]
        [CustomLocDisplayName("Settings_Lint_TrailingBlankLines")]
        [LocDescription("Settings_Lint_TrailingBlankLines_Description")]
        [DefaultValue(true)]
        public bool TrailingBlankLines {
            get => _settings.LintOptions.TrailingBlankLines;
            set => _settings.LintOptions.TrailingBlankLines = value;
        }
        #endregion

        #region Quotes
        [LocCategory("Settings_LintCategory_Whitespace")]
        [CustomLocDisplayName("Settings_Lint_DoubleQuotes")]
        [LocDescription("Settings_Lint_DoubleQuotes_Description")]
        [DefaultValue(true)]
        public bool DoubleQuotes {
            get => _settings.LintOptions.DoubleQuotes;
            set => _settings.LintOptions.DoubleQuotes = value;
        }
        #endregion

        #region Text
        [LocCategory("Settings_LintCategory_Text")]
        [CustomLocDisplayName("Settings_Lint_LineLength")]
        [LocDescription("Settings_Lint_LineLength_Description")]
        [DefaultValue(true)]
        public bool LineLength {
            get => _settings.LintOptions.LineLength;
            set => _settings.LintOptions.LineLength = value;
        }

        [LocCategory("Settings_LintCategory_Text")]
        [CustomLocDisplayName("Settings_Lint_MaxLineLength")]
        [LocDescription("Settings_Lint_MaxLineLength_Description")]
        [DefaultValue(80)]
        public int MaxLineLength {
            get => _settings.LintOptions.MaxLineLength;
            set => _settings.LintOptions.MaxLineLength = value;
        }
        #endregion

        #region Statements
        [LocCategory("Settings_LintCategory_Statements")]
        [CustomLocDisplayName("Settings_Lint_Semicolons")]
        [LocDescription("Settings_Lint_Semicolons_Description")]
        [DefaultValue(false)]
        public bool Semicolons {
            get => _settings.LintOptions.LineLength;
            set => _settings.LintOptions.LineLength = value;
        }

        [LocCategory("Settings_LintCategory_Statements")]
        [CustomLocDisplayName("Settings_Lint_MultipleStatements")]
        [LocDescription("Settings_Lint_MultipleStatements_Description")]
        [DefaultValue(true)]
        public bool MultipleStatements {
            get => _settings.LintOptions.MultipleStatements;
            set => _settings.LintOptions.MultipleStatements = value;
        }
        #endregion
    }
}
