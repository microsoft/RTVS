// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Editor.Functions {
    public interface IFunctionInfo : IPersistentFunctionInfo {
        /// <summary>
        /// Package the function belongs to
        /// </summary>
        string Package { get; }

        /// <summary>
        /// Function signatures
        /// </summary>
        IReadOnlyList<ISignatureInfo> Signatures { get; }

        /// <summary>
        /// Return value description
        /// </summary>
        string ReturnValue { get; }
    }
}
