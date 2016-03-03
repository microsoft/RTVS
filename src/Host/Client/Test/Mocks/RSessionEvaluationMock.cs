// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Test.Mocks {
    public sealed class RSessionEvaluationMock : IRSessionEvaluation {
        public IReadOnlyList<IRContext> Contexts {
            get {
                return new List<IRContext>() { new RContextMock(), new RContextMock() };
            }
        }

        public void Dispose() {
        }

        public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind = REvaluationKind.Normal) {
            return Task.FromResult(new REvaluationResult());
        }
    }
}
