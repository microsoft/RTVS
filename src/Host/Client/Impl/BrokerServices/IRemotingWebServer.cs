using Microsoft.Common.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRemotingWebServer {
        Task<string> CreateWebServerAsync(string remoteUrl, string baseAddress, string name, IActionLog log, IConsole console, CancellationToken ct = default(CancellationToken));
    }
}
