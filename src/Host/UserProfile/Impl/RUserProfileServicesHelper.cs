// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Json;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;
using Microsoft.R.Platform.Core.IO;
using Microsoft.R.Platform.IO;
using Newtonsoft.Json;

namespace Microsoft.R.Host.UserProfile {
    public class RUserProfileServicesHelper {
        public static async Task CreateProfileAsync(int serverTimeOutms = 0, int clientTimeOutms = 0, IUserProfileServices userProfileService = null, IUserProfileNamedPipeFactory pipeFactory = null, CancellationToken ct = default(CancellationToken), ILogger logger = null) {
            userProfileService = userProfileService ?? new RUserProfileServices();
            pipeFactory = pipeFactory ?? new NamedPipeServerStreamFactory();
            await ProfileServiceOperationAsync(userProfileService.CreateUserProfile, NamedPipeServerStreamFactory.CreatorName, pipeFactory, serverTimeOutms, clientTimeOutms, ct, logger);
        }

        public static async Task DeleteProfileAsync(int serverTimeOutms = 0, int clientTimeOutms = 0, IUserProfileServices userProfileService = null, IUserProfileNamedPipeFactory pipeFactory = null, CancellationToken ct = default(CancellationToken), ILogger logger = null) {
            userProfileService = userProfileService ?? new RUserProfileServices();
            pipeFactory = pipeFactory ?? new NamedPipeServerStreamFactory();
            await ProfileServiceOperationAsync(userProfileService.DeleteUserProfile, NamedPipeServerStreamFactory.DeletorName, pipeFactory, serverTimeOutms, clientTimeOutms, ct, logger);
        }

        private static async Task ProfileServiceOperationAsync(Func<IUserCredentials, ILogger, IUserProfileServiceResult> operation, string name, IUserProfileNamedPipeFactory pipeFactory, int serverTimeOutms = 0, int clientTimeOutms = 0, CancellationToken ct = default(CancellationToken), ILogger logger = null) {
            using (var server = pipeFactory.CreatePipe(name)) {
                await server.WaitForConnectionAsync(ct);

                var forceDisconnect = new ManualResetEventSlim(false);
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

                        var requestRaw = new byte[1024];
                        var bytesRead = 0;

                        while (bytesRead == 0 && !cts.IsCancellationRequested) {
                            bytesRead = await server.ReadAsync(requestRaw, 0, requestRaw.Length, cts.Token);
                        }

                        var json = Encoding.Unicode.GetString(requestRaw, 0, bytesRead);
                        var requestData = Json.DeserializeObject<RUserProfileServiceRequest>(json);
                        var result = operation?.Invoke(requestData, logger);

                        var jsonResp = JsonConvert.SerializeObject(result);
                        var respData = Encoding.Unicode.GetBytes(jsonResp);

                        await server.WriteAsync(respData, 0, respData.Length, cts.Token);
                        await server.FlushAsync(cts.Token);
                    }

                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct)) {
                        if (clientTimeOutms > 0) {
                            cts.CancelAfter(clientTimeOutms);
                        }

                        // Waiting here to allow client to finish reading client should disconnect after reading.
                        var requestRaw = new byte[1024];
                        var bytesRead = 0;
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
    }
}
