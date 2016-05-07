// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.DataInspection {
    /// <summary>
    /// Describes the result of evaluating an expression that is an error. 
    /// </summary>
    public interface IRErrorInfo : IREvaluationResultInfo {
        string ErrorText { get; }
    }
}
