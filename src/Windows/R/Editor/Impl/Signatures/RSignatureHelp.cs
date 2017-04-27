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
        private readonly IFunctionSignatureHelp _signatureHelp;

        public RSignatureHelp(IFunctionSignatureHelp signatureHelp) {
            _signatureHelp = signatureHelp;
            _signatureHelp.CurrentParameterChanged += OnCurrentParameterChanged;
        }

        public string FunctionName => _signatureHelp.FunctionName;

        #region ISignature
        /// <summary>
        /// Content of the signature, including all the characters to be displayed.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Documentation associated with this signature.
        /// </summary>
        public string Documentation => _signatureHelp.Documentation;

        /// <summary>
        /// Span of text in the buffer to which this signature help is applicable.
        /// </summary>
        public ITrackingSpan ApplicableToSpan {
            get => _signatureHelp.ApplicableToRange.As<ITrackingSpan>();
            set => _signatureHelp.ApplicableToRange = new TrackingTextRange(value);
        }

        /// <summary>
        /// List of parameters that this signature knows about.
        /// </summary>
        public ReadOnlyCollection<IParameter> Parameters
            => new ReadOnlyCollection<IParameter>(_signatureHelp.Parameters.Select(p => new RSignatureHelpParameter(this, p) as IParameter).ToList());

        /// <summary>
        /// Content of the signature, pretty-printed into a form suitable for display on-screen.
        /// </summary>
        public string PrettyPrintedContent { get; set; }

        /// <summary>
        /// Occurs when the currently-selected parameter changes.
        /// </summary>
        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;

        /// <summary>
        /// Current parameter for this signature.
        /// </summary>
        public IParameter CurrentParameter {
            get => new RSignatureHelpParameter(this, _signatureHelp.CurrentParameter);
            set => _signatureHelp.CurrentParameter = (value as RSignatureHelpParameter)?.SignatureParameterHelp;
        }
        #endregion

        private void OnCurrentParameterChanged(object sender, SignatureParameterChangedEventArgs e) {
            var prevParameter = new RSignatureHelpParameter(this, e.PreviousParameter);
            var newParameter = new RSignatureHelpParameter(this, e.NewParameter);
            CurrentParameterChanged?.Invoke(this, new CurrentParameterChangedEventArgs(prevParameter, newParameter));
        }
    }
}
