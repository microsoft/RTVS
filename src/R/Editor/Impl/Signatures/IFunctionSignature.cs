// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Describes function signature. Typically used in various tooltips.
    /// </summary>
    public interface IFunctionSignature {
        string SignatureString { get; }
        string Documentation { get; }
    }
}
