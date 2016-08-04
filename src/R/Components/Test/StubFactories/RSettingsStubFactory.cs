// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.Test.Stubs;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Components.Test.StubFactories {
    [ExcludeFromCodeCoverage]
    public sealed class RSettingsStubFactory {
        public static RSettingsStub CreateForExistingRPath() {
            return new RSettingsStub {
                RBasePath = new RInstallation().GetRInstallPath()
            };
        }
    }
}
