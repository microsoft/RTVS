// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Validation.Errors;
using Microsoft.R.Editor.Validation.Lint;

namespace Microsoft.R.Editor.Validation {
    internal sealed class ValidatorAggregator : IAstVisitor {
        private readonly IServiceContainer _services;
        private readonly IREditorSettings _settings;
        private readonly IEnumerable<IRDocumentValidator> _validators;

        public ValidatorAggregator(IServiceContainer services) {
            _services = services;
            _settings = _services.GetService<IREditorSettings>();

            var list = new List<IRDocumentValidator>() { new LintValidator() };
            var locator = _services.GetService<IContentTypeServiceLocator>();
            list.AddRange(locator.GetAllServices<IRDocumentValidator>("R"));
            _validators = list;
        }

        public void Run(AstRoot ast, ConcurrentQueue<IValidationError> results, CancellationToken ct) {
            BeginValidation(_settings);
            try {
                var outcome = Validate(ast, ct);
                foreach (var o in outcome) {
                    results.Enqueue(o);
                }
            } catch (OperationCanceledException) { } finally {
                EndValidation();
            }
        }

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

        private IEnumerable<IValidationError> Validate(AstRoot ast, CancellationToken ct) {
            var context = new ValidationContext(ct);
            ast.Accept(this, context);
            if (!ct.IsCancellationRequested) {
                foreach (var v in _validators) {
                    if (ct.IsCancellationRequested) {
                        break;
                    }
                    context.Errors.AddRange(v.ValidateWhitespace(ast.TextProvider));
                }
            }
            return context.Errors;
        }

        private void BeginValidation(IREditorSettings settings) {
            foreach (var v in _validators) {
                v.OnBeginValidation(settings);
            }
        }

        private void EndValidation() {
            foreach (var v in _validators) {
                v.OnEndValidation();
            }
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
