using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Host.Client.Test.Script {
    public sealed class RHostClientTestApp : IRHostClientApp {
        public string CranUrlFromName(string name) {
            return "https://cran.rstudio.com";
        }

        public Task Plot(string filePath, CancellationToken ct) {
            throw new NotImplementedException();
        }

        public Task ShowErrorMessage(string message) {
            return Task.CompletedTask;
        }

        public Task ShowHelp(string url) {
            return Task.CompletedTask;
        }

        public Task<MessageButtons> ShowMessage(string message, MessageButtons buttons) {
            return Task.FromResult(MessageButtons.OK);
        }
    }
}
