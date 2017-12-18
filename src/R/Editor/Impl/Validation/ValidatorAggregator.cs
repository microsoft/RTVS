// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Validation.Errors;
using Microsoft.R.Editor.Validation.Lint;

namespace Microsoft.R.Editor.Validation {
    /// <summary>
    /// Aggregates multiple validators: internal and provided as services
    /// In VS validators may be exported via MEF for the "R" content type.
    /// </summary>
    internal sealed class ValidatorAggregator : IValidatorAggregator, IAstVisitor {
        private readonly IREditorSettings _settings;
        private readonly IEnumerable<IRDocumentValidator> _validators;
        private Task _validationTask;
        private TaskCompletionSource<bool> _tcs;

        public ValidatorAggregator(IServiceContainer services) {
            var services1 = services;
            _settings = services1.GetService<IREditorSettings>();

            // Populate known validators
            var list = new List<IRDocumentValidator>() { new LintValidator() };

            // Fetch externally provided ones
            var locator = services1.GetService<IContentTypeServiceLocator>();
            list.AddRange(locator.GetAllServices<IRDocumentValidator>("R"));

            _validators = list;
        }

        #region IValidatorAggregator
        /// <summary>
        /// Launches AST and file validation / syntax check.
        /// The process runs asyncronously.
        /// </summary>
        public Task RunAsync(AstRoot ast, bool projectedBuffer, bool linterEnabled, ConcurrentQueue<IValidationError> results, CancellationToken ct) {
            _tcs = new TaskCompletionSource<bool>();
            try {
                BeginValidation(_settings, projectedBuffer, linterEnabled);
                _validationTask = Task.Run(() => {
                    var outcome = Validate(ast, ct);
                    foreach (var o in outcome) {
                        results.Enqueue(o);
                    }
                }).ContinueWith(t => {
                    EndValidation();
                    _tcs.TrySetResult(true);
                });
            } catch (OperationCanceledException) {
                EndValidation();
                _tcs.TrySetCanceled();
            }

            return _tcs.Task;
        }

        public bool Busy => _validationTask != null;
        #endregion

        private IEnumerable<IValidationError> Validate(AstRoot ast, CancellationToken ct) {
            var context = new ValidationContext(ct);
            ast.Accept(this, context);
            foreach (var v in _validators) {
                context.CancellationToken.ThrowIfCancellationRequested();
                context.Errors.AddRange(v.ValidateWhitespace(ast.TextProvider));
            }
            return context.Errors;
        }

        #region IAstVisitor
        public bool Visit(IAstNode node, object parameter) {
            var context = (ValidationContext)parameter;
            foreach (var v in _validators) {
                if (context.CancellationToken.IsCancellationRequested) {
                    return false;
                }
                context.Errors.AddRange(v.ValidateElement(node));
            }
            return true;
        }
        #endregion

        private void BeginValidation(IREditorSettings settings, bool projectedBuffer, bool linterEnabled) {
            foreach (var v in _validators) {
                v.OnBeginValidation(settings, projectedBuffer, linterEnabled);
            }
        }

        private void EndValidation() {
            foreach (var v in _validators) {
                v.OnEndValidation();
            }
            _validationTask = null;
        }

        private class ValidationContext {
            public CancellationToken CancellationToken { get; }
            public List<IValidationError> Errors { get; }
            public ValidationContext(CancellationToken ct) {
                CancellationToken = ct;
                Errors = new List<IValidationError>();
            }
        }
    }
}
