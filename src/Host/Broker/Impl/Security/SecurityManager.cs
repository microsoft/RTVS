// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.R.Host.Broker.Security {
    public class SecurityManager {
        private readonly SecurityOptions _options;
        private readonly ILogger _logger;

        public SecurityManager(IOptions<SecurityOptions> options, ILogger<SecurityManager> logger) {
            _options = options.Value;
            _logger = logger;
        }

        public Task SignInAsync(BasicSignInContext context) {
            ClaimsPrincipal principal = (_options.Secret != null) ? SignInUsingSecret(context) : SignInUsingLogon(context);
            if (principal != null) {
                context.Ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), context.Options.AuthenticationScheme);
            }

            context.HandleResponse();
            return Task.CompletedTask;
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

        private ClaimsPrincipal SignInUsingLogon(BasicSignInContext context) {
            var user = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
            var domain = new StringBuilder(NativeMethods.CREDUI_MAX_PASSWORD_LENGTH + 1);

            if (NativeMethods.CredUIParseUserName(context.Username, user, user.Capacity, domain, domain.Capacity) != 0) {
                return null;
            }

            IntPtr token;
            WindowsIdentity winIdentity = null;
            string profilePath = "";

            _logger.LogInformation($"Attempting login for user: {context.Username}");
            if (NativeMethods.LogonUser(user.ToString(), domain.ToString(), context.Password, (int)LogonType.LOGON32_LOGON_NETWORK, (int)LogonProvider.LOGON32_PROVIDER_DEFAULT, out token)) {
                _logger.LogInformation($"Login succeeded for user: {context.Username}");
                winIdentity = new WindowsIdentity(token);
                StringBuilder profileDir = new StringBuilder(NativeMethods.MAX_PATH);
                uint size = (uint)profileDir.Capacity;

                _logger.LogInformation($"Attempting profile creation for user: {context.Username}");
                uint error = NativeMethods.CreateProfile(winIdentity.User.Value, user.ToString(), profileDir, size);
                // 0x800700b7 - Profile already exists.
                if (error != 0 && error != 0x800700b7) {
                    _logger.LogError($"Profile creation failed for user [{context.Username}] with error: {error.ToString("X")}");
                    return null;
                } else if (error == 0x800700b7) {
                    _logger.LogInformation($"Profile already exists for user: {context.Username}. Continuing with existing profile.");
                } else {
                    _logger.LogInformation($"Profile successfully created for user: {context.Username}");
                }

                profileDir = new StringBuilder(NativeMethods.MAX_PATH * 2);
                size = (uint)profileDir.Capacity;

                if (NativeMethods.GetUserProfileDirectory(token, profileDir, ref size)) {
                    profilePath = profileDir.ToString();
                } else {
                    _logger.LogError($"Failed to get profile for user: [{context.Username}] with error: {NativeMethods.GetLastError().ToString("X")}");
                }

            } else {
                _logger.LogError($"Login FAILED for user [{context.Username}] with error: {NativeMethods.GetLastError().ToString("X")}");
                return null;
            }

            var principal = new WindowsPrincipal(winIdentity);
            if (principal.IsInRole(_options.AllowedGroup)) {
                var claims = new[] {
                    //new Claim(ClaimTypes.Name, context.Username),
                    new Claim(Claims.RUser, ""),
                    // TODO: figure out how to avoid keeping raw credentials around. 
                    new Claim(Claims.Password, context.Password),
                    new Claim(Claims.RUserProfileDir, profilePath)
                };

                var claimsIdentity = new ClaimsIdentity(claims, context.Options.AuthenticationScheme);
                principal.AddIdentities(new[] { claimsIdentity });
            }

            return principal;
        }
    }
}
