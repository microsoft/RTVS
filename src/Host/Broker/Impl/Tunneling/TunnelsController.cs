// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Net;

namespace Microsoft.R.Host.Broker.Tunneling {
    [Authorize]
    [Route("/tunnels")]
    public class TunnelsController : Controller {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id) {
            if (HttpContext.Connection.RemoteIpAddress == null && !IPAddress.IsLoopback(HttpContext.Connection.RemoteIpAddress)) {
                return Forbid();
            }

            if (!HttpContext.WebSockets.IsWebSocketRequest) {
                return BadRequest("Websocket connection expected");
            }

            var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await WebSocketWorker(socket);

            return new EmptyResult();
        }

        private Task WebSocketWorker(WebSocket socket) {
            // TODO: tunnel data to host
            return Task.CompletedTask;
        }
    }
}
