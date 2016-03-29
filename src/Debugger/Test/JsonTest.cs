// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Newtonsoft.Json;
using Xunit;
using static System.FormattableString;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    public class JsonTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public JsonTest(TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _sessionProvider = new RSessionProvider();
            _session = _sessionProvider.GetOrCreate(Guid.NewGuid(), new RHostClientTestApp());
        }

        public async Task InitializeAsync() {
            await _session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = RUtilities.FindExistingRBasePath()
            }, 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [CompositeTest]
        [Category.R.Debugger]
        [InlineData(@"'‘’“”'", @"""‘’“”""")]
        public async Task Serialize(string expr, string json) {
            using (var eval = await _session.BeginEvaluationAsync()) {
                var res = await eval.EvaluateAsync(expr, REvaluationKind.Json);
                res.Error.Should().BeNullOrEmpty();
                res.JsonResult.Should().NotBeNull();
                var actualJson = JsonConvert.SerializeObject(res.JsonResult);
                actualJson.Should().Be(json);
            }
        }
    }
}
