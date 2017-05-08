// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Text change event arguments.
    /// </summary>
    public sealed class TextChangeEventArgs : EventArgs {
        public TextChange Change { get; }

        [DebuggerStepThrough]
        public TextChangeEventArgs(TextChange change) => Change = change;
    }
}
