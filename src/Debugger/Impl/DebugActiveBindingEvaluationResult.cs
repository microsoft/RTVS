// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Debugger {
    public class DebugActiveBindingEvaluationResult : DebugEvaluationResult {
        internal DebugActiveBindingEvaluationResult(DebugSession session, string environmentExpression, string expression, string name)
            : base(session, environmentExpression, expression, name) {
        }
    }
}
