using System;
using System.Collections.ObjectModel;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Signatures
{
    public partial class SignatureHelp : ISignature
    {
        // http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.language.intellisense.isignature.aspx

        protected ITextView TextView { get; set; }
        protected ITextBuffer SubjectBuffer { get; set; }
        protected ISignatureHelpSession Session { get; set; }

        private IParameter _currentParameter;
        private ITrackingSpan _applicableToSpan;
        private int _initialPosition;

        public string FunctionName { get; private set; }

        public SignatureHelp(ISignatureHelpSession session, ITextBuffer subjectBuffer, string functionName, string documentation)
        {
            FunctionName = functionName;

            Documentation = documentation;
            Parameters = null;

            Session = session;
            Session.Dismissed += OnSessionDismissed;

            TextView = session.TextView;

            SubjectBuffer = subjectBuffer;
            SubjectBuffer.Changed += OnSubjectBufferChanged;
        }

        internal static int ComputeCurrentParameter(ITextSnapshot snapshot, AstRoot ast, int position)
        {
            ParametersInfo parameterInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, snapshot, position);
            return parameterInfo != null ? parameterInfo.ParameterIndex : 0;
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
        public ITrackingSpan ApplicableToSpan
        {
            get
            {
                return _applicableToSpan;
            }
            set
            {
                _initialPosition = value.GetStartPoint(SubjectBuffer.CurrentSnapshot);
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
        public IParameter CurrentParameter
        {
            get { return _currentParameter; }
            internal set
            {
                if (_currentParameter != value)
                {
                    IParameter prevCurrentParameter = _currentParameter;
                    _currentParameter = value;

                    if (CurrentParameterChanged != null)
                        CurrentParameterChanged(this, new CurrentParameterChangedEventArgs(prevCurrentParameter, _currentParameter));
                }
            }
        }
        #endregion

        protected virtual void OnSubjectBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            if (Session != null && e.Changes.Count > 0)
            {
                int start, oldLength, newLength;
                TextUtility.CombineChanges(e, out start, out oldLength, out newLength);

                int position = start + newLength;
                if (position < _initialPosition)
                {
                    Session.Dismiss();
                }
                else
                {
                    IREditorDocument document = REditorDocument.FromTextBuffer(e.After.TextBuffer);
                    if (document != null)
                    {
                        SnapshotPoint? p = REditorDocument.MapCaretPositionFromView(TextView);
                        if (p.HasValue)
                        {
                            document.EditorTree.EnsureTreeReady();
                            ComputeCurrentParameter(document.EditorTree.AstRoot, p.Value.Position);
                        }
                        else
                        {
                            Session.Dismiss();
                        }
                    }
                }
            }
        }

        protected virtual void OnSessionDismissed(object sender, EventArgs e)
        {
            Session.Dismissed -= OnSessionDismissed;
            SubjectBuffer.Changed -= OnSubjectBufferChanged;

            Session = null;
            TextView = null;
            SubjectBuffer = null;
        }

        public virtual void ComputeCurrentParameter(AstRoot ast, int position)
        {
            if (Parameters == null || Parameters.Count == 0)
            {
                this.CurrentParameter = null;
                return;
            }

            var parameterIndex = ComputeCurrentParameter(SubjectBuffer.CurrentSnapshot, ast, position);

            if (parameterIndex < Parameters.Count)
            {
                this.CurrentParameter = Parameters[parameterIndex];
            }
            else
            {
                //too many commas, so use the last parameter as the current one.
                this.CurrentParameter = Parameters[Parameters.Count - 1];
            }
        }
    }
}
