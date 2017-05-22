// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.Signatures {
    public sealed class SignatureParameterChangedEventArgs: EventArgs {
        public SignatureParameterChangedEventArgs(ISignatureParameterHelp previousParameter, ISignatureParameterHelp newParameter) {
            PreviousParameter = previousParameter;
            NewParameter = newParameter;
        }

        /// <summary>
        /// Parameter that is now the current parameter.
        /// </summary>
        public ISignatureParameterHelp NewParameter { get; }

        /// <summary>
        /// Parameter that was previously the current parameter.
        /// </summary>
        public ISignatureParameterHelp PreviousParameter { get; }
    }
}
