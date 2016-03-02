// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.History;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    public sealed class RHistoryProviderStubFactory {
        public static IRHistoryProvider CreateDefault() {
            return Substitute.For<IRHistoryProvider>();
        }
    }
}