// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Text range that allows attaching of simple properties
    /// </summary>
    public interface ITextRange<T> : ITextRange {
        T Tag { get; set; }
    }
}
