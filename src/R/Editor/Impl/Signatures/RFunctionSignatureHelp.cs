// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.QuickInfo;
using Microsoft.Languages.Editor.Signatures;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Implements function signature help in for the editor intellisense session.
    /// </summary>
    public sealed class RFunctionSignatureHelp : IRFunctionSignatureHelp {
        private const int MaxSignatureLength = Functions.SignatureInfo.MaxSignatureLength;

        private readonly IEditorView _view;
        private readonly IEditorBuffer _editorBuffer;
        private readonly IViewSignatureBroker _signatureBroker;

        private IEditorIntellisenseSession _session;
        private ISignatureParameterHelp _currentParameter;
        private ITrackingTextRange _applicableToRange;
        private int _initialPosition;

        public static IRFunctionSignatureHelp Create(IRIntellisenseContext context, IFunctionInfo functionInfo, ISignatureInfo signatureInfo, ITrackingTextRange applicableSpan) {
            var sig = new RFunctionSignatureHelp(context.Session, context.EditorBuffer, functionInfo.Name, string.Empty, signatureInfo);
            var paramList = new List<ISignatureParameterHelp>();

            // Locus points in the pretty printed signature (the one displayed in the tooltip)
            var locusPoints = new List<int>();
            var signatureString = signatureInfo.GetSignatureString(functionInfo.Name, locusPoints);
            sig.Content = signatureString;
            sig.ApplicableToRange = applicableSpan;

            sig.Documentation = functionInfo.Description?.Wrap(Math.Min(MaxSignatureLength, sig.Content.Length));

            Debug.Assert(locusPoints.Count == signatureInfo.Arguments.Count + 1);
            for (var i = 0; i < signatureInfo.Arguments.Count; i++) {
                var p = signatureInfo.Arguments[i];
                if (p != null) {
                    var locusStart = locusPoints[i];
                    var locusLength = locusPoints[i + 1] - locusStart;

                    Debug.Assert(locusLength >= 0);
                    var locus = new TextRange(locusStart, locusLength);

                    // VS may end showing very long tooltip so we need to keep 
                    // description reasonably short: typically about
                    // same length as the function signature.
                    var description = p.Description.Wrap(Math.Min(MaxSignatureLength, sig.Content.Length));
                    paramList.Add(new RSignatureParameterHelp(description, locus, p.Name, sig));
                }
            }

            sig.Parameters = new ReadOnlyCollection<ISignatureParameterHelp>(paramList);
            sig.ComputeCurrentParameter(context.AstRoot, context.Position);

            return sig;
        }

        private RFunctionSignatureHelp(IEditorIntellisenseSession session, IEditorBuffer textBuffer, string functionName, string documentation, ISignatureInfo signatureInfo) {
            FunctionName = functionName;
            SignatureInfo = signatureInfo;

            Documentation = documentation;
            Parameters = null;

            _editorBuffer = textBuffer;
            _view = session.View;

            Session = session;

            _signatureBroker = session.Services.GetService<IViewSignatureBroker>();
            Debug.Assert(_signatureBroker != null);
        }

        #region IRFunctionSignatureHelp
        public IEditorIntellisenseSession Session {
            get => _session;
            set {
                SessionDetached();
                _session = value;
                SessionAttached();
            }
        }

        public ISignatureInfo SignatureInfo { get; }

        public string FunctionName { get; }

        /// <inheritdoc />
        /// <summary>
        /// Content of the signature, including all the characters to be displayed.
        /// </summary>
        public string Content { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Documentation associated with this signature.
        /// </summary>
        public string Documentation { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Span of text in the buffer to which this signature help is applicable.
        /// </summary>
        public ITrackingTextRange ApplicableToRange {
            get => _applicableToRange;
            set {
                if (_editorBuffer != null) {
                    _initialPosition = value.GetStartPoint(_editorBuffer.CurrentSnapshot);
                }
                _applicableToRange = value;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// List of parameters that this signature knows about.
        /// </summary>
        public ReadOnlyCollection<ISignatureParameterHelp> Parameters { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// Content of the signature, pretty-printed into a form suitable for display on-screen.
        /// </summary>
        public string PrettyPrintedContent { get; set; }

        /// <summary>
        /// Occurs when the currently-selected parameter changes.
        /// </summary>
        public event EventHandler<SignatureParameterChangedEventArgs> CurrentParameterChanged;

        /// <inheritdoc />
        /// <summary>
        /// Current parameter for this signature.
        /// </summary>
        public ISignatureParameterHelp CurrentParameter {
            get => _currentParameter;
            set {
                if (_currentParameter != value) {
                    var prevCurrentParameter = _currentParameter;
                    _currentParameter = value;
                    CurrentParameterChanged?.Invoke(this, new SignatureParameterChangedEventArgs(prevCurrentParameter, _currentParameter));
                }
            }
        }
        #endregion

        #region Event handlers
        private void OnTextBufferChanged(object sender, TextChangeEventArgs e) {
            if (_session != null) {
                var position = e.Change.Start + e.Change.NewLength;
                if (position < _initialPosition) {
                    _signatureBroker.DismissSignatureSession(_view);
                } else {
                    UpdateCurrentParameter();
                }
            }
        }

        private void OnCaretPositionChanged(object sender, EventArgs e) {
            if (_view != null && !_session.IsDismissed) {
                UpdateCurrentParameter();
            } else {
                _view.Caret.PositionChanged -= OnCaretPositionChanged;
            }
        }
        #endregion

        private void UpdateCurrentParameter() {
            if (_editorBuffer != null && _view != null) {
                var document = _editorBuffer.GetEditorDocument<IREditorDocument>();
                if (document != null) {
                    var p = _view.GetCaretPosition(_editorBuffer);
                    if (p != null) {
                        document.EditorTree.InvokeWhenReady((o) => {
                            if (_view != null) {
                                // Session is still active
                                p = _view.GetCaretPosition(_editorBuffer);
                                if (p != null) {
                                    ComputeCurrentParameter(document.EditorTree.AstRoot, p.Position);
                                }
                            }
                        }, null, GetType());
                    } else {
                        _signatureBroker.DismissSignatureSession(_view);
                    }
                }
            }
        }

        public void ComputeCurrentParameter(AstRoot ast, int position) {
            if (Parameters == null || Parameters.Count == 0 || _editorBuffer == null) {
                CurrentParameter = null;
                return;
            }

            var settings = _session.Services.GetService<IREditorSettings>();
            var parameterIndex = SignatureInfo.ComputeCurrentParameter(_editorBuffer.CurrentSnapshot, ast, position, settings);
            if (parameterIndex < Parameters.Count) {
                CurrentParameter = Parameters[parameterIndex];
            } else {
                //too many commas, so use the last parameter as the current one.
                CurrentParameter = Parameters[Parameters.Count - 1];
            }
        }

        private void OnSessionDismissed(object sender, EventArgs e) => SessionDetached();

        private void SessionAttached() {
            _session.Dismissed += OnSessionDismissed;
            _editorBuffer.Changed += OnTextBufferChanged;
            _view.Caret.PositionChanged += OnCaretPositionChanged;
        }

        private void SessionDetached() {
            if (_session != null) {
                _session.Dismissed -= OnSessionDismissed;
                _editorBuffer.Changed -= OnTextBufferChanged;
                _view.Caret.PositionChanged -= OnCaretPositionChanged;
            }
        }
    }
}
