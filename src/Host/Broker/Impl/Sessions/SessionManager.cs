// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Pipes;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Sessions {
    public class SessionManager {
        private readonly LoggingOptions _loggingOptions;
        private readonly ILogger _hostOutputLogger, _messageLogger;
        private readonly IRHostProcessService _processService;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger _sessionLogger;

        private readonly Dictionary<string, List<Session>> _sessions = new Dictionary<string, List<Session>>();
        private readonly HashSet<string> _blockedUsers = new HashSet<string>();

        public SessionManager(IRHostProcessService processService
            , IApplicationLifetime applicationLifetime
            , IOptions<LoggingOptions> loggingOptions
            , ILogger<Session> sessionLogger
            , ILogger<MessagePipe> messageLogger
            , ILogger<Process> hostOutputLogger) {

            _loggingOptions = loggingOptions.Value;
            _processService = processService;
            _applicationLifetime = applicationLifetime;
            _sessionLogger = sessionLogger;

            if (_loggingOptions.LogPackets) {
                _messageLogger = messageLogger;
            }

            if (_loggingOptions.LogHostOutput) {
                _hostOutputLogger = hostOutputLogger;
            }
        }

        public IEnumerable<Session> GetSessions(IIdentity user) {
            lock (_sessions) {
                return _sessions.TryGetValue(user.Name, out var userSessions) 
                    ? userSessions.ToArray() 
                    : Enumerable.Empty<Session>();
            }
        }

        public IDisposable BlockSessionsCreationForUser(IIdentity user, bool terminateSession) {
            lock (_sessions) {
                if (terminateSession) {
                    var userSessions = GetOrCreateSessionList(user);
                    var sessions = userSessions.ToArray();
                    foreach (var session in sessions) {
                        userSessions.Remove(session);
                        Task.Run(() => session.KillHost()).SilenceException<Exception>().DoNotWait();
                        session.State = SessionState.Terminated;
                    }
                }
                _blockedUsers.Add(user.Name);
                return Disposable.Create(() => UnblockSessionCreationForUser(user));
            }
        }

        private void UnblockSessionCreationForUser(IIdentity user) {
            lock (_sessions) {
                _blockedUsers.Remove(user.Name);
            }
        }

        public IEnumerable<string> GetUsers() {
            lock (_sessions) {
                return _sessions.Keys.ToArray();
            }
        }

        public Session GetSession(IIdentity user, string id) {
            lock (_sessions) {
                if (user != null &&_blockedUsers.Contains(user.Name)) {
                    return null;
                }
                return _sessions.Values
                    .SelectMany(sessions => sessions)
                    .FirstOrDefault(session => (user == null || session.User.Name == user.Name) && session.Id == id);
            }
        }

        private List<Session> GetOrCreateSessionList(IIdentity user) {
            lock (_sessions) {
                List<Session> userSessions;
                _sessions.TryGetValue(user.Name, out userSessions);
                if (userSessions == null) {
                    _sessions[user.Name] = userSessions = new List<Session>();
                }

                return userSessions;
            }
        }

        public Session CreateSession(ClaimsPrincipal principal, string id, Interpreter interpreter, string commandLineArguments, bool isInteractive) {
            Session session;
            var user = principal.Identity;
            lock (_sessions) {
                if (_blockedUsers.Contains(user.Name)) {
                    throw new InvalidOperationException(Resources.Error_BlockedByProfileDeletion.FormatInvariant(user.Name));
                }

                var oldUserSessions = GetOrCreateSessionList(user);

                var oldSessions = oldUserSessions.Where(s => s.Id == id).ToArray();
                foreach (var oldSession in oldSessions) {
                    oldUserSessions.Remove(oldSession);
                    Task.Run(() => oldSession.KillHost()).SilenceException<Exception>().DoNotWait();
                    oldSession.State = SessionState.Terminated;
                }

                var userSessions = GetOrCreateSessionList(user);
                session = new Session(this, _processService, _applicationLifetime, _sessionLogger, _messageLogger, principal, interpreter, id, commandLineArguments, isInteractive);
                session.StateChanged += Session_StateChanged;

                userSessions.Add(session);
            }

            session.StartHost(
                _loggingOptions.LogFolder,
                _loggingOptions.LogHostOutput ? _hostOutputLogger : null,
                _loggingOptions.LogPackets || _loggingOptions.LogHostOutput ? LogVerbosity.Traffic : LogVerbosity.Minimal);

            return session;
        }

        private void Session_StateChanged(object sender, SessionStateChangedEventArgs e) {
            var session = (Session)sender;
            if (e.NewState == SessionState.Terminated) {
                lock (_sessions) {
                    var userSessions = GetOrCreateSessionList(session.User);
                    userSessions.Remove(session);

                    if (userSessions.Count == 0) {
                        _sessions.Remove(session.User.Name);
                    }
                }
            }
        }
    }
}
