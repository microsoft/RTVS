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
        private const string SameAsInput = "<INPUT>";

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
        [InlineData("NULL", "null")]
        [InlineData("NA", "null")]
        [InlineData("NA_integer_", "null")]
        [InlineData("NA_real_", "null")]
        [InlineData("NA_character_", "null")]
        [InlineData("TRUE[FALSE]", "null")]
        [InlineData("0[FALSE]", "null")]
        [InlineData("0L[FALSE]", "null")]
        [InlineData("''[FALSE]", "null")]
        [InlineData("TRUE", "true")]
        [InlineData("FALSE", "false")]
        [InlineData("0", "0")]
        [InlineData("0L", "0")]
        [InlineData(@"""text""", SameAsInput)]
        [InlineData(@"""\r\n\t\f\""""", SameAsInput)]
        [InlineData(@"""\a\v""", @"""\u0007\u000b""")]
        [InlineData(@"""Ûñïçôδè ƭèжƭ: русский ελληνικά ქართული հայերեն हिन्दी ግዕዝ ᓀᐦᐃᔭᐍᐏᐣ 汉语/漢語 日本語 한국어/조선말 العَرَبِية‎ עברית""", SameAsInput)]
        [InlineData(@"""‘’“”""", SameAsInput)]
        [InlineData("list()", "[]")]
        [InlineData("list(0, 's', NULL)", @"[0,""s"",null]")]
        [InlineData("list(NA, 1, NA, 2, NA)", "[1,2]")]
        [InlineData("structure(list(), names = ''[FALSE])", "{}")]
        [InlineData("list(n = 0, s = 's', u = NULL)", @"{""n"":0,""s"":""s"",""u"":null}")]
        [InlineData("list(n = 0, na = NA)", @"{""n"":0}")]
        [InlineData("list(n = 0, na = NA)", @"{""n"":0}")]
        [InlineData("as.environment(list())", "{}")]
        [InlineData("as.environment(list(n = 0, s = 's', u = NULL))", @"{""n"":0,""s"":""s"",""u"":null}")]
        [InlineData("as.environment(list(n = 0, na = NA))", @"{""n"":0}")]
        [InlineData("as.environment(list(n = 0, na = NA))", @"{""n"":0}")]
        [InlineData("list(as.environment(list(l = list())))", @"[{""l"":[]}]")]
        public async Task Serialize(string expr, string json) {
            if (json == SameAsInput) {
                json = expr;
            }

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
