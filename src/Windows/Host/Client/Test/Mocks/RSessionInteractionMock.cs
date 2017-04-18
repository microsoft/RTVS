// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Test.Mocks {
    public sealed class RSessionInteractionMock : IRSessionInteraction {
        public IReadOnlyList<IRContext> Contexts {
            get {
                return new List<IRContext>() { new RContextMock() };
            }
        }

        public int MaxLength => 4096;

        public string Prompt => ">";

        public bool IsEvaluationAllowed => true;

        public void Dispose() {
        }

        public Task RespondAsync(string messageText) {
            return Task.CompletedTask;
        }
    }
}
