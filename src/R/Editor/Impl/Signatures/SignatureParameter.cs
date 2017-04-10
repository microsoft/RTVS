// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Signatures
{
    /// <summary>
    /// Represents an individual parameter description inside the description of a method signature
    /// </summary>
    public class SignatureParameter: IParameter
    {
        // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.language.intellisense.iparameter.aspx

        /// <summary>
        /// Documentation associated with the parameter.
        /// </summary>
        public string Documentation { get; protected set; }

        /// <summary>
        /// Location of this parameter relative to the signature's content.
        /// </summary>
        public Span Locus { get; protected set; }

        /// <summary>
        /// Name of this parameter.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Signature of which this parameter is a part.
        /// </summary>
        public ISignature Signature { get; protected set; }

        /// <summary>
        /// Text location of this parameter relative to the signature's pretty-printed content.
        /// </summary>
        public Span PrettyPrintedLocus { get; protected set; }

        public SignatureParameter(string documentation, Span locus, Span prettyPrintedLocus, string name, ISignature signature)
        {
            Documentation = documentation;
            Locus = locus;
            PrettyPrintedLocus = prettyPrintedLocus;
            Name = name;
            Signature = signature;
        }
    }
}
