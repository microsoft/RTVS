// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace Microsoft.R.Editor.Formatting.Data {
    internal sealed class ClipboardDataProvider : IClipboardDataProvider {
        public bool ContainsData(string format) {
            return Clipboard.ContainsData(format);
        }
        public object GetData(string format) {
            return Clipboard.GetData(format);
        }
    }
}
