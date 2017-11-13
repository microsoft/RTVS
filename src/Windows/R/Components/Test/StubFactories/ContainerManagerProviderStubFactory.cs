// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.Containers;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    [ExcludeFromCodeCoverage]
    public sealed class ContainerManagerProviderStubFactory {
        public static IContainerManagerProvider CreateDefault() {
            return Substitute.For<IContainerManagerProvider>();
        }
    }
}