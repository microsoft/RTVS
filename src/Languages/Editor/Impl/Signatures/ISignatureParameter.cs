// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Signatures {
    /// <summary>
    /// Represents an individual parameter description inside the description 
    /// of a signature for Signature Help (Parameter Info)
    /// </summary>
    public interface ISignatureParameter {
        /// <summary>
        /// Parameter description
        /// </summary>
        string Documentation { get; }
        /// <summary>
        /// Location of this parameter relative to the signature's content.
        /// </summary>
        ITextRange Locus { get; }

        /// <summary>
        /// Parameter name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Function signature of which this parameter is a part.
        /// </summary>
        IFunctionSignature Signature { get; }
    }
}
