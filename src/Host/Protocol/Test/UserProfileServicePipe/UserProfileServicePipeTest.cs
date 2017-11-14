// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Json;
using Microsoft.Common.Core.OS;
using Microsoft.R.Host.UserProfile;
using Microsoft.R.Platform.IO;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.R.Host.Protocol.Test.UserProfileServicePipe {
    public class UserProfileServicePipeTest {

        private async Task<UserProfileResultMock> CreateProfileClientTestWorkerAsync(string input, CancellationToken ct = default(CancellationToken)) {
            string jsonResp = null;
            using (var client = new NamedPipeClientStream("Microsoft.R.Host.UserProfile.Creator{b101cc2d-156e-472e-8d98-b9d999a93c7a}")) {
                await client.ConnectAsync(ct);
                var data = Encoding.Unicode.GetBytes(input);

                await client.WriteAsync(data, 0, data.Length, ct);
                await client.FlushAsync(ct);

                var responseRaw = new byte[1024];
                var bytesRead = await client.ReadAsync(responseRaw, 0, responseRaw.Length, ct);
                jsonResp = Encoding.Unicode.GetString(responseRaw, 0, bytesRead);
            }
            return Json.DeserializeObject<UserProfileResultMock>(jsonResp);
        }

        private async Task CreateProfileTestRunnerAsync(IUserProfileServices creator, IUserProfileNamedPipeFactory pipeFactory, string input, bool isValidParse, bool isValidAccount, bool isExistingAccount, int serverTimeOut, int clientTimeOut) {
            var testDone = new ManualResetEventSlim(false);
            Task.Run(async () => {
                try {
                    if (isValidParse) {
                        Func<Task> f = async () => await RUserProfileServicesHelper.CreateProfileAsync(serverTimeOutms: serverTimeOut, clientTimeOutms: clientTimeOut, userProfileService: creator, pipeFactory: pipeFactory);
                        f.ShouldNotThrow();
                    } else {
                        Func<Task> f = () => RUserProfileServicesHelper.CreateProfileAsync(serverTimeOutms: serverTimeOut, clientTimeOutms: clientTimeOut, userProfileService: creator, pipeFactory: pipeFactory);
                        await f.ShouldThrowAsync<Exception>();
                    }
                } finally {
                    testDone.Set();
                }
            }).DoNotWait();

            using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(clientTimeOut))) {
                var result = await CreateProfileClientTestWorkerAsync(input, cts.Token);
                if (isValidParse) {
                    result.Error.Should().Be((uint)(isValidAccount ? 0 : 13));
                    result.ProfileExists.Should().Be(isExistingAccount);
                } else {
                    result.Should().BeNull();
                }
            }

            testDone.Wait(serverTimeOut + clientTimeOut);
        }

        private async Task CreateProfileFuzzTestRunnerAsync(IUserProfileServices creator, IUserProfileNamedPipeFactory pipeFactory, string input, int serverTimeOut, int clientTimeOut) {
            var task = Task.Run(async () => {
                try {
                    await RUserProfileServicesHelper.CreateProfileAsync(serverTimeOutms: serverTimeOut, clientTimeOutms: clientTimeOut, userProfileService: creator, pipeFactory: pipeFactory);
                } catch (JsonReaderException) {
                    // expecting JSON parsing to fail
                    // JSON parsing may fail due to randomly generated strings as input.
                }
            });

            using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(clientTimeOut))) {
                var result = await CreateProfileClientTestWorkerAsync(input, cts.Token);
                // fuzz test parsing succeeded, the creator always fails for this test.
                result?.Error.Should().Be(13);
            }

            await ParallelTools.When(task, serverTimeOut + clientTimeOut);
        }

        [CompositeTest]
        // valid test data
        [InlineData("{\"Username\":\"testname\", \"Domain\":\"testdomain\", \"Sid\":\"testSid\"}", "testname", "testdomain", "testSid", true, true, false)]
        // Missing quote
        [InlineData("{Username\":\"testname\", \"Domain\":\"testdomain\", \"Sid\":\"testSid\"}", null, null, null, false, false, false)]
        // Missing closing parenthesis
        [InlineData("{", null, null, null, false, false, false)]
        // empty json object
        [InlineData("{}", null, null, null, false, false, false)]
        // No username, domain and sid
        [InlineData("{\"Username\":, \"Domain\":, \"Sid\":}", null, null, null, false, false, false)]
        // empty username domain sid
        [InlineData("{\"Username\": \"\", \"Domain\": \"\", \"Sid\": \"\"}", "", "", "", true, false, false)]
        // whitespace input string
        [InlineData("                     ", null, null, null, true, false, false)]
        // empty string
        [InlineData("", null, null, null, false, false, false)]
        public async Task CreateProfileTest(string input, string username, string domain, string sid, bool isValidParse, bool isValidAccount, bool isExistingAccount) {
            var creator = UserProfileServiceMock.Create(username, domain, sid, isValidParse, isValidAccount, isExistingAccount);
            var pipeFactory = new UserProfileTestNamedPipeTestStreamFactory();
            await CreateProfileTestRunnerAsync(creator, pipeFactory, input, isValidParse, isValidAccount, isExistingAccount, 500, 500);
        }

        [Test]
        [Category.FuzzTest]
        public async Task CreateProfileFuzzTest() {
            var inner = "\"Username\": {0}, \"Domain\": {1}, \"Sid\":{2}";
            for (var i = 0; i < 100000; ++i) {

                var usernameBytes = GenerateBytes();
                var domainBytes = GenerateBytes();
                var sidBytes = GenerateBytes();

                var username = Encoding.Unicode.GetString(usernameBytes);
                var domain = Encoding.Unicode.GetString(domainBytes);
                var sid = Encoding.Unicode.GetString(sidBytes);

                var json = "{" + string.Format(inner, username, domain, sid) + "}";
                
                var testResult = string.Empty;
                var creator = new UserProfileServiceFuzzTestMock();
                var pipeFactory = new UserProfileTestNamedPipeTestStreamFactory();
                try {
                    await CreateProfileFuzzTestRunnerAsync(creator, pipeFactory, json, 100, 100);
                } catch (IOException) {
                    // expect pipe to fail. The client side pipe throws an IOException when the server side pipe 
                    // closes due to IO error or attempt to access unauthorized memory.
                } catch (TaskCanceledException) {
                } catch (OperationCanceledException) {
                }
            }
        }

        private byte[] GenerateBytes() {
            var rd = new Random();
            var data = new byte[rd.Next(0, 1024)];
            rd.NextBytes(data);
            return data;
        }
    }
}
