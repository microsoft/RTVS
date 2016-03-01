using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendSmileCommand : SendMailCommand {
        public SendSmileCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendSmile) {
        }

        internal override void SetStatus() {
            Enabled = true;
        }

        internal override void Handle() {
            SendMail(":-)", "RTVS Smile", null);
        }
    }
}
