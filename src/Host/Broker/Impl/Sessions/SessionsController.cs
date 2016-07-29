// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Pipes;
using Microsoft.R.Host.Broker.Security;

namespace Microsoft.R.Host.Broker.Sessions {
    [Authorize(Policy = Policies.RUser)]
    [Route("/sessions")]
    public class SessionsController : Controller {
        private readonly InterpreterManager _interpManager;
        private readonly SessionManager _sessionManager;

        public SessionsController(InterpreterManager interpManager, SessionManager sessionManager) {
            _interpManager = interpManager;
            _sessionManager = sessionManager;
        }

        [HttpGet]
        public IEnumerable<SessionInfo> Get() {
            yield break;
        }

        [HttpPut("{id}")]
        public SessionInfo Put(string id, [FromBody] SessionCreateRequest request) {
            SecureString securePassword = null;
            string password = User.FindFirst(Claims.Password)?.Value;
            if (password != null) {
                securePassword = new SecureString();
                foreach (var ch in password) {
                    securePassword.AppendChar(ch);
                }
            }

            var interp = _interpManager.Interpreters.First(ip => ip.Info.Id ==  request.InterpreterId);
            var session = _sessionManager.CreateSession(id, interp, User.Identity, securePassword);
            return session.Info;
        }

        [HttpGet("{id}/pipe")]
        public IActionResult GetPipe(string id) {
            var session = _sessionManager.GetSession(User.Identity, id);
            return new WebSocketPipeAction(session);
        }
    }
}
