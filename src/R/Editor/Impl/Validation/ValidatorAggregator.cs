// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
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
        private readonly IServiceContainer _services;
        private readonly IREditorSettings _settings;
        private readonly IEnumerable<IRDocumentValidator> _validators;
        private Task _validationTask;

        public ValidatorAggregator(IServiceContainer services) {
            _services = services;
            _settings = _services.GetService<IREditorSettings>();

            // Populate known validators
            var list = new List<IRDocumentValidator>() { new LintValidator() };

            // Fetch externally provided ones
            var locator = _services.GetService<IContentTypeServiceLocator>();
            list.AddRange(locator.GetAllServices<IRDocumentValidator>("R"));

            _validators = list;
        }

        #region IValidatorAggregator
        /// <summary>
        /// Launches AST and file validation / syntax check.
        /// The process runs asyncronously.
        /// </summary>
        public void Run(AstRoot ast, ConcurrentQueue<IValidationError> results, CancellationToken ct) {
            // Begin/End validation mush happen on main thread so validators
            // can acquire necessary resources and services up front and release
            // then appropriately when done.
            _services.MainThread().Assert();
            try {
                BeginValidation(_settings);
                _validationTask = Task.Run(() => {
                    var outcome = Validate(ast, ct);
                    foreach (var o in outcome) {
                        results.Enqueue(o);
                    }
                }).ContinueWith(async t => {
                    await _services.MainThread().SwitchToAsync();
                    EndValidation();
                });
            } catch (OperationCanceledException) {
                EndValidation();
            }
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
                context.CancellationToken.ThrowIfCancellationRequested();
                context.Errors.AddRange(v.ValidateElement(node));
            }
            return true;
        }
        #endregion

        private void BeginValidation(IREditorSettings settings) {
            foreach (var v in _validators) {
                v.OnBeginValidation(settings);
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
