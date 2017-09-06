// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.Common.Core.Extensions {
    public static class ClipboardExtensions {
        public static void CopyToClipboard(this string data) {
            try {
                Clipboard.SetData(DataFormats.UnicodeText, FormattableString.Invariant($"\"{data}\""));
            } catch (ExternalException) { }
        }
    }
}
