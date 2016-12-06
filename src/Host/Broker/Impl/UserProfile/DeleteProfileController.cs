using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker.UserProfile {
    [Authorize(Policy = Policies.RUser)]
    [Route("/deleteuser")]
    public class DeleteProfileController : Controller {
        private readonly SessionManager _sessionManager;

        public DeleteProfileController(SessionManager sessionManager) {
            _sessionManager = sessionManager;
        }

        [HttpGet]
        public Task<RUserProfileDeleteResponse> GetAsync() {
            _sessionManager.CloseAndBlockSessionsCreationForUser(User.Identity);
            // TODO: call userprofile service to delete profile
            _sessionManager.UnblockSessionCreationForUser(User.Identity);
            return Task.FromResult(new RUserProfileDeleteResponse());
        }
    }
}
