using Microsoft.Common.Core;
using Microsoft.Common.Core.Json;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Broker.UserProfile {
    public class UserProfileManager {
        private readonly ILogger _logger;

        public UserProfileManager(ILogger<UserProfileManager> logger) {
            _logger = logger;
        }

        public Task<RUserProfileServiceResponse> CreateProfileAsync(RUserProfileServiceRequest request, CancellationToken ct) {
            return ProfileWorkerAsync(NamedPipeServerStreamFactory.CreatorName, Resources.Error_ProfileCreationFailedIO, request, ct);
        }

        public Task<RUserProfileServiceResponse> DeleteProfileAsync(RUserProfileServiceRequest request, CancellationToken ct) {
            return ProfileWorkerAsync(NamedPipeServerStreamFactory.DeletorName, Resources.Error_ProfileDeletionFailedIO, request, ct);
        }

        private async Task<RUserProfileServiceResponse> ProfileWorkerAsync(string name, string log, RUserProfileServiceRequest request, CancellationToken ct) {
            using (NamedPipeClientStream client = new NamedPipeClientStream(name)) {
                try {
                    await client.ConnectAsync(ct);

                    string jsonReq = JsonConvert.SerializeObject(request);
                    byte[] data = Encoding.Unicode.GetBytes(jsonReq.ToString());

                    await client.WriteAsync(data, 0, data.Length, ct);
                    await client.FlushAsync(ct);

                    byte[] responseRaw = new byte[1024];
                    var bytesRead = await client.ReadAsync(responseRaw, 0, responseRaw.Length, ct);
                    string jsonResp = Encoding.Unicode.GetString(responseRaw, 0, bytesRead);
                    return Json.DeserializeObject<RUserProfileServiceResponse>(jsonResp);
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    _logger.LogError(log, request.Username);
                    return RUserProfileServiceResponse.Blank;
                }
            }
        }
    }
}
