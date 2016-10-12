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
using System.Runtime.InteropServices;

namespace Microsoft.R.Host.UserProfile {
    partial class RUserProfileService : ServiceBase {

        private static int ServiceShutdownTimeoutMs => 5000;
        private static int ClientResponseReadTimeoutMs => 3000;

        public RUserProfileService() {
            InitializeComponent();
        }

        private ManualResetEvent _workerdone;
        private CancellationTokenSource _cts;
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;
        protected override void OnStart(string[] args) {
            ServiceStartPending();
            _loggerFactory = new LoggerFactory();
            _loggerFactory
                .AddDebug()
                .AddProvider(new ServiceLoggerProvider());
            _logger = _loggerFactory.CreateLogger<RUserProfileService>();

            _cts = new CancellationTokenSource();
            _workerdone = new ManualResetEvent(false);
            CreateProfileWorkerAsync(_cts.Token).DoNotWait();

            ServiceStarted();
        }

        protected override void OnStop() {
            ServiceStopPending();

            _cts.Cancel();
            _workerdone.WaitOne(TimeSpan.FromMilliseconds(ServiceShutdownTimeoutMs));

            ServiceStopped();
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

                        // Waiting here to allow client to finish reading 
                        // client should disconnect after reading.
                        while (bytesRead == 0 && !ct.IsCancellationRequested) {
                            bytesRead = await server.ReadAsync(requestRaw, 0, requestRaw.Length, ct);
                        }

                        // if there was an attempt to write, disconnect.
                        server.Disconnect();
                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                        _logger?.LogError(Resources.Error_UserProfileCreationError, ex.Message);
                    } 
                }
            }
            _workerdone.Set();
        }

        private void ServiceStartPending() {
            SetServiceStatus(ServiceState.SERVICE_START_PENDING, 100000);
        }

        private void ServiceStarted() {
            SetServiceStatus(ServiceState.SERVICE_RUNNING);
        }

        private void ServiceStopPending() {
            SetServiceStatus(ServiceState.SERVICE_STOP_PENDING, 100000);
        }

        private void ServiceStopped() {
            SetServiceStatus(ServiceState.SERVICE_STOPPED);
        }

        private void SetServiceStatus(ServiceState state, long wait = 0) {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = state;

            if(wait > 0) {
                serviceStatus.dwWaitHint = wait;
            }
            
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        private enum ServiceState {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ServiceStatus {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
    }
}
