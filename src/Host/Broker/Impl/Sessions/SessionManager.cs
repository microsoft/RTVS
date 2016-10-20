// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Security.Principal;
using Microsoft.Common.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Pipes;
using Microsoft.R.Host.Protocol;

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
                return userSessions.ToArray() ?? Enumerable.Empty<Session>();
            }
        }

        public IEnumerable<string> GetUsers() {
            lock (_sessions) {
                return _sessions.Keys.ToArray();
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

        private const int MaximumConcurrentClientWindowsUsers = 1;
        private bool IsUserAllowedToCreateSession(IIdentity user) {
            lock (_sessions) {
                int activeUsers = _sessions.Keys.Where((k) => _sessions[k].Count > 0).Count();
                bool userHasARecord = _sessions.Keys.Contains(user.Name);

                bool userHasActiveSession = false;
                if (userHasARecord) {
                    List<Session> userSessions;
                    _sessions.TryGetValue(user.Name, out userSessions);

                    userHasActiveSession = userSessions?.Count > 0;
                }

                if (userHasActiveSession) {
                    // This user's session already exists
                    return true;
                }

                if (activeUsers < MaximumConcurrentClientWindowsUsers) {
                    // User session doesn't exist AND there are slot(s) available for user session
                    // Allow user to create a session on Client Windows
                    return true;
                } 

                // user session doesn't exist AND there are NO slots available for user session
                // Do NOT allow user to create a session on Client Windows
                return false;
                
            }
        }

        public Session CreateSession(IIdentity user, string id, Interpreter interpreter, SecureString password, string profilePath, string commandLineArguments) {
            Session session;

            lock (_sessions) {
                if (!NativeMethods.IsWindowsServer() && !IsUserAllowedToCreateSession(user)) {
                    // This is Client Windows, only 1 user is allowed to create sessions at a time.
                    throw new BrokerMaxedUsersException(Resources.Exception_MaxAllowedUsers);
                }

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
                password,
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
