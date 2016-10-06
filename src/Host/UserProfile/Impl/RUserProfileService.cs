// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Common.Core;

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

                        byte[] request = new byte[1024];
                        int bytesRead = 0;

                        while(bytesRead == 0 && !ct.IsCancellationRequested) {
                            bytesRead = await server.ReadAsync(request, 0, request.Length, ct);
                        }
                        
                        byte[] data = new byte[bytesRead];
                        Array.Copy(request, data, bytesRead);

                        string json = Encoding.Unicode.GetString(data);
                        JArray dataArray = JArray.Parse(json);

                        uint error = 13; // invalid data
                        bool profileExists = false;
                        string profilePath = string.Empty;

                        if(dataArray.Count == 3) {
                            string username = dataArray[0].Value<string>();
                            string domain = dataArray[1].Value<string>();
                            string password = dataArray[2].Value<string>();

                            var result = RUserProfileCreator.Create(username, domain, password);
                            error = result.Win32Result;
                            profileExists = result.ProfileExists;
                            profilePath = result.ProfilePath;

                        } 

                        JArray respArray = new JArray();
                        respArray.Add(error);
                        respArray.Add(profileExists);
                        respArray.Add(profilePath);

                        byte[] respData = Encoding.Unicode.GetBytes(respArray.ToString());

                        await server.WriteAsync(respData, 0, respData.Length, ct);
                        await server.FlushAsync(ct);

                        server.WaitForPipeDrain();

                    } catch (IOException) {
                    } 
                }
            }
            _workerdone.Set();
        }
    }
}
