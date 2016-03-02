// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Languages.Core.Formatting {
    /// <summary>
    /// Used to capture or restore IndentBuilder state when a non-standard indent
    /// increase/decrease is desired.
    /// </summary>
    public sealed class IndentState {
        public IndentState(int indentLevel, List<string> indentStrings) {
            IndentLevel = indentLevel;
            IndentStrings = indentStrings;
        }

        public int IndentLevel { get; private set; }
        public List<string> IndentStrings { get; private set; }
    }
}
