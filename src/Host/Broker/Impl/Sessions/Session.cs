// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Pipes;
using Microsoft.R.Host.Broker.Startup;
using Microsoft.R.Host.Protocol;
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.Sessions {
    public class Session {
        private const string RHostExe = "Microsoft.R.Host.exe";

        private readonly ILogger _sessionLogger;
        private Win32Process _process;
        private MessagePipe _pipe;
        private volatile IMessagePipeEnd _hostEnd;

        public SessionManager Manager { get; }

        public IIdentity User { get; }

        /// <remarks>
        /// Unique for a given <see cref="User"/> only.
        /// </remarks>
        public string Id { get; }

        public Interpreter Interpreter { get; }

        public string CommandLineArguments { get; }

        private volatile SessionState _state;

        public SessionState State {
            get {
                return _state;
            }
            set {
                var oldState = _state;
                _state = value;
                StateChanged?.Invoke(this, new SessionStateChangedEventArgs(oldState, value));
            }
        }

        public event EventHandler<SessionStateChangedEventArgs> StateChanged;

        public Win32Process Process => _process;

        public SessionInfo Info => new SessionInfo {
            Id = Id,
            InterpreterId = Interpreter.Id,
            CommandLineArguments = CommandLineArguments,
            State = State,
        };

        internal Session(SessionManager manager, IIdentity user, string id, Interpreter interpreter, string commandLineArguments, ILogger sessionLogger, ILogger messageLogger) {
            Manager = manager;
            Interpreter = interpreter;
            User = user;
            Id = id;
            CommandLineArguments = commandLineArguments;
            _sessionLogger = sessionLogger;

            _pipe = new MessagePipe(messageLogger);
        }

        public void StartHost(string profilePath, string logFolder, ILogger outputLogger, LogVerbosity verbosity) {
            if (_hostEnd != null) {
                throw new InvalidOperationException("Host process is already running");
            }

            var useridentity = User as WindowsIdentity;
            // In remote broker User Identity type is always WindowsIdentity
            string suppressUI = (useridentity == null) ? string.Empty : " --suppress-ui ";
            string brokerPath = Path.GetDirectoryName(typeof(Program).Assembly.GetAssemblyPath());
            string rhostExePath = Path.Combine(brokerPath, RHostExe);
            string logFolderParam = string.IsNullOrEmpty(logFolder) ? string.Empty : Invariant($"--rhost-log-dir \"{logFolder}\"");
            string arguments = Invariant($"{suppressUI}--rhost-name \"{Id}\" {logFolderParam} --rhost-log-verbosity {(int)verbosity} {CommandLineArguments}");
            var usernameBldr = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH + 1);
            var domainBldr = new StringBuilder(NativeMethods.CREDUI_MAX_DOMAIN_LENGTH + 1);

            // Get R_HOME value
            var shortHome = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetShortPathName(Interpreter.Info.Path, shortHome, shortHome.Capacity);

            Stream stdout, stdin, stderror;
            bool loggedOnUser = useridentity != null && WindowsIdentity.GetCurrent().User != useridentity.User;

            // build user environment block
            Win32EnvironmentBlock eb;
            if (loggedOnUser) {
                uint error = NativeMethods.CredUIParseUserName(User.Name, usernameBldr, usernameBldr.Capacity, domainBldr, domainBldr.Capacity);
                if (error != 0) {
                    _sessionLogger.LogError(Resources.Error_UserNameParse, User.Name, error);
                    throw new ArgumentException(Resources.Error_UserNameParse.FormatInvariant(User.Name, error));
                }

                string username = usernameBldr.ToString();
                string domain = domainBldr.ToString();

                eb = CreateEnvironmentBlockForUser(useridentity, username, profilePath);
            } else {
                eb = Win32EnvironmentBlock.Create((useridentity ?? WindowsIdentity.GetCurrent()).Token);
            }

            // add additional variables to the environment block
            eb["R_HOME"] = shortHome.ToString();
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "R_HOME", eb["R_HOME"]);
            eb["PATH"] = Invariant($"{Interpreter.Info.BinPath};{Environment.GetEnvironmentVariable("PATH")}");
            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariable, "PATH", eb["PATH"]);


            _sessionLogger.LogInformation(Resources.Info_StartingRHost, Id, User.Name, rhostExePath, arguments);
            using (Win32NativeEnvironmentBlock nativeEnv = eb.GetNativeEnvironmentBlock()) {
                if (loggedOnUser) {
                    _process = Win32Process.StartProcessAsUser(useridentity, rhostExePath, arguments, Path.GetDirectoryName(rhostExePath), nativeEnv, out stdin, out stdout, out stderror);
                } else {
                    _process = Win32Process.StartProcessAsUser(null, rhostExePath, arguments, Path.GetDirectoryName(rhostExePath), nativeEnv, out stdin, out stdout, out stderror);
                }
            }

            _process.Exited += delegate {
                _hostEnd?.Dispose();
                _hostEnd = null;
                State = SessionState.Terminated;
            };

            _process.WaitForExit(250);
            if (_process.HasExited && _process.ExitCode < 0) {
                var message = ErrorCodeConverter.MessageFromErrorCode(_process.ExitCode);
                if (!string.IsNullOrEmpty(message)) {
                    throw new Win32Exception(message);
                }
                throw new Win32Exception(_process.ExitCode);
            }

            _sessionLogger.LogInformation(Resources.Info_StartedRHost, Id, User.Name);

            var hostEnd = _pipe.ConnectHost(_process.ProcessId);
            _hostEnd = hostEnd;

            ClientToHostWorker(stdin, hostEnd).DoNotWait();
            HostToClientWorker(stdout, hostEnd).DoNotWait();

            HostToClientErrorWorker(stderror, _process.ProcessId, (int processid, string errdata) => {
                outputLogger?.LogTrace(Resources.Trace_ErrorDataReceived, processid, errdata);
            }).DoNotWait();
        }

        private Win32EnvironmentBlock CreateEnvironmentBlockForUser(WindowsIdentity useridentity, string username, string profilePath) {
            Win32EnvironmentBlock eb = Win32EnvironmentBlock.Create(useridentity.Token);

            _sessionLogger.LogTrace(Resources.Trace_EnvironmentVariableCreationBegin, User.Name, profilePath);
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

        public void KillHost() {
            _sessionLogger.LogTrace("Killing host process for session '{0}'.", Id);

            try {
                _process?.Kill();
            } catch (Exception ex) {
                _sessionLogger.LogError(0, ex, "Failed to kill host process for session '{0}'.", Id);
                throw;
            }

            _process = null;
        }

        public IMessagePipeEnd ConnectClient() {
            _sessionLogger.LogTrace("Connecting client to message pipe for session '{0}'.", Id);

            if (_pipe == null) {
                _sessionLogger.LogError("Session '{0}' already has a client pipe connected.", Id);
                throw new InvalidOperationException(Resources.Error_RHostFailedToStart.FormatInvariant(Id));
            }

            return _pipe.ConnectClient();
        }

        private async Task HostToClientErrorWorker(Stream stream, int processid, Action<int, string> opp) {
            using (StreamReader reader = new StreamReader(stream)) {
                while (true) {
                    try {
                        string data = await reader.ReadLineAsync();
                        if (data.Length > 0) {
                            opp?.Invoke(processid, data);
                        }
                    } catch (IOException) {
                        break;
                    }
                }
            }
        }

        private async Task ClientToHostWorker(Stream stream, IMessagePipeEnd pipe) {
            using (stream) {
                while (true) {
                    byte[] message;
                    try {
                        message = await pipe.ReadAsync(CommonStartup.CancellationToken);
                    } catch (PipeDisconnectedException) {
                        break;
                    }

                    var sizeBuf = BitConverter.GetBytes(message.Length);
                    try {
                        await stream.WriteAsync(sizeBuf, 0, sizeBuf.Length);
                        await stream.WriteAsync(message, 0, message.Length);
                        await stream.FlushAsync();
                    } catch (IOException) {
                        break;
                    }
                }
            }
        }

        private async Task HostToClientWorker(Stream stream, IMessagePipeEnd pipe) {
            var sizeBuf = new byte[sizeof(int)];
            while (true) {
                if (!await FillFromStreamAsync(stream, sizeBuf)) {
                    break;
                }
                int size = BitConverter.ToInt32(sizeBuf, 0);

                var message = new byte[size];
                if (!await FillFromStreamAsync(stream, message)) {
                    break;
                }

                pipe.Write(message);
            }
        }

        private static async Task<bool> FillFromStreamAsync(Stream stream, byte[] buffer) {
            for (int index = 0, count = buffer.Length; count != 0;) {
                int read = await stream.ReadAsync(buffer, index, count);
                if (read == 0) {
                    return false;
                }

                index += read;
                count -= read;
            }

            return true;
        }
    }
}
