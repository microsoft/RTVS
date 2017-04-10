// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if _NOT_USED_YET_
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation
{
    /// <summary>
    /// Validator manager is an aggregator which manages individual validators 
    /// imported via MEF. Class instance is created per document instance, 
    /// so each document has its own set of validators.
    /// </summary>
    internal sealed class ValidatorManager
    {
        /// <summary>
        /// List of validator providers imported via MEF
        /// </summary>
        List<IValidatorProvider> _validatorProviders = new List<IValidatorProvider>();

        /// <summary>
        /// List of validator instances. 
        /// </summary>
        private List<IValidator> _validators;

        public ValidatorManager()
        {
            shell.Current.CompositionService.SatisfyImportsOnce(this);

            // Import validator providers
            var validatorImportComposer = new ContentTypeImportComposer<IValidatorProvider>(shell.Current.CompositionService);
            var validatorProviderImports = validatorImportComposer.GetAllLazy(RContentTypeDefinition.ContentType);

            foreach (var import in validatorProviderImports)
            {
                _validatorProviders.Add(import.Value);
            }
        }

        private void EnsureValidators()
        {
            // Create actual validator instances if we haven't done it yet.
            if (_validators == null)
            {
                _validators = new List<IValidator>();

                foreach (var itemValidatorProvider in _validatorProviders)
                {
                    var validator = itemValidatorProvider.GetValidator();
                    _validators.Add(validator);
                }
            }
        }

        public void OnBeginValidation()
        {
            EnsureValidators();

            // Tell validators the work is about to begin.
            foreach (var validator in _validators)
            {
                validator.OnBeginValidation();
            }
        }

        public void OnEndValidation()
        {
            foreach (var validator in _validators)
            {
                validator.OnEndValidation();
            }
        }

        public void OnBeginQueuing()
        {
            EnsureValidators();
        }

        /// <summary>
        /// Main element validation worker.
        /// </summary>
        /// <param name="workItem">Element to validate</returns>
        public IEnumerable<IValidationError> Validate(IAstNode node)
        {
            // Combined results from all validators
            var combinedResults = new List<IValidationError>();

            foreach (var validator in _validators)
            {
                var itemResults = validator.ValidateElement(node);
                combinedResults.AddRange(itemResults);
            }

            return combinedResults;
        }
    }
}
#endif
