// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.DataInspection {
    /// <summary>
    /// Describes the result of evaluating an expression that references an active binding. 
    /// </summary>
    public interface IRActiveBindingInfo : IREvaluationResultInfo {
        /// <summary>
        /// This property is set to an instance of <see cref="RValueInfo"/> if <see cref="REvaluationResultProperties.ComputedValueProperty"/> 
        /// is used while evaluating active bindings. This is null otherwise.
        /// </summary>
        IRValueInfo ComputedValue { get; }
    }
}
