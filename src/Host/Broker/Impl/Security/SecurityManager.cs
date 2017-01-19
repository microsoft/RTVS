// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Common.Core.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.UserProfile;
using Microsoft.R.Host.Protocol;
using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.R.Host.Broker.Security {
    public class SecurityManager {
        private readonly SecurityOptions _options;
        private readonly ILogger _logger;
        private readonly UserProfileManager _userProfileManager;

        public SecurityManager(UserProfileManager userProfileManager, IOptions<SecurityOptions> options, ILogger<SecurityManager> logger) {
            _userProfileManager = userProfileManager;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SignInAsync(BasicSignInContext context) {
            ClaimsPrincipal principal;
            if (context.IsSignInRequired()) {
                principal = (_options.Secret != null) ? SignInUsingSecret(context) : await SignInUsingLogonAsync(context);
            } else {
                var claims = new[] { new Claim(ClaimTypes.Anonymous, "") };
                var claimsIdentity = new ClaimsIdentity(claims, context.Options.AuthenticationScheme);
                principal = new ClaimsPrincipal(claimsIdentity);
            }

            if (principal != null) {
                context.Ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), context.Options.AuthenticationScheme);
            }

            context.HandleResponse();
        }

        private ClaimsPrincipal SignInUsingSecret(BasicSignInContext context) {
            if (_options.Secret != context.Password) {
                return null;
            }

            var claims = new[] {
                new Claim(ClaimTypes.Name, context.Username),
                new Claim(Claims.RUser, "")
            };

            var identity = new ClaimsIdentity(claims, context.Options.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        private async Task<ClaimsPrincipal> SignInUsingLogonAsync(BasicSignInContext context) {
            var user = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
            var domain = new StringBuilder(NativeMethods.CREDUI_MAX_DOMAIN_LENGTH + 1);

            uint error = NativeMethods.CredUIParseUserName(context.Username, user, user.Capacity, domain, domain.Capacity);
            if (error != 0) {
                _logger.LogError(Resources.Error_UserNameParse, context.Username, error.ToString("X"));
                return null;
            }

            IntPtr token;
            WindowsIdentity winIdentity = null;

            string profilePath = string.Empty;
            _logger.LogTrace(Resources.Trace_LogOnUserBegin, context.Username);
            if (NativeMethods.LogonUser(user.ToString(), domain.ToString(), context.Password, (int)LogonType.LOGON32_LOGON_NETWORK, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, out token)) {
                _logger.LogTrace(Resources.Trace_LogOnSuccess, context.Username);
                winIdentity = new WindowsIdentity(token);

                StringBuilder profileDir = new StringBuilder(NativeMethods.MAX_PATH * 2);
                uint size = (uint)profileDir.Capacity;
                if (NativeMethods.GetUserProfileDirectory(token, profileDir, ref size)) {
                    profilePath = profileDir.ToString();
                    _logger.LogTrace(Resources.Trace_UserProfileDirectory, context.Username, profilePath);
                } else {
#if DEBUG
                    CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
#else
                    CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
#endif
                    _logger.LogTrace(Resources.Trace_UserProfileCreation, context.Username);

                    var result = await _userProfileManager.CreateProfileAsync(new RUserProfileServiceRequest(user.ToString(), domain.ToString(), winIdentity.User.Value), cts.Token);
                    if (result.IsInvalidResponse()) {
                        _logger.LogError(Resources.Error_ProfileCreationFailedInvalidResponse, context.Username, Resources.Info_UserProfileServiceName);
                        return null;
                    }

                    error = result.Error;
                    // 0x800700b7 - Profile already exists.
                    if (error != 0 && error != 0x800700b7) {
                        _logger.LogError(Resources.Error_ProfileCreationFailed, context.Username, error.ToString("X"));
                        return null;
                    } else if (error == 0x800700b7 || result.ProfileExists) {
                        _logger.LogInformation(Resources.Info_ProfileAlreadyExists, context.Username);
                    } else {
                        _logger.LogInformation(Resources.Info_ProfileCreated, context.Username);
                    }

                    if (!string.IsNullOrEmpty(result.ProfilePath)) {
                        profilePath = result.ProfilePath;
                        _logger.LogTrace(Resources.Trace_UserProfileDirectory, context.Username, profilePath);
                    } else {
                        if (NativeMethods.GetUserProfileDirectory(token, profileDir, ref size)) {
                            profilePath = profileDir.ToString();
                            _logger.LogTrace(Resources.Trace_UserProfileDirectory, context.Username, profilePath);
                        } else {
                            _logger.LogError(Resources.Error_GetUserProfileDirectory, context.Username, Marshal.GetLastWin32Error().ToString("X"));
                        }
                    }
                }
            } else {
                _logger.LogError(Resources.Error_LogOnFailed, context.Username, Marshal.GetLastWin32Error().ToString("X"));
                return null;
            }

            var principal = new WindowsPrincipal(winIdentity);
            if (principal.IsInRole(_options.AllowedGroup)) {
                var claims = new[] {
                    //new Claim(ClaimTypes.Name, context.Username),
                    new Claim(Claims.RUser, ""),
                    new Claim(Claims.RUserProfileDir, profilePath)
                };

                var claimsIdentity = new ClaimsIdentity(claims, context.Options.AuthenticationScheme);
                principal.AddIdentities(new[] { claimsIdentity });
            }

            return principal;
        }
    }
}
