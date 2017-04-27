// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Signatures;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Represents an individual parameter inside function signature during
    /// editor intellisense session. Common to all platforms.
    /// </summary>
    internal sealed class RSignatureParameterHelp : ISignatureParameterHelp {
        /// <summary>
        /// Documentation associated with the parameter.
        /// </summary>
        public string Documentation { get; }

        /// <summary>
        /// Location of this parameter relative to the signature's content.
        /// </summary>
        public ITextRange Locus { get; }

        /// <summary>
        /// Name of this parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Function signature of which this parameter is a part.
        /// </summary>
        public IFunctionSignatureHelp Signature { get; }

        public RSignatureParameterHelp(string documentation, ITextRange locus, string name, IFunctionSignatureHelp signature) {
            Documentation = documentation;
            Locus = locus;
            Name = name;
            Signature = signature;
        }
    }
}
