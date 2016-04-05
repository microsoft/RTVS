// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using static System.FormattableString;

namespace Microsoft.R.Debugger {
    public class DebugPromiseEvaluationResult : DebugEvaluationResult {
        public string Code { get; }

        public DebugPromiseEvaluationResult(DebugStackFrame stackFrame, string expression, string name, string code)
            : base(stackFrame, expression, name) {
            Code = code;
        }

        public override string ToString() {
            return Invariant($"PROMISE: {Code}");
        }
    }
}
