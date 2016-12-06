using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileDeletor {
        public static async Task DeleteProfileAsync(int serverTimeOutms = 0, int clientTimeOutms = 0, IUserProfileServices userProfileService = null, CancellationToken ct = default(CancellationToken), ILogger logger = null) {

        }
    }
}
