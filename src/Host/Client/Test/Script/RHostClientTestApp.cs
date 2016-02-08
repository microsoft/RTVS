using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client.Test.Script {
    public class RHostClientTestApp : IRHostClientApp {
        public virtual string CranUrlFromName(string name) {
            return "https://cran.rstudio.com";
        }

        public virtual Task Plot(string filePath, CancellationToken ct) {
            throw new NotImplementedException();
        }

        public virtual Task ShowErrorMessage(string message) {
            return Task.CompletedTask;
        }

        public virtual Task ShowHelp(string url) {
            return Task.CompletedTask;
        }

        public virtual Task<MessageButtons> ShowMessage(string message, MessageButtons buttons) {
            return Task.FromResult(MessageButtons.OK);
        }
    }
}
