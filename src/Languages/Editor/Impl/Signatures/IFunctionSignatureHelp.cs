// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Signatures {
    /// <summary>
    /// Represents visible function signature help (visual tooltip in the editor)
    /// which display function signature, current parameter and its description.
    /// </summary>
    public interface IFunctionSignatureHelp {
        /// <summary>
        /// Function name
        /// </summary>
        string FunctionName { get; }

        /// <summary>
        /// Span of text in the buffer to which this signature help is applicable.
        /// </summary>
        ITrackingTextRange ApplicableToRange { get; set; }

        /// <summary>
        /// Content of the signature, including all the characters to be displayed.
        /// </summary>
        string Content { get; }

        /// <summary>
        /// Current parameter for this signature.
        /// </summary>
        ISignatureParameterHelp CurrentParameter { get; set; }

        /// <summary>
        /// Documentation associated with this signature.
        /// </summary>
        string Documentation { get; }

        /// <summary>
        /// List of parameters that this signature knows about.
        /// </summary>
        ReadOnlyCollection<ISignatureParameterHelp> Parameters { get; }

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
