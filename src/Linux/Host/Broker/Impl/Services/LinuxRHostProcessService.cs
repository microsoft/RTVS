// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Broker.Security;

namespace Microsoft.R.Host.Broker.Services {
    class LinuxRHostProcessService : IRHostProcessService {
        private readonly ILogger<Session> _sessionLogger;
        private readonly IProcessServices _ps;

        public LinuxRHostProcessService(ILogger<Session> sessionLogger, IProcessServices ps) {
            _sessionLogger = sessionLogger;
            _ps = ps;
        }

        public IProcess StartHost(Interpreter interpreter, string profilePath, string userName, ClaimsPrincipal principal, string commandLine) {
            var args = ParseArgumentsIntoList(commandLine);
            var environment = GetHostEnvironment(interpreter, profilePath, userName);
            var password = principal.FindFirst(UnixClaims.RPassword).Value;

            Process process = Utility.AuthenticateAndRunAsUser(_sessionLogger, _ps, userName, password, profilePath, args, environment);
            process.WaitForExit(250);
            if (process.HasExited && process.ExitCode < 0) {
                var message = _ps.MessageFromExitCode(process.ExitCode);
                if (!string.IsNullOrEmpty(message)) {
                    throw new Win32Exception(message);
                }
                throw new Win32Exception(process.ExitCode);
            }

            return new UnixProcess(process);
        }

        private IDictionary<string, string> GetHostEnvironment(Interpreter interpreter, string profilePath, string userName) {
            var currentEnvironment = Process.GetCurrentProcess().StartInfo.Environment;
            string siteLibrary = string.Join(":", interpreter.RInterpreterInfo.SiteLibraryDirs);
            string loadLibraryPath = string.Join(":", new string[] { interpreter.RInterpreterInfo.BinPath, currentEnvironment["LD_LIBRARY_PATH"]});

            Dictionary<string, string> environment = new Dictionary<string, string>() {
                { "HOME"                    , profilePath},
                { "LD_LIBRARY_PATH"         , loadLibraryPath},
                { "LN_S"                    , GetCurrentOrDefault("LN_S", currentEnvironment)},
                { "PATH"                    , currentEnvironment["PATH"]},
                { "PWD"                     , profilePath},
                { "R_ARCH"                  , GetCurrentOrDefault("R_ARCH", currentEnvironment)},
                { "R_BROWSER"               , GetCurrentOrDefault("R_BROWSER", currentEnvironment)},
                { "R_BZIPCMD"               , GetCurrentOrDefault("R_BZIPCMD", currentEnvironment)},
                { "R_DOC_DIR"               , interpreter.RInterpreterInfo.DocPath},
                { "R_GZIPCMD"               , GetCurrentOrDefault("R_GZIPCMD", currentEnvironment)},
                { "R_HOME"                  , interpreter.RInterpreterInfo.InstallPath},
                { "R_INCLUDE_DIR"           , interpreter.RInterpreterInfo.IncludePath},
                { "R_LIBS_SITE"             , siteLibrary},
                { "R_PAPERSIZE"             , GetCurrentOrDefault("R_PAPERSIZE", currentEnvironment)},
                { "R_PAPERSIZE_USER"        , GetCurrentOrDefault("R_PAPERSIZE_USER", currentEnvironment)},
                { "R_PDFVIEWER"             , GetCurrentOrDefault("R_PDFVIEWER", currentEnvironment)},
                { "R_PRINTCMD"              , GetCurrentOrDefault("R_PRINTCMD", currentEnvironment)},
                { "R_RD4PDF"                , GetCurrentOrDefault("R_RD4PDF", currentEnvironment)},
                { "R_SHARE_DIR"             , interpreter.RInterpreterInfo.RShareDir},
                { "R_TEXI2DVICMD"           , GetCurrentOrDefault("R_TEXI2DVICMD", currentEnvironment)},
                { "R_UNZIPCMD"              , GetCurrentOrDefault("R_UNZIPCMD", currentEnvironment)},
                { "R_ZIPCMD"                , GetCurrentOrDefault("R_ZIPCMD", currentEnvironment)},
                { "SED"                     , GetCurrentOrDefault("SED", currentEnvironment)},
                { "SHELL"                   , GetCurrentOrDefault("SHELL", currentEnvironment)},
                { "SHLVL"                   , GetCurrentOrDefault("SHLVL", currentEnvironment)},
                { "TAR"                     , GetCurrentOrDefault("TAR", currentEnvironment)},
                { "USER"                    , userName},
            };
            return environment;
        }

        private static string GetCurrentOrDefault(string key, IDictionary<string,string> current) {
            var value = current[key];
            if (string.IsNullOrEmpty(value) || !_defaultEnvironment.TryGetValue(key, out value)) {
                return string.Empty;
            }
            return value;
        }
        private static readonly IDictionary<string, string> _defaultEnvironment = new Dictionary<string,string>(){
            { "LN_S"                    , "ln -s"},
            {"R_ARCH"                  , ""},
            {"R_BROWSER"               , "xdg-open"},
            {"R_BZIPCMD"               , "/bin/bzip2"},
            {"R_GZIPCMD"               , "/bin/gzip -n"},
            {"R_PAPERSIZE"             , "letter"},
            {"R_PAPERSIZE_USER"        , "letter"},
            {"R_PDFVIEWER"             , "/usr/bin/xdg-open"},
            {"R_PRINTCMD"              , "/usr/bin/lpr"},
            {"R_RD4PDF"                , "times,inconsolata,hyper"},
            {"R_TEXI2DVICMD"           , "/usr/bin/texi2dvi"},
            {"R_UNZIPCMD"              , "/usr/bin/unzip"},
            {"R_ZIPCMD"                , "/usr/bin/zip"},
            { "SED"                     , "/bin/sed"},
            { "SHELL"                   , "/bin/bash"},
            { "SHLVL"                   , "2"},
            { "TAR"                     , "/bin/tar"},
        };

        private static IEnumerable<string> ParseArgumentsIntoList(string arguments) {
            List<string> results = new List<string>();
            var currentArgument = new StringBuilder();
            bool inQuotes = false;

            // Iterate through all of the characters in the argument string.
            for (int i = 0; i < arguments.Length; i++) {
                // From the current position, iterate through contiguous backslashes.
                int backslashCount = 0;
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

                char c = arguments[i];

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