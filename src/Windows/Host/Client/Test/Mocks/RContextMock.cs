// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client.Test.Mocks {
    public sealed class RContextMock : IRContext {
        public RContextType CallFlag { get; set; } = RContextType.TopLevel;
    }
}
