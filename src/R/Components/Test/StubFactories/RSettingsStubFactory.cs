// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Components.Test.Stubs;

namespace Microsoft.R.Components.Test.StubFactories {
    public sealed class RSettingsStubFactory {
        public static RSettingsStub CreateForExistingRPath() {
            return new RSettingsStub {
                RBasePath = RUtilities.FindExistingRBasePath()
            };
        }
    }
}
