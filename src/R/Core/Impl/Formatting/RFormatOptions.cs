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
        public bool BracesOnNewLine { get; set; } = false;

        public int IndentSize { get; set; } = 2;

        public int TabSize { get; set; } = 2;

        public IndentType IndentType { get; set; } = IndentType.Spaces;

        public bool SpaceAfterComma { get; set; } = true;
        public bool SpaceAfterKeyword { get; set; } = true;
        public bool SpacesAroundEquals { get; set; } = true;
        public bool SpaceBeforeCurly { get; set; } = true;
    }
}
