using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class InterruptRCommand : PackageCommand {
        private readonly IRSessionProvider _rSessionProvider;

        public InterruptRCommand(IRSessionProvider rSessionProvider) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdInterruptR) {
            _rSessionProvider = rSessionProvider;
        }

        protected override void SetStatus() {
            Enabled = (_rSessionProvider.Current != null);
        }

        protected override void Handle() {
            _rSessionProvider.Current?.CancelAllAsync().DoNotWait();
        }
    }
}
