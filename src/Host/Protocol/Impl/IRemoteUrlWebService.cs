using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Protocol {
    public interface IRemoteUrlWebService {
        Task<IEnumerable<SessionInfo>> GetAsync();
        Task<SessionInfo> PutAsync(string id, SessionCreateRequest request);
    }
}
