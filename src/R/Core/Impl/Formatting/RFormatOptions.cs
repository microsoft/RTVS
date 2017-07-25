// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Formatting;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// R formatting options. Typical styles can be found at
    /// http://google-styleguide.googlecode.com/svn/trunk/Rguide.xml
    /// http://adv-r.had.co.nz/Style.html
    /// </summary>
    public class RFormatOptions {
        /// <summary>
        /// Place open curly brace on a new line
        /// </summary>
        public bool BracesOnNewLine { get; set; } = false;

        public int IndentSize { get; set; } = 2;

        public int TabSize { get; set; } = 2;

        public IndentType IndentType { get; set; } = IndentType.Spaces;

        /// <summary>
        /// Insert space after comma in function arguments
        /// </summary>
        public bool SpaceAfterComma { get; set; } = true;

        /// <summary>
        /// Insert space after keyword and before opening brace such as in case of 'if', 'while', 'repeat'
        /// </summary>
        public bool SpaceAfterKeyword { get; set; } = true;

        /// <summary>
        /// Determines if formatter should place spaces around equals sign
        /// </summary>
        public bool SpacesAroundEquals { get; set; } = true;

        /// <summary>
        /// Determines if formatter should be adding space before opening curly brace
        /// </summary>
        public bool SpaceBeforeCurly { get; set; } = true;

        /// <summary>
        /// When there multiple statements on the same line, break them into separate lines.
        /// </summary>
        public bool BreakMultipleStatements { get; set; } = true;
    }
}
