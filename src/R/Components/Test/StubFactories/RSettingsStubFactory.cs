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
