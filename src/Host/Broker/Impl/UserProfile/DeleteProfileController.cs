using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Common.Core;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker.UserProfile {
    [Authorize(Policy = Policies.RUser)]
    [Route("/deleteuser")]
    public class DeleteProfileController : Controller {
        private readonly SessionManager _sessionManager;
        private readonly UserProfileManager _userProfileManager;

        public DeleteProfileController(SessionManager sessionManager, UserProfileManager userProfileManager) {
            _sessionManager = sessionManager;
            _userProfileManager = userProfileManager;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync() {
            RUserProfileServiceResponse result = null;
            try {
                _sessionManager.CloseAndBlockSessionsCreationForUser(User.Identity);

                var username = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
                var domain = new StringBuilder(NativeMethods.CREDUI_MAX_PASSWORD_LENGTH + 1);
                uint error = NativeMethods.CredUIParseUserName(User.Identity.Name, username, username.Capacity, domain, domain.Capacity);
                if (error != 0) {
                    throw new ArgumentException(Resources.Error_UserNameParse.FormatInvariant(User.Identity.Name, error));
                }

#if DEBUG
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
#else
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
#endif

                string password = User.FindFirst(Claims.Password)?.Value;
                if (string.IsNullOrWhiteSpace(password)) {
                    return NotFound();
                }

                result = await _userProfileManager.DeleteProfileAsync(RUserProfileServiceRequest.Create(username.ToString(), domain.ToString(), password), cts.Token);
            } finally {
                _sessionManager.UnblockSessionCreationForUser(User.Identity);
            }

            if(result.Error == 0) {
                return Ok();
            }

            return BadRequest(result.Error.ToString());
        }
    }
}
