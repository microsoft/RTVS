// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.PackageManager;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    [ExcludeFromCodeCoverage]
    public sealed class RPackageManagerProviderStubFactory {
        public static IRPackageManagerProvider CreateDefault() {
            return Substitute.For<IRPackageManagerProvider>();
        }
    }
}