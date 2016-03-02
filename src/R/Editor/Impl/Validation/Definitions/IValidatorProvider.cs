// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Editor.Validation.Definitions
{
    /// <summary>
    /// Inmplemented by a provider of AST node validator. 
    /// Exported via MEF for a particular content type.
    /// </summary>
    public interface IValidatorProvider
    {
        /// <summary>
        /// Creates HTML element validator
        /// </summary>
        /// <returns></returns>
        IValidator GetValidator();
    }
}
