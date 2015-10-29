using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendFrownCommand : SendMailCommand {
        //TODO: localize
        private const string _disclaimer =
@"Please attach RTVSLogs.zip file that can be found in your user TEMP folder 
and briefly describe what you were doing that led to the issue if applicable. 
Please be aware that the data contained in the attached logs contain 
your command history as well as all output displayed in the R Interactive Window";

        public SendFrownCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendFrown) {
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {

            string zipPath = DiagnosticLogs.Collect();
            SendMail(_disclaimer, "RTVS Frown", zipPath);
        }
    }
}
