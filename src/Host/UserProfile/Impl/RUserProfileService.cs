// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;

namespace Microsoft.R.Host.UserProfile {
    partial class RUserProfileService : ServiceBase {
        public RUserProfileService() {
            InitializeComponent();
        }

        private ManualResetEvent _workerdone;
        private CancellationTokenSource _cts;
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;
        protected override void OnStart(string[] args) {
            base.OnStart(args);

            _loggerFactory = new LoggerFactory();
            _loggerFactory
                .AddDebug()
                .AddProvider(new ServiceLoggerProvider());
            _logger = _loggerFactory.CreateLogger<RUserProfileService>();

            _cts = new CancellationTokenSource();
            _workerdone = new ManualResetEvent(false);
            CreateProfileWorkerAsync(_cts.Token).DoNotWait();
        }

        protected override void OnStop() {
            base.OnStop();
            _cts.Cancel();
            _workerdone.WaitOne(TimeSpan.FromSeconds(5));
        }

        async Task CreateProfileWorkerAsync(CancellationToken ct) {
            while (!ct.IsCancellationRequested) {
                PipeSecurity ps = new PipeSecurity();
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
                ps.AddAccessRule(par);
                using (NamedPipeServerStream server = new NamedPipeServerStream("Microsoft.R.Host.UserProfile.Creator{b101cc2d-156e-472e-8d98-b9d999a93c7a}", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps)) {
                    try {
                        await server.WaitForConnectionAsync(ct);

                        byte[] requestRaw = new byte[1024];
                        int bytesRead = 0;

                        while(bytesRead == 0 && !ct.IsCancellationRequested) {
                            bytesRead = await server.ReadAsync(requestRaw, 0, requestRaw.Length, ct);
                        }
                        
                        string json = Encoding.Unicode.GetString(requestRaw, 0, bytesRead);

                        var requestData = JsonConvert.DeserializeObject<RUserProfileCreateRequest>(json);

                        var result = RUserProfileCreator.Create(requestData, _logger);

                        string jsonResp = JsonConvert.SerializeObject(result);
                        byte[] respData = Encoding.Unicode.GetBytes(jsonResp);

                        await server.WriteAsync(respData, 0, respData.Length, ct);
                        await server.FlushAsync(ct);

                        server.WaitForPipeDrain();

                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                        _logger?.LogError(Resources.Error_UserProfileCreationError, ex.Message);
                    } 
                }
            }
            _workerdone.Set();
        }
    }
}
