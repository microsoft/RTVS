using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public static class InteractiveWindowRSessionHelper {
        public static IRSession GetInteractiveWindowRSession(this IRSessionProvider provider) {
            return provider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, RHostClientApp.Instance);
        }
    }
}
