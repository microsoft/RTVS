// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Utility;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Validation.Errors;

namespace Microsoft.R.Editor.Validation {
    /// <summary>
    /// Main R validator: performs syntax check, produces results that
    /// then sent to the task list and tagger that creates squiggles.
    /// Works asynchronously except the final part that pushes actual 
    /// task items to the IDE. Additional validators can be added
    /// via MEF exports.
    /// 
    /// Validator listens to tree updated events and schedules validation
    /// passes for next idle slot. Next idle validation thread starts
    /// and performas its work. Validator also listens to changes in
    /// settings which may turn validation on or off.
    /// </summary>
    public sealed class TreeValidator {
        private static readonly BooleanSwitch _traceValidation = new BooleanSwitch("traceRValidation", "Trace R validation events in debug window.");
        private static int _validationDelay = 200;

        public BooleanSwitch TraceValidation => _traceValidation;

        /// <summary>
        /// Queue of validation results. Typically accessed from the main 
        /// thread that pushes errors/warning into a task list window. 
        /// Code that places items on the task list should be checking if 
        /// node that produced the error is still exist in the document.
        /// </summary>
        internal ConcurrentQueue<IValidationError> ValidationResults { get; }

        private IREditorTree _editorTree;
        private readonly ICoreShell _shell;
        private readonly IREditorSettings _settings;
        private bool _syntaxCheckEnabled = false;
        private bool _validationStarted = false;

        private bool _advisedToIdleTime = false;
        private DateTime _idleRequestTime = DateTime.UtcNow;

        /// <summary>
        /// Fires when validator is cleared. Typically when validation was switched off
        /// so task list and all error tags must be removed from the editor.
        /// </summary>
        public event EventHandler<EventArgs> Cleared;

        #region Constructors
        public TreeValidator(IREditorTree editorTree, IREditorSettings settings) {
#if DEBUG
            TraceValidation.Enabled = false;
#endif

            _editorTree = editorTree;
            _editorTree.NodesRemoved += OnNodesRemoved;
            _editorTree.UpdateCompleted += OnTreeUpdateCompleted;
            _editorTree.Closing += OnTreeClose;

            _settings = settings;
            // Advise to settings changed *after* accessing the RSettings, 
            // since accessing the host application (VS) settings object may 
            // cause it fire Changed notification in some cases.
            _settings.SettingsChanged += OnSettingsChanged;
            _syntaxCheckEnabled = IsSyntaxCheckEnabled(_editorTree.EditorBuffer);

            // We don't want to start validation right away since it may 
            // interfere with the editor perceived startup performance.

            StartValidationNextIdle();
            ValidationResults = new ConcurrentQueue<IValidationError>();

            editorTree.EditorBuffer.Services.AddService(this);
        }
        #endregion

        /// <summary>
        /// Retrieves (or creates) the validator (syntax checker) 
        /// for the document that is associated with the text buffer
        /// </summary>
        /// <param name="textBuffer">Text buffer</param>
        /// <param name="editorTree"></param>
        /// <param name="shell"></param>
        public static TreeValidator EnsureFromEditorBuffer(IEditorBuffer editorBuffer, IREditorTree editorTree, IREditorSettings settings)
            => editorBuffer.Services.GetService<TreeValidator>() ?? new TreeValidator(editorTree, settings);

        /// <summary>
        /// Determines if background validation is currently in 
        /// progress (i.e. validation thread is running).
        /// </summary>
        public bool IsValidationInProgress {
            get { return _syntaxCheckEnabled && _validationStarted; }
        }

        private void StartValidationNextIdle() {
            if (_syntaxCheckEnabled) {
                AdviseToIdle();
            }
        }

        #region Idle time handler
        private void OnIdle(object sender, EventArgs e) {
            // Throttle validator idle a bit
            if (TimeUtility.MillisecondsSinceUtc(_idleRequestTime) > _validationDelay) {
                UnadviseFromIdle();
                StartValidation();
            }
        }

        private void AdviseToIdle() {
            if (!_advisedToIdleTime) {
                _idleRequestTime = DateTime.UtcNow;

                _shell.Idle += OnIdle;
                _advisedToIdleTime = true;
            }
        }

        private void UnadviseFromIdle() {
            if (_advisedToIdleTime) {
                _shell.Idle -= OnIdle;
                _advisedToIdleTime = false;
            }
        }

        #endregion

        #region Settings change handler
        private void OnSettingsChanged(object sender, EventArgs e) {
            bool syntaxCheckWasEnabled = _syntaxCheckEnabled;

            _syntaxCheckEnabled = IsSyntaxCheckEnabled(_editorTree.EditorBuffer);

            if (syntaxCheckWasEnabled && !_syntaxCheckEnabled) {
                StopValidation();
            } else if (!syntaxCheckWasEnabled && _syntaxCheckEnabled) {
                StartValidation();
            }

            Cleared?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        //public static bool IsSyntaxCheckEnabled(IEditorBuffer editorBuffer) {
        //    var document = editorBuffer.GetDocument<IREditorDocument>();
        //    if (document != null) {
        //        var view = document.GetFirstView();
        //        var settings = editorBuffer.GetService<IREditorSettings>();
        //        return view != null && view.IsRepl() ? settings.SyntaxCheckInRepl : settings.SyntaxCheck;
        //    }
        //    return false;
        //}

        private void StartValidation() {
            if (_syntaxCheckEnabled && _editorTree != null) {
                _validationStarted = true;
                QueueTreeForValidation();
            }
        }

        private void StopValidation() {
            _validationStarted = false;

            //  Empty the results queue
            while (!ValidationResults.IsEmpty) {
                IValidationError error;
                ValidationResults.TryDequeue(out error);
            }

            UnadviseFromIdle();
        }

        #region Tree event handlers

        /// <summary>
        /// Listens to 'nodes removed' event which fires when user
        /// deletes text that generated AST nodes or pastes over
        /// new content. This allows validator to remove related
        /// errors from the task list quickly so they don't linger
        /// until the next validation pass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNodesRemoved(object sender, TreeNodesRemovedEventArgs e) {
            if (_syntaxCheckEnabled) {
                foreach (var node in e.Nodes) {
                    ClearResultsForNode(node);
                }
            }
        }

        private void OnTreeUpdateCompleted(object sender, TreeUpdatedEventArgs e) {
            if (e.UpdateType == TreeUpdateType.NewTree) {
                StopValidation();
                StartValidationNextIdle();
            }
        }

        private void OnTreeClose(object sender, EventArgs e) {
            _editorTree.EditorBuffer.Services.RemoveService<TreeValidator>();

            StopValidation();

            _editorTree.NodesRemoved -= OnNodesRemoved;
            _editorTree.UpdateCompleted -= OnTreeUpdateCompleted;
            _editorTree.Closing -= OnTreeClose;

            _editorTree = null;

            REditorSettings.Changed -= OnSettingsChanged;
        }
        #endregion

        private void ClearResultsForNode(IAstNode node) {
            foreach (var child in node.Children) {
                ClearResultsForNode(child);
            }

            // Adding sentinel will cause task list handler
            // to remove all results from the task list 
            ValidationResults.Enqueue(new ValidationSentinel());
        }

        private void QueueTreeForValidation() {
            // Transfer available errors from the tree right away
            foreach (ParseError e in _editorTree.AstRoot.Errors) {
                ValidationResults.Enqueue(new ValidationError(e, ErrorText.GetText(e.ErrorType), e.Location));
            }
        }
    }
}
