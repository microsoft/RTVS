// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.UserProfile;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Services {
    public class WindowsAuthenticationService : IAuthenticationService {
        private readonly SecurityOptions _options;
        private readonly ILogger<IAuthenticationService> _logger;
        private readonly UserProfileManager _userProfileManager;

        public WindowsAuthenticationService(UserProfileManager userProfileManager, IOptions<SecurityOptions> options, ILogger<IAuthenticationService> logger) {
            _userProfileManager = userProfileManager;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> SignInAsync(string username, string password, string authenticationScheme) {
            var user = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
            var domain = new StringBuilder(NativeMethods.CREDUI_MAX_DOMAIN_LENGTH + 1);

            uint error = NativeMethods.CredUIParseUserName(username, user, user.Capacity, domain, domain.Capacity);
            if (error != 0) {
                _logger.LogError(Resources.Error_UserNameParse, username, error.ToString("X"));
                return null;
            }

            IntPtr token;
            WindowsIdentity winIdentity = null;

            string profilePath = string.Empty;
            _logger.LogTrace(Resources.Trace_LogOnUserBegin, username);
            if (NativeMethods.LogonUser(user.ToString(), domain.ToString(), password, (int)LogonType.LOGON32_LOGON_NETWORK, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, out token)) {
                _logger.LogTrace(Resources.Trace_LogOnSuccess, username);
                winIdentity = new WindowsIdentity(token);

                StringBuilder profileDir = new StringBuilder(NativeMethods.MAX_PATH * 2);
                uint size = (uint)profileDir.Capacity;
                if (NativeMethods.GetUserProfileDirectory(token, profileDir, ref size)) {
                    profilePath = profileDir.ToString();
                    _logger.LogTrace(Resources.Trace_UserProfileDirectory, username, profilePath);
                } else {
#if DEBUG
                    CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
#else
                    CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
#endif
                    _logger.LogTrace(Resources.Trace_UserProfileCreation, username);

                    var result = await _userProfileManager.CreateProfileAsync(new RUserProfileServiceRequest(user.ToString(), domain.ToString(), winIdentity.User.Value), cts.Token);
                    if (result.IsInvalidResponse()) {
                        _logger.LogError(Resources.Error_ProfileCreationFailedInvalidResponse, username, Resources.Info_UserProfileServiceName);
                        return null;
                    }

                    error = result.Error;
                    // 0x800700b7 - Profile already exists.
                    if (error != 0 && error != 0x800700b7) {
                        _logger.LogError(Resources.Error_ProfileCreationFailed, username, error.ToString("X"));
                        return null;
                    } else if (error == 0x800700b7 || result.ProfileExists) {
                        _logger.LogInformation(Resources.Info_ProfileAlreadyExists, username);
                    } else {
                        _logger.LogInformation(Resources.Info_ProfileCreated, username);
                    }

                    if (!string.IsNullOrEmpty(result.ProfilePath)) {
                        profilePath = result.ProfilePath;
                        _logger.LogTrace(Resources.Trace_UserProfileDirectory, username, profilePath);
                    } else {
                        if (NativeMethods.GetUserProfileDirectory(token, profileDir, ref size)) {
                            profilePath = profileDir.ToString();
                            _logger.LogTrace(Resources.Trace_UserProfileDirectory, username, profilePath);
                        } else {
                            _logger.LogError(Resources.Error_GetUserProfileDirectory, username, Marshal.GetLastWin32Error().ToString("X"));
                        }
                    }
                }
            } else {
                _logger.LogError(Resources.Error_LogOnFailed, username, Marshal.GetLastWin32Error().ToString("X"));
                return null;
            }

            var principal = new WindowsPrincipal(winIdentity);
            if (principal.IsInRole(_options.AllowedGroup)) {
                var claims = new[] {
                    //new Claim(ClaimTypes.Name, username),
                    new Claim(Claims.RUser, ""),
                    new Claim(Claims.RUserProfileDir, profilePath)
                };

                var claimsIdentity = new ClaimsIdentity(claims, authenticationScheme);
                principal.AddIdentities(new[] { claimsIdentity });
            }

            return principal;
        }
    }
}
