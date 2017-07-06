// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Languages.Editor.Signatures;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Implements information source for the Visual Studio editor
    /// on function signatures (parameters, description, etc).
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.language.intellisense.isignature.aspx
    /// </remarks>
    public sealed class RSignatureHelp : ISignature {
        public IRFunctionSignatureHelp FunctionSignatureHelp { get; }

        public RSignatureHelp(IRFunctionSignatureHelp signatureHelp) {
            FunctionSignatureHelp = signatureHelp;
            FunctionSignatureHelp.CurrentParameterChanged += OnCurrentParameterChanged;
        }

        public string FunctionName => FunctionSignatureHelp.FunctionName;

        #region ISignature
        /// <summary>
        /// Content of the signature, including all the characters to be displayed.
        /// </summary>
        public string Content => FunctionSignatureHelp.Content;

        /// <summary>
        /// Documentation associated with this signature.
        /// </summary>
        public string Documentation => FunctionSignatureHelp.Documentation;

        /// <summary>
        /// Span of text in the buffer to which this signature help is applicable.
        /// </summary>
        public ITrackingSpan ApplicableToSpan {
            get => FunctionSignatureHelp.ApplicableToRange.As<ITrackingSpan>();
            set => FunctionSignatureHelp.ApplicableToRange = new TrackingTextRange(value);
        }

        /// <summary>
        /// List of parameters that this signature knows about.
        /// </summary>
        public ReadOnlyCollection<IParameter> Parameters
            => new ReadOnlyCollection<IParameter>(FunctionSignatureHelp.Parameters.Select(p => new RSignatureHelpParameter(this, p) as IParameter).ToList());

        /// <summary>
        /// Content of the signature, pretty-printed into a form suitable for display on-screen.
        /// </summary>
        public string PrettyPrintedContent => FunctionSignatureHelp.PrettyPrintedContent;

        /// <summary>
        /// Occurs when the currently-selected parameter changes.
        /// </summary>
        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;

        /// <summary>
        /// Current parameter for this signature.
        /// </summary>
        public IParameter CurrentParameter {
            get => FunctionSignatureHelp.CurrentParameter != null ? new RSignatureHelpParameter(this, FunctionSignatureHelp.CurrentParameter) : null;
            set => FunctionSignatureHelp.CurrentParameter = (value as RSignatureHelpParameter)?.SignatureParameterHelp;
        }
        #endregion

        private void OnCurrentParameterChanged(object sender, SignatureParameterChangedEventArgs e) {
            var prevParameter = e.PreviousParameter != null ? new RSignatureHelpParameter(this, e.PreviousParameter) : null;
            var newParameter = e.NewParameter != null ? new RSignatureHelpParameter(this, e.NewParameter) : null;
            CurrentParameterChanged?.Invoke(this, new CurrentParameterChangedEventArgs(prevParameter, newParameter));
        }
    }
}
