// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.R.LanguageServer.Server.Settings {
    /// <summary>
    /// Settings that match 'configuration' section in package.json
    /// </summary>
    /// <remarks>
    /// Each member matches one property such as 'r.property'.
    /// For nested properties such as 'r.group.property' there 
    /// should be 'group' member of class type and that class
    /// should have 'property' member.
    /// 
    /// </remarks>
    public sealed class VsCodeRSettings {
        public EditorSettings Editor { get; set; }
        public LinterSettings Linting { get; set; }
    }

    public sealed class EditorSettings {
        public int tabSize { get; set; }
        public bool formatScope { get; set; }
        public bool SpaceAfterKeyword { get; set; }
        public bool spacesAroundEquals { get; set; }
        public bool spaceBeforeCurly { get; set; }
        public bool breakMultipleStatements { get; set; }
    }

    public sealed class LinterSettings {
        public bool Enabled { get; set; }
        public bool camelCase { get; set; }
        public bool snakeCase { get; set; }
        public bool PascalCase { get; set; }
        public bool upperCase { get; set; }
        public bool multipleDots { get; set; }
        public bool nameLength { get; set; }
        public int maxNameLength { get; set; }
        public bool trueFalseNames { get; set; }
        public bool assignmentType { get; set; }
        public bool spacesAroundComma { get; set; }
        public bool spacesAroundOperators { get; set; }
        public bool closeCurlySeparateLine { get; set; }
        public bool spaceBeforeOpenBrace { get; set; }
        public bool spacesInsideParenthesis { get; set; }
        public bool noSpaceAfterFunctionName { get; set; }
        public bool openCurlyPosition { get; set; }
        public bool noTabs { get; set; }
        public bool trailingWhitespace { get; set; }
        public bool trailingBlankLines { get; set; }
        public bool doubleQuotes { get; set; }
        public bool lineLength { get; set; }
        public int maxLineLength { get; set; }
        public bool semicolons { get; set; }
        public bool multipleStatements { get; set; }
    }
}
