// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO.Pipes;
using System.Security.Principal;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Protocol {
    public class RUserProfileCreator {
        public static async Task CreateProfileAsync(int serverTimeOutms = 0, int clientTimeOutms = 0, IUserProfileServices userProfileService = null,  CancellationToken ct = default(CancellationToken), ILogger logger = null) {
            userProfileService = userProfileService ?? new RUserProfileCreatorImpl();
            PipeSecurity ps = new PipeSecurity();
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            ps.AddAccessRule(par);
            using (NamedPipeServerStream server = new NamedPipeServerStream("Microsoft.R.Host.UserProfile.Creator{b101cc2d-156e-472e-8d98-b9d999a93c7a}", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps)) {
                await server.WaitForConnectionAsync(ct);

                using (CancellationTokenSource serverTimedCTS = new CancellationTokenSource(serverTimeOutms))
                using (CancellationTokenSource combinedServerCTS = CancellationTokenSource.CreateLinkedTokenSource(serverTimedCTS.Token, ct)) {
                    CancellationToken serverCT = ct;
                    if (serverTimeOutms > 0) {
                        serverCT = combinedServerCTS.Token;
                    }

                    await CreateProfileHandleUserCredentails(userProfileService, server, serverCT, logger);
                }

                using (CancellationTokenSource clientTimedCTS = new CancellationTokenSource(clientTimeOutms))
                using(CancellationTokenSource combinedClientCTS = CancellationTokenSource.CreateLinkedTokenSource(clientTimedCTS.Token, ct)) {
                    CancellationToken clientCT = ct;
                    if (clientTimeOutms > 0) {
                        clientCT = combinedClientCTS.Token;
                    }

                    // Waiting here to allow client to finish reading client should disconnect after reading.
                    byte[] requestRaw = new byte[1024];
                    int bytesRead = 0;
                    while (bytesRead == 0 && !clientCT.IsCancellationRequested) {
                        bytesRead = await server.ReadAsync(requestRaw, 0, requestRaw.Length, clientCT);
                    }

                    // if there was an attempt to write, disconnect.
                    server.Disconnect();
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

            var requestData = JsonConvert.DeserializeObject<RUserProfileCreateRequest>(json);

            var result = userProfileService.CreateUserProfile(requestData, logger);

            string jsonResp = JsonConvert.SerializeObject(result);
            byte[] respData = Encoding.Unicode.GetBytes(jsonResp);

            await stream.WriteAsync(respData, 0, respData.Length, ct);
            await stream.FlushAsync(ct);
        }
    }
}
