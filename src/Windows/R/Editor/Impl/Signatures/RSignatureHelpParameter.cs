// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Signatures;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Represents an individual parameter description inside the description 
    /// of a function signature in the Visual Studio editor intellisense.
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.language.intellisense.iparameter.aspx
    /// </remarks>
    internal sealed class RSignatureHelpParameter : IParameter {
        public ISignatureParameterHelp SignatureParameterHelp { get; }

        /// <summary>
        /// Documentation associated with the parameter.
        /// </summary>
        public string Documentation => SignatureParameterHelp.Documentation;

        /// <summary>
        /// Location of this parameter relative to the signature's content.
        /// </summary>
        public Span Locus => SignatureParameterHelp.Locus.ToSpan();

        /// <summary>
        /// Name of this parameter.
        /// </summary>
        public string Name => SignatureParameterHelp.Name;

        /// <summary>
        /// Signature of which this parameter is a part.
        /// </summary>
        public ISignature Signature { get; }

        /// <summary>
        /// Text location of this parameter relative to the signature's pretty-printed content.
        /// </summary>
        public Span PrettyPrintedLocus => Locus;

        public RSignatureHelpParameter(ISignature signature, ISignatureParameterHelp parameterHelp) {
            Signature = signature;
            SignatureParameterHelp = parameterHelp;
        }
    }
}
