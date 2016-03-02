// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Formatting.Data {
    /// <summary>
    /// Wrapper over Windows clipboard object to simplify mocking in tests
    /// </summary>
    interface IClipboardDataProvider {
        bool ContainsData(string format);
        object GetData(string format);
    }
}
