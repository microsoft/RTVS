using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Utility;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Validation.Definitions;
using Microsoft.R.Editor.Validation.Errors;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Validation
{
    /// <summary>
    /// Main R validator (syntax check, etc) that generates error 
    /// list items and squiggles. Works asynchronously except
    /// the final part that pushes actual task items to the IDE.
    /// </summary>
    public sealed class TreeValidator
    {
        private static BooleanSwitch _traceValidation =
            new BooleanSwitch("traceRValidation", "Trace R validation events in debug window.");
        public BooleanSwitch TraceValidation
        {
            get { return _traceValidation; }
        }

        private static int _validationDelay = 1000;

        /// <summary>
        /// Queue of validation results. Typically accessed from the main 
        /// thread that pushes errors/warning into a task list window. 
        /// Code that places items on the task list should be checking if element that the error
        /// is about still exist in the document.
        /// </summary>
        internal ConcurrentQueue<IValidationError> ValidationResults { get; private set; }

        private EditorTree _editorTree;
        private bool _validationEnabled = false;
        private bool _errorsAsWarnings = true;
        private bool _validationStarted = false;

        private bool _advisedToIdleTime = false;
        private DateTime _idleRequestTime = DateTime.UtcNow;

        /// <summary>
        /// Fires when validator is cleared. Typically when validation was switched off
        /// so task list and all error tags must be removed from the editor.
        /// </summary>
        public event EventHandler<EventArgs> Cleared;

        #region Constructors
        public TreeValidator(EditorTree editorTree)
        {
#if DEBUG
            TraceValidation.Enabled = false;
#endif

            _editorTree = editorTree;

            _editorTree.NodesRemoved += OnNodesRemoved;
            _editorTree.UpdateCompleted += OnTreeUpdateCompleted;
            _editorTree.Closing += OnTreeClose;

            _validationEnabled = RSettings.ValidationEnabled;

            // Advise to settings changed after accessing the RSettings, as accessing
            // the settings may actually fire the Changed notification for some reason.
            RSettings.Changed += OnSettingsChanged;

            // We don't want to start validation right away since it may interfere with 
            // editor perceived startup performance primarily because queueing the entire
            // tree of elements is done from main thread and may take some time.
            // We'll do it on next idle instead.

            StartValidationNextIdle();
            ValidationResults = new ConcurrentQueue<IValidationError>();

            ServiceManager.AddService<TreeValidator>(this, editorTree.TextBuffer);
        }
        #endregion

        /// <summary>
        /// Retrieves (or creates) the validator (syntax checker) 
        /// for the document that is associated with the text buffer
        /// </summary>
        /// <param name="textBuffer">Text buffer</param>
        public static TreeValidator EnsureFromTextBuffer(ITextBuffer textBuffer, EditorTree editorTree)
        {
            TreeValidator validator = ServiceManager.GetService<TreeValidator>(textBuffer);
            if (validator == null)
            {
                validator = new TreeValidator(editorTree);
            }

            return validator;
        }

        /// <summary>
        /// Determines if background validation is currently in 
        /// progress (i.e. validation thread is running).
        /// </summary>
        public bool IsValidationInProgress
        {
            get { return _validationEnabled && _validationStarted; }
        }

        private void StartValidationNextIdle()
        {
            if (_validationEnabled)
            {
                AdviseToIdle();
            }
        }

        #region Idle time handler
        private void OnIdle(object sender, EventArgs e)
        {
            // Throttle validator idle a bit
            if (TimeUtility.MillisecondsSinceUTC(_idleRequestTime) > _validationDelay)
            {
                UnadviseFromIdle();
                StartValidation();
            }
        }

        private void AdviseToIdle()
        {
            if (!_advisedToIdleTime)
            {
                _idleRequestTime = DateTime.UtcNow;

                EditorShell.OnIdle += OnIdle;
                _advisedToIdleTime = true;
            }
        }

        private void UnadviseFromIdle()
        {
            if (_advisedToIdleTime)
            {
                EditorShell.OnIdle -= OnIdle;
                _advisedToIdleTime = false;
            }
        }

        #endregion

        #region Settings change handler
        private void OnSettingsChanged(object sender, EventArgs e)
        {
            bool validationWasEnabled = _validationEnabled;
            bool errorsAsWarnings = _errorsAsWarnings;

            _validationEnabled = RSettings.ValidationEnabled;

            if (validationWasEnabled && !_validationEnabled)
            {
                StopValidation();
            }
            else if (!validationWasEnabled && _validationEnabled)
            {
                StartValidation();
            }

            if (Cleared != null)
                Cleared(this, EventArgs.Empty);
        }
        #endregion

        private void StartValidation()
        {
            if (_validationEnabled && _editorTree != null)
            {
                _validationStarted = true;
                QueueTreeForValidation();
            }
        }

        private void StopValidation()
        {
            _validationStarted = false;

            //  Empty the results queue
            while(!ValidationResults.IsEmpty)
            {
                IValidationError error;
                ValidationResults.TryDequeue(out error);
            }

            UnadviseFromIdle();
        }

        #region Tree event handlers

        private void OnNodesRemoved(object sender, TreeNodesRemovedEventArgs e)
        {
            if (_validationEnabled)
            {
                foreach (var element in e.Nodes)
                {
                    ClearResultsForElement(element);
                }
            }
        }

        private void OnTreeUpdateCompleted(object sender, TreeUpdatedEventArgs e)
        {
            StopValidation();
            StartValidationNextIdle();
        }

        private void OnTreeClose(object sender, EventArgs e)
        {
            ServiceManager.RemoveService<TreeValidator>(_editorTree.TextBuffer);

            StopValidation();

            _editorTree.NodesRemoved -= OnNodesRemoved;
            _editorTree.UpdateCompleted -= OnTreeUpdateCompleted;
            _editorTree.Closing -= OnTreeClose;

            _editorTree = null;

            RSettings.Changed -= OnSettingsChanged;
        }
        #endregion

        private void ClearResultsForElement(IAstNode node)
        {
            foreach (var child in node.Children)
            {
                ClearResultsForElement(child);
            }

            // null for thne aggregatevalidator as we want it removed for all
            ValidationResults.Enqueue(new ValidationSentinel(new RToken(RTokenType.EndOfStream, TextRange.EmptyRange)));
        }

        private void QueueTreeForValidation()
        {
            // Transfer available errors from the tree right away
            foreach(ParseError e in _editorTree.AstRoot.Errors)
            {
                ValidationResults.Enqueue(new ValidationError(e.Token, e.ErrorType.ToString(), ValidationErrorLocation.Node));
            }
        }
    }
}
