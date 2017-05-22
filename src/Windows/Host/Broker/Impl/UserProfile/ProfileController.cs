// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Common.Core.OS;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.UserProfile {
    [Authorize(Policy = Policies.RUser)]
    [Route("/profile")]
    public class ProfileController : Controller {
        private readonly SessionManager _sessionManager;
        private readonly UserProfileManager _userProfileManager;

        public ProfileController(SessionManager sessionManager, UserProfileManager userProfileManager) {
            _sessionManager = sessionManager;
            _userProfileManager = userProfileManager;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync() {
            RUserProfileServiceResponse result = null;

            using (_sessionManager.BlockSessionsCreationForUser(User.Identity, true)) {
                var username = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
                var domain = new StringBuilder(NativeMethods.CREDUI_MAX_DOMAIN_LENGTH + 1);
                uint error = NativeMethods.CredUIParseUserName(User.Identity.Name, username, username.Capacity, domain, domain.Capacity);
                if (error != 0) {
                    return new ApiErrorResult(BrokerApiError.Win32Error, ErrorCodeConverter.MessageFromErrorCode((int)error));
                }

#if DEBUG
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
#else
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
#endif

                string sid = ((WindowsIdentity)User.Identity).User.Value;
                result = await _userProfileManager.DeleteProfileAsync(new RUserProfileServiceRequest(username.ToString(), domain.ToString(), sid), cts.Token);
            }

            if(result.Error == 0) {
                return Ok();
            }

            return new ApiErrorResult(BrokerApiError.Win32Error, ErrorCodeConverter.MessageFromErrorCode((int)result.Error));
        }
    }
}
