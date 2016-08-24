// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.Plots;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    [ExcludeFromCodeCoverage]
    public sealed class RPlotManagerProviderStubFactory {
        public static IRPlotManagerProvider CreateDefault() {
            return Substitute.For<IRPlotManagerProvider>();
        }
    }
}