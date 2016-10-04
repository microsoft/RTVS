// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.UserProfile {
    partial class RUserProfileService : ServiceBase {
        public RUserProfileService() {
            InitializeComponent();
        }

        private ManualResetEvent _workerdone;
        private CancellationTokenSource _cts;
        protected override void OnStart(string[] args) {
            base.OnStart(args);
            _cts = new CancellationTokenSource();
            _workerdone = new ManualResetEvent(false);
            Task.WhenAny(CreateProfileWorkerAsync(_cts.Token));
        }

        protected override void OnStop() {
            base.OnStop();
            _cts.Cancel();
            _workerdone.WaitOne(TimeSpan.FromSeconds(10));
        }

        async Task CreateProfileWorkerAsync(CancellationToken ct) {
            while (!ct.IsCancellationRequested) {
                PipeSecurity ps = new PipeSecurity();
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
                ps.AddAccessRule(par);
                using (NamedPipeServerStream server = new NamedPipeServerStream("RUserCreatorPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps))
                using (BinaryReader reader = new BinaryReader(server))
                using (BinaryWriter writer = new BinaryWriter(server)) {
                    try {
                        await server.WaitForConnectionAsync(ct);

                        string username = reader.ReadString();
                        string domain = reader.ReadString();
                        string password = reader.ReadString();

                        var result = RUserProfileCreator.Create(username, domain, password);

                        writer.Write(result.Win32Result);
                        writer.Write(result.ProfileExists);
                        writer.Write(result.ProfilePath);

                        server.WaitForPipeDrain();
                    } catch (IOException) {

                    } 
                }
            }
            _workerdone.Set();
        }
    }
}
