// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.InteractiveWorkflow;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    [ExcludeFromCodeCoverage]
    public sealed class InteractiveWorkflowStubFactory {
        public static IRInteractiveWorkflow CreateDefault() {
            return Substitute.For<IRInteractiveWorkflow>();
        }
    }
}
