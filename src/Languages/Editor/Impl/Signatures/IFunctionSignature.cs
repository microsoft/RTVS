// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Signatures {
    public interface IFunctionSignature {
        /// <summary>
        /// Span of text in the buffer to which this signature help is applicable.
        /// </summary>
        ITrackingTextRange ApplicableToSpan { get; }

        /// <summary>
        /// Content of the signature, including all the characters to be displayed.
        /// </summary>
        string Content { get; }

        /// <summary>
        /// Current parameter for this signature.
        /// </summary>
        ISignatureParameter CurrentParameter { get; }

        /// <summary>
        /// Documentation associated with this signature.
        /// </summary>
        string Documentation { get; }

        /// <summary>
        /// List of parameters that this signature knows about.
        /// </summary>
        ReadOnlyCollection<ISignatureParameter> Parameters { get; }

        /// <summary>
        /// Content of the signature, pretty-printed into a form suitable for display
        /// </summary>
        string PrettyPrintedContent { get; }

        //
        // Summary:
        //     Occurs when the currently-selected parameter changes.
        event EventHandler<SignatureParameterChangedEventArgs> CurrentParameterChanged;
    }
}
