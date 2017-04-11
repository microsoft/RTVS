﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Signatures {
    // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.language.intellisense.isignature.aspx
    public partial class SignatureHelp : ISignature {
        private readonly ISignatureInfo _signatureInfo;
        private readonly IApplication _app;

        private IParameter _currentParameter;
        private ITrackingSpan _applicableToSpan;
        private int _initialPosition;

        protected ITextView TextView { get; set; }
        protected ITextBuffer SubjectBuffer { get; set; }
        protected ISignatureHelpSession Session { get; set; }

        public string FunctionName { get; private set; }

        public SignatureHelp(ISignatureHelpSession session, ITextBuffer subjectBuffer, string functionName, string documentation, ISignatureInfo signatureInfo, IApplication app) {
            FunctionName = functionName;
            _signatureInfo = signatureInfo;
            _app = app;

            Documentation = documentation;
            Parameters = null;

            Session = session;
            Session.Dismissed += OnSessionDismissed;

            TextView = session.TextView;
            TextView.Caret.PositionChanged += OnCaretPositionChanged;

            SubjectBuffer = subjectBuffer;
            SubjectBuffer.Changed += OnSubjectBufferChanged;
        }

        internal int ComputeCurrentParameter(ITextSnapshot snapshot, AstRoot ast, int position) {
            ParameterInfo parameterInfo = ast.GetParametersInfoFromBuffer(snapshot, position);
            int index = 0;

            if (parameterInfo != null) {
                index = parameterInfo.ParameterIndex;
                if (parameterInfo.NamedParameter) {
                    // A function f <- function(foo, bar) is said to have formal parameters "foo" and "bar", 
                    // and the call f(foo=3, ba=13) is said to have (actual) arguments "foo" and "ba".
                    // R first matches all arguments that have exactly the same name as a formal parameter. 
                    // Two identical argument names cause an error. Then, R matches any argument names that
                    // partially matches a(yet unmatched) formal parameter. But if two argument names partially 
                    // match the same formal parameter, that also causes an error. Also, it only matches 
                    // formal parameters before ... So formal parameters after ... must be specified using 
                    // their full names. Then the unnamed arguments are matched in positional order to 
                    // the remaining formal arguments.

                    int argumentIndexInSignature = _signatureInfo.GetArgumentIndex(parameterInfo.ParameterName, _app.Services.GetService<IREditorSettings>().PartialArgumentNameMatch);
                    if (argumentIndexInSignature >= 0) {
                        index = argumentIndexInSignature;
                    }
                }
            }
            return index;
        }

        #region ISignature
        /// <summary>
        /// Content of the signature, including all the characters to be displayed.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Documentation associated with this signature.
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// Span of text in the buffer to which this signature help is applicable.
        /// </summary>
        public ITrackingSpan ApplicableToSpan {
            get {
                return _applicableToSpan;
            }
            set {
                if (SubjectBuffer != null) {
                    _initialPosition = value.GetStartPoint(SubjectBuffer.CurrentSnapshot);
                }

                _applicableToSpan = value;
            }
        }

        /// <summary>
        /// List of parameters that this signature knows about.
        /// </summary>
        public ReadOnlyCollection<IParameter> Parameters { get; set; }

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
            get { return _currentParameter; }
            internal set {
                if (_currentParameter != value) {
                    IParameter prevCurrentParameter = _currentParameter;
                    _currentParameter = value;

                    if (CurrentParameterChanged != null)
                        CurrentParameterChanged(this, new CurrentParameterChangedEventArgs(prevCurrentParameter, _currentParameter));
                }
            }
        }
        #endregion

        protected virtual void OnSubjectBufferChanged(object sender, TextContentChangedEventArgs e) {
            if (Session != null && e.Changes.Count > 0) {
                int start, oldLength, newLength;
                TextUtility.CombineChanges(e, out start, out oldLength, out newLength);

                int position = start + newLength;
                if (position < _initialPosition) {
                    DismissSession(TextView, _app);
                } else {
                    UpdateCurrentParameter();
                }
            }
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            if (TextView != null) {
                if (SignatureHelper.IsSameSignatureContext(TextView, SubjectBuffer, _app.Services.GetService<ISignatureHelpBroker>())) {
                    UpdateCurrentParameter();
                } else {
                    DismissSession(TextView, _app, retrigger: true);
                }
            } else {
                e.TextView.Caret.PositionChanged -= OnCaretPositionChanged;
            }
        }

        private void UpdateCurrentParameter() {
            if (SubjectBuffer != null && TextView != null) {
                var document = SubjectBuffer.GetDocument<IREditorDocument>();
                if (document != null) {
                    SnapshotPoint? p = TextView.GetCaretPosition(document.EditorBuffer);
                    if (p.HasValue) {
                        document.EditorTree.InvokeWhenReady((o) => {
                            if (TextView != null) {
                                // Session is still active
                                p = TextView.GetCaretPosition(document.EditorBuffer);
                                if (p.HasValue) {
                                    ComputeCurrentParameter(document.EditorTree.AstRoot, p.Value.Position);
                                }
                            }
                        }, null, this.GetType());
                    } else {
                        DismissSession(TextView, _app);
                    }
                }
            }
        }

        public virtual void ComputeCurrentParameter(AstRoot ast, int position) {
            if (Parameters == null || Parameters.Count == 0 || SubjectBuffer == null) {
                this.CurrentParameter = null;
                return;
            }

            var parameterIndex = ComputeCurrentParameter(SubjectBuffer.CurrentSnapshot, ast, position);
            if (parameterIndex < Parameters.Count) {
                this.CurrentParameter = Parameters[parameterIndex];
            } else {
                //too many commas, so use the last parameter as the current one.
                this.CurrentParameter = Parameters[Parameters.Count - 1];
            }
        }

        protected virtual void OnSessionDismissed(object sender, EventArgs e) {
            if (Session != null) {
                Session.Dismissed -= OnSessionDismissed;
                Session = null;
            }

            if (SubjectBuffer != null) {
                SubjectBuffer.Changed -= OnSubjectBufferChanged;
                SubjectBuffer = null;
            }

            if (TextView != null) {
                TextView.Caret.PositionChanged -= OnCaretPositionChanged;
                TextView = null;
            }
        }
    }
}
