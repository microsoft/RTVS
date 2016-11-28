// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileCreator {
        public static async Task CreateProfileAsync(int serverTimeOutms = 0, int clientTimeOutms = 0, IUserProfileServices userProfileService = null, CancellationToken ct = default(CancellationToken), ILogger logger = null) {
            userProfileService = userProfileService ?? new RUserProfileCreatorImpl();
            PipeSecurity ps = new PipeSecurity();
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            ps.AddAccessRule(par);
            using (NamedPipeServerStream server = new NamedPipeServerStream("Microsoft.R.Host.UserProfile.Creator{b101cc2d-156e-472e-8d98-b9d999a93c7a}", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps)) {
                await server.WaitForConnectionAsync(ct);

                ManualResetEventSlim forceDisconnect = new ManualResetEventSlim(false);
                try {
                    if (serverTimeOutms + clientTimeOutms > 0) {
                        Task.Run(() => {
                            // This handles Empty string input cases. This is usually the case were client is connected and writes a empty string.
                            // on the server side this blocks the ReadAsync indefinitely (even with the cancellation set). The code below protects 
                            // the server from being indefinitely blocked by a malicious client.
                            forceDisconnect.Wait(serverTimeOutms + clientTimeOutms);
                            if (server.IsConnected) {
                                server.Disconnect();
                                logger?.LogError(Resources.Error_ClientTimedOut);
                            }
                        }).DoNotWait();
                    }

                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct)) {
                        if (serverTimeOutms > 0) {
                            cts.CancelAfter(serverTimeOutms);
                        }

                        await CreateProfileHandleUserCredentails(userProfileService, server, cts.Token, logger);
                    }

                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct)) {
                        if (clientTimeOutms > 0) {
                            cts.CancelAfter(clientTimeOutms);
                        }

                        // Waiting here to allow client to finish reading client should disconnect after reading.
                        byte[] requestRaw = new byte[1024];
                        int bytesRead = 0;
                        while (bytesRead == 0 && !cts.Token.IsCancellationRequested) {
                            bytesRead = await server.ReadAsync(requestRaw, 0, requestRaw.Length, cts.Token);
                        }

                        // if there was an attempt to write, disconnect.
                        server.Disconnect();
                    }
                } finally {
                    // server work is done.
                    forceDisconnect.Set();
                }
            }
        }

        private static async Task CreateProfileHandleUserCredentails(IUserProfileServices userProfileService, Stream stream,  CancellationToken ct, ILogger logger = null) {
            byte[] requestRaw = new byte[1024];
            int bytesRead = 0;

            while (bytesRead == 0 && !ct.IsCancellationRequested) {
                bytesRead = await stream.ReadAsync(requestRaw, 0, requestRaw.Length, ct);
            }

            string json = Encoding.Unicode.GetString(requestRaw, 0, bytesRead);

            var requestData = Json.DeserializeObject<RUserProfileCreateRequest>(json);


            var result = userProfileService.CreateUserProfile(requestData, logger);

            string jsonResp = JsonConvert.SerializeObject(result);
            byte[] respData = Encoding.Unicode.GetBytes(jsonResp);

            await stream.WriteAsync(respData, 0, respData.Length, ct);
            await stream.FlushAsync(ct);
        }
    }
}
