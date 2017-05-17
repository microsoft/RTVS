// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Broker.Startup;

namespace Microsoft.R.Host.Broker.Services {
    public class WindowsRHostProcessService : IRHostProcessService {
        private readonly ILogger<Session> _sessionLogger;
        private const string RHostExe = "Microsoft.R.Host.exe";

        public WindowsRHostProcessService(ILogger<Session> sessionLogger) {
            _sessionLogger = sessionLogger;
        }

        public IProcess StartHost(Interpreter interpreter, string profilePath, string userName, ClaimsPrincipal principal, string commandLine) {
            string brokerPath = Path.GetDirectoryName(typeof(Program).Assembly.GetAssemblyPath());
            string rhostExePath = Path.Combine(brokerPath, RHostExe);
            commandLine = FormattableString.Invariant($"\"{rhostExePath}\" {commandLine}");
            var usernameBldr = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
            var domainBldr = new StringBuilder(NativeMethods.CREDUI_MAX_DOMAIN_LENGTH + 1);

            // Get R_HOME value
            var shortHome = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetShortPathName(interpreter.Info.Path, shortHome, shortHome.Capacity);

            WindowsIdentity useridentity = principal.Identity as WindowsIdentity;
            var loggedOnUser = useridentity != null && WindowsIdentity.GetCurrent().User != useridentity.User;

            // build user environment block
            Win32EnvironmentBlock eb;
            if (loggedOnUser) {
                uint error = NativeMethods.CredUIParseUserName(userName, usernameBldr, usernameBldr.Capacity, domainBldr, domainBldr.Capacity);
                if (error != 0) {
                    _sessionLogger.LogError(Resources.Error_UserNameParse, userName, error);
                    throw new ArgumentException(Resources.Error_UserNameParse.FormatInvariant(userName, error));
                }

                string username = usernameBldr.ToString();
                string domain = domainBldr.ToString();

                eb = CreateEnvironmentBlockForUser(useridentity, username, profilePath);

                // add globally set environment variables
                AddGlobalREnvironmentVariables(eb);
            } else {
                eb = Win32EnvironmentBlock.Create((useridentity ?? WindowsIdentity.GetCurrent()).Token);
            }

            // add additional variables to the environment block
            eb["R_HOME"] = shortHome.ToString();
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "R_HOME", eb["R_HOME"]);
            eb["PATH"] = FormattableString.Invariant($"{interpreter.Info.BinPath};{Environment.GetEnvironmentVariable("PATH")}");
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "PATH", eb["PATH"]);

            Win32Process win32Process;
            using (Win32NativeEnvironmentBlock nativeEnv = eb.GetNativeEnvironmentBlock()) {
                if (loggedOnUser) {
                    win32Process = Win32Process.StartProcessAsUser(useridentity, rhostExePath, commandLine, Path.GetDirectoryName(rhostExePath), nativeEnv);
                } else {
                    win32Process = Win32Process.StartProcessAsUser(null, rhostExePath, commandLine, Path.GetDirectoryName(rhostExePath), nativeEnv);
                }
            }

            win32Process.WaitForExit(250);
            if (win32Process.HasExited && win32Process.ExitCode < 0) {
                var message = ErrorCodeConverter.MessageFromErrorCode(win32Process.ExitCode);
                if (!string.IsNullOrEmpty(message)) {
                    throw new Win32Exception(message);
                }
                throw new Win32Exception(win32Process.ExitCode);
            }

            return win32Process;
        }

        private void AddGlobalREnvironmentVariables(Win32EnvironmentBlock eb) {
            // Get the broker's environment block
            var brokerEb = Win32EnvironmentBlock.Create(WindowsIdentity.GetCurrent().Token);
            foreach (var e in brokerEb) {
                if (e.Key.StartsWithOrdinal("R_")) {
                    eb[e.Key] = e.Value;
                }
            }
        }

        private Win32EnvironmentBlock CreateEnvironmentBlockForUser(WindowsIdentity useridentity, string username, string profilePath) {
            Win32EnvironmentBlock eb = Win32EnvironmentBlock.Create(useridentity.Token);

            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariableCreationBegin, username, profilePath);
            // if broker and rhost are run as different users recreate user environment variables.
            eb["USERNAME"] = username;
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "USERNAME", eb["USERNAME"]);

            eb["HOMEDRIVE"] = profilePath.Substring(0, 2);
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "HOMEDRIVE", eb["HOMEDRIVE"]);

            eb["HOMEPATH"] = profilePath.Substring(2);
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "HOMEPATH", eb["HOMEPATH"]);

            eb["USERPROFILE"] = $"{eb["HOMEDRIVE"]}{eb["HOMEPATH"]}";
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "USERPROFILE", eb["USERPROFILE"]);

            eb["APPDATA"] = $"{eb["USERPROFILE"]}\\AppData\\Roaming";
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "APPDATA", eb["APPDATA"]);

            eb["LOCALAPPDATA"] = $"{eb["USERPROFILE"]}\\AppData\\Local";
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "LOCALAPPDATA", eb["LOCALAPPDATA"]);

            eb["TEMP"] = $"{eb["LOCALAPPDATA"]}\\Temp";
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "TEMP", eb["TEMP"]);

            eb["TMP"] = $"{eb["LOCALAPPDATA"]}\\Temp";
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "TMP", eb["TMP"]);

            return eb;
        }
    }
}