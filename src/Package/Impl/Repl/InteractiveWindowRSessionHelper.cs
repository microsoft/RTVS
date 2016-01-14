using System;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public static class InteractiveWindowRSessionHelper {
        public static Guid InteractiveWindowRSessionGuid = new Guid("77E2BCD9-BEED-47EF-B51E-2B892260ECA7");

        public static IRSession GetInteractiveWindowRSession(this IRSessionProvider provider) {
            return provider.GetOrCreate(InteractiveWindowRSessionGuid, RHostClientApp.Instance);
        }
    }
}
