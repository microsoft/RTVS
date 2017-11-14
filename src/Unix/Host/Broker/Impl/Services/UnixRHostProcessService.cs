// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Sessions;

namespace Microsoft.R.Host.Broker.Services {
    internal abstract class UnixRHostProcessService : IRHostProcessService {
        private readonly ILogger<Session> _sessionLogger;
        private readonly IProcessServices _ps;

        public UnixRHostProcessService(ILogger<Session> sessionLogger, IProcessServices ps) {
            _sessionLogger = sessionLogger;
            _ps = ps;
        }

        public IProcess StartHost(Interpreter interpreter, string profilePath, string userName, ClaimsPrincipal principal, string commandLine) {
            IProcess process;
            if (principal.HasClaim((c) => c.Type == UnixClaims.RPassword)) {
                var args = ParseArgumentsIntoList(commandLine);
                var environment = GetHostEnvironment(interpreter, profilePath, userName);
                var password = principal.FindFirst(UnixClaims.RPassword).Value;
                process = Utility.AuthenticateAndRunAsUser(_sessionLogger, _ps, userName, password, profilePath, args, environment);
            } else {
                process = Utility.RunAsCurrentUser(_sessionLogger, _ps, commandLine, GetRHomePath(interpreter), GetLoadLibraryPath(interpreter));
            }
            process.WaitForExit(250);
            if (process.HasExited && process.ExitCode != 0) {
                var message = _ps.MessageFromExitCode(process.ExitCode);
                if (!string.IsNullOrEmpty(message)) {
                    throw new Win32Exception(message);
                }
                throw new Win32Exception(process.ExitCode);
            }

            return process;
        }



        private string GetRHomePath(Interpreter interpreter) => interpreter.RInterpreterInfo.InstallPath;

        protected abstract string GetRHostBinaryPath();
        protected abstract string GetLoadLibraryPath(Interpreter interpreter);

        private IDictionary<string, string> GetHostEnvironment(Interpreter interpreter, string profilePath, string userName) {
            // set required environment variables.
            var environment = new Dictionary<string, string>() {
                { "HOME"                    , profilePath},
                { "PATH"                    , Environment.GetEnvironmentVariable("PATH")},
                { "PWD"                     , profilePath},
                { "R_HOME"                  , GetRHomePath(interpreter)},
                { "USER"                    , Utility.GetUnixUserName(userName)},
                { "LD_LIBRARY_PATH"         , GetLoadLibraryPath(interpreter)}
            };

            // set optional environment variables if available
            string[] optionalVariables = { "LN_S", "R_ARCH", "R_BROWSER", "R_BZIPCMD", "R_GZIPCMD", "R_LIBS_SITE", "R_INCLUDE_DIR", "R_DOC_DIR", "R_PAPERSIZE", "R_PAPERSIZE_USER", "R_PDFVIEWER", "R_PRINTCMD", "R_RD4PDF", "R_SHARE_DIR", "R_TEXI2DVICMD", "R_UNZIPCMD", "R_ZIPCMD", "SED", "SHELL", "SHLVL", "TAR" };
            foreach(var key in optionalVariables) {
                SetEnvironmentVaraibleIfAvailable(environment, key);
            }

            return environment;
        }

        private void SetEnvironmentVaraibleIfAvailable(Dictionary<string,string> environment, string key) {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(value)) {
                environment.Add(key, value);
            }
        }

        private static IEnumerable<string> ParseArgumentsIntoList(string arguments) {
            var results = new List<string>();
            var currentArgument = new StringBuilder();
            var inQuotes = false;

            // Iterate through all of the characters in the argument string.
            for (var i = 0; i < arguments.Length; i++) {
                // From the current position, iterate through contiguous backslashes.
                var backslashCount = 0;
                for (; i < arguments.Length && arguments[i] == '\\'; i++, backslashCount++) ;
                if (backslashCount > 0) {
                    if (i >= arguments.Length || arguments[i] != '"') {
                        // Backslashes not followed by a double quote:
                        // they should all be treated as literal backslashes.
                        currentArgument.Append('\\', backslashCount);
                        i--;
                    } else {
                        // Backslashes followed by a double quote:
                        // - Output a literal slash for each complete pair of slashes
                        // - If one remains, use it to make the subsequent quote a literal.
                        currentArgument.Append('\\', backslashCount / 2);
                        if (backslashCount % 2 == 0) {
                            i--;
                        } else {
                            currentArgument.Append('"');
                        }
                    }
                    continue;
                }

                var c = arguments[i];

                // If this is a double quote, track whether we're inside of quotes or not.
                // Anything within quotes will be treated as a single argument, even if
                // it contains spaces.
                if (c == '"') {
                    inQuotes = !inQuotes;
                    continue;
                }

                // If this is a space/tab and we're not in quotes, we're done with the current
                // argument, and if we've built up any characters in the current argument,
                // it should be added to the results and then reset for the next one.
                if ((c == ' ' || c == '\t') && !inQuotes) {
                    if (currentArgument.Length > 0) {
                        results.Add(currentArgument.ToString());
                        currentArgument.Clear();
                    }
                    continue;
                }

                // Nothing special; add the character to the current argument.
                currentArgument.Append(c);
            }

            // If we reach the end of the string and we still have anything in our current
            // argument buffer, treat it as an argument to be added to the results.
            if (currentArgument.Length > 0) {
                results.Add(currentArgument.ToString());
            }

            return results;
        }
    }
}