// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Utility;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
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
        private static int _validationDelay = 300;

        public BooleanSwitch TraceValidation => _traceValidation;

        /// <summary>
        /// Queue of validation results. Typically accessed from the main 
        /// thread that pushes errors/warning into a task list window. 
        /// Code that places items on the task list should be checking if 
        /// node that produced the error is still exist in the document.
        /// </summary>
        public ConcurrentQueue<IValidationError> ValidationResults { get; }

        private readonly IREditorTree _editorTree;
        private readonly IREditorSettings _settings;
        private readonly IIdleTimeService _idleTime;
        private readonly ValidatorAggregator _aggregator;

        private CancellationTokenSource _cts;
        private bool _syntaxCheckEnabled;
        private bool _advisedToIdleTime;
        private DateTime _idleRequestTime = DateTime.UtcNow;

        /// <summary>
        /// Fires when validator is cleared. Typically when validation was switched off
        /// so task list and all error tags must be removed from the editor.
        /// </summary>
        public event EventHandler<EventArgs> Cleared;

        #region Constructors
        public TreeValidator(IREditorTree editorTree, IServiceContainer services) {
#if DEBUG
            TraceValidation.Enabled = false;
#endif

            _editorTree = editorTree;
            _editorTree.NodesRemoved += OnNodesRemoved;
            _editorTree.UpdateCompleted += OnTreeUpdateCompleted;
            _editorTree.Closing += OnTreeClose;

            _settings = services.GetService<IREditorSettings>();
            _idleTime = services.GetService<IIdleTimeService>();

            // Advise to settings changed *after* accessing the RSettings, 
            // since accessing the host application (VS) settings object may 
            // cause it fire Changed notification in some cases.
            _settings.SettingsChanged += OnSettingsChanged;
            _syntaxCheckEnabled = IsSyntaxCheckEnabled(_editorTree.EditorBuffer, _settings);

            // We don't want to start validation right away since it may 
            // interfere with the editor perceived startup performance.

            StartValidationNextIdle();
            ValidationResults = new ConcurrentQueue<IValidationError>();

            editorTree.EditorBuffer.AddService(this);
            _aggregator = new ValidatorAggregator(services);
        }
        #endregion

        /// <summary>
        /// Determines if background validation is currently in 
        /// progress (i.e. validation thread is running).
        /// </summary>
        public bool IsValidationInProgress => _syntaxCheckEnabled && _aggregator.Busy;

        private void StartValidationNextIdle() {
            if (_syntaxCheckEnabled) {
                AdviseToIdle();
            }
        }

        #region Idle time handler
        private void OnIdle(object sender, EventArgs e) {
            // Throttle validator idle a bit and check if previous 
            // task is still running. If so, try next idle.
            if (TimeUtility.MillisecondsSinceUtc(_idleRequestTime) > _validationDelay && !_aggregator.Busy) {
                UnadviseFromIdle();
                StartValidation();
            }
        }

        private void AdviseToIdle() {
            if (!_advisedToIdleTime) {
                _idleRequestTime = DateTime.UtcNow;
                _idleTime.Idle += OnIdle;
                _advisedToIdleTime = true;
            }
        }

        private void UnadviseFromIdle() {
            if (_advisedToIdleTime) {
                _idleTime.Idle -= OnIdle;
                _advisedToIdleTime = false;
            }
        }
        #endregion

        #region Settings change handler
        private void OnSettingsChanged(object sender, EventArgs e) {
            bool syntaxCheckWasEnabled = _syntaxCheckEnabled;

            _syntaxCheckEnabled = IsSyntaxCheckEnabled(_editorTree.EditorBuffer, _settings);
            if (syntaxCheckWasEnabled && !_syntaxCheckEnabled) {
                StopValidation();
            } else if (!syntaxCheckWasEnabled && _syntaxCheckEnabled) {
                StartValidation();
            }

            Cleared?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public static bool IsSyntaxCheckEnabled(IEditorBuffer editorBuffer, IREditorSettings settings) {
            var document = editorBuffer.GetEditorDocument<IREditorDocument>();
            if (document != null) {
                var view = document.PrimaryView;
                return view != null && view.IsRepl() ? settings.SyntaxCheckInRepl : settings.SyntaxCheckEnabled;
            }
            return false;
        }

        private void StartValidation() {
            if (_syntaxCheckEnabled && _editorTree != null) {
                // Transfer available errors from the tree right away
                foreach (var e in _editorTree.AstRoot.Errors) {
                    ValidationResults.Enqueue(new ValidationError(e, ErrorText.GetText(e.ErrorType), e.Location));
                }
                // Run all validators
                _cts = new CancellationTokenSource();
                _aggregator.RunAsync(_editorTree.AstRoot, ValidationResults, _cts.Token).DoNotWait();
            }
        }

        private void StopValidation() {
            _cts.Cancel();

            //  Empty the results queue
            while (!ValidationResults.IsEmpty) {
                ValidationResults.TryDequeue(out IValidationError error);
            }
            ClearResults();
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
                ClearResults();
            }
        }

        private void OnTreeUpdateCompleted(object sender, TreeUpdatedEventArgs e) {
            // We run validation on all updates since there 
            // may be whitespace checkers like lint
            StopValidation();
            StartValidationNextIdle();
        }

        private void OnTreeClose(object sender, EventArgs e) {
            _editorTree.EditorBuffer.RemoveService(this);

            StopValidation();

            _editorTree.NodesRemoved -= OnNodesRemoved;
            _editorTree.UpdateCompleted -= OnTreeUpdateCompleted;
            _editorTree.Closing -= OnTreeClose;

            _settings.SettingsChanged -= OnSettingsChanged;
            _idleTime.Idle -= OnIdle;
        }
        #endregion

        private void ClearResults() {
            // Adding sentinel will cause task list handler
            // to remove all results from the task list 
            ValidationResults.Enqueue(new ValidationSentinel());
        }
    }
}
