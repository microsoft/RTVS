// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Common.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Pipes;

namespace Microsoft.R.Host.Broker.Sessions {
    public class SessionManager {
        private readonly InterpreterManager _interpManager;
        private readonly LoggingOptions _loggingOptions;
        private readonly ILogger _hostOutputLogger, _messageLogger, _sessionLogger;

        private readonly Dictionary<string, List<Session>> _sessions = new Dictionary<string, List<Session>>();

        [ImportingConstructor]
        public SessionManager(
            InterpreterManager interpManager,
            IOptions<LoggingOptions> loggingOptions,
            ILogger<Session> sessionLogger,
            ILogger<MessagePipe> messageLogger,
            ILogger<Process> hostOutputLogger
        ) {
            _interpManager = interpManager;
            _loggingOptions = loggingOptions.Value;
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
                List<Session> userSessions;
                _sessions.TryGetValue(user.Name, out userSessions);
                return userSessions ?? Enumerable.Empty<Session>();
            }
        }

        public Session GetSession(IIdentity user, string id) {
            lock (_sessions) {
                return _sessions.Values.SelectMany(sessions => sessions).FirstOrDefault(session => session.User.Name == user.Name && session.Id == id);
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

        public Session CreateSession(IIdentity user, string id, Interpreter interpreter, ClaimsPrincipal cp, string profilePath, string commandLineArguments) {
            Session session;

            lock (_sessions) {
                var userSessions = GetOrCreateSessionList(user);

                var oldSession = userSessions.FirstOrDefault(s => s.Id == id);
                if (oldSession != null) {
                    try {
                        oldSession.KillHost();
                    } catch (Exception) { }

                    oldSession.State = SessionState.Terminated;
                }

                session = new Session(this, user, id, interpreter, commandLineArguments, _sessionLogger, _messageLogger);
                session.StateChanged += Session_StateChanged;

                userSessions.Add(session);
            }

            session.StartHost(
                cp,
                profilePath,
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
                }
            }
        }
    }
}
