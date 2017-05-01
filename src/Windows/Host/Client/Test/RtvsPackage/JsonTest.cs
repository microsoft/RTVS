// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.R.RtvsPackage.Test {
    [ExcludeFromCodeCoverage]
    public class JsonTest : IAsyncLifetime {
        private const string SameAsInput = "<INPUT>";

        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public JsonTest(IServiceContainer services, TestMethodFixture testMethod) {
            _sessionProvider = new RSessionProvider(services);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(nameof(JsonTest));
            await _session.StartHostAsync(new RHostStartupInfo(), new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [CompositeTest]
        [Category.R.RtvsPackage]
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
        [InlineData("as.environment(list())", "{}")]
        [InlineData("as.environment(list(n = 0, s = 's', u = NULL))", @"{""n"":0,""s"":""s"",""u"":null}")]
        [InlineData("as.environment(list(n = 0, na = NA))", @"{""n"":0}")]
        [InlineData("list(as.environment(list(l = list())))", @"[{""l"":[]}]")]
        public async Task Serialize(string expr, string json) {
            if (json == SameAsInput) {
                json = expr;
            }

            var res = await _session.EvaluateAsync(expr, REvaluationKind.Normal);
            res.Error.Should().BeNullOrEmpty();
            res.Result.Should().NotBeNull();
            res.RawResult.Should().BeNull();
            var actualJson = JsonConvert.SerializeObject(res.Result).ToUnicodeQuotes();
            actualJson.Should().Be(json);
        }

        [CompositeTest]
        [Category.R.RtvsPackage]
        [InlineData("list('Ûñïçôdè' = 0)", @"{""Ûñïçôdè"":0}", 1252)]
        [InlineData("as.environment(list('Ûñïçôdè' = 0))", @"{""Ûñïçôdè"":0}", 1252)]
        [InlineData("list('«Юникод»' = 0)", @"{""«Юникод»"":0}", 1251)]
        [InlineData("as.environment(list('«Юникод»' = 0))", @"{""«Юникод»"":0}", 1251)]
        public async Task SerializeWithEncoding(string expr, string json, int codepage) {
            if (json == SameAsInput) {
                json = expr;
            }

            await _session.SetCodePageAsync(codepage);
            var res = await _session.EvaluateAsync(expr, REvaluationKind.Normal);
            res.Error.Should().BeNullOrEmpty();
            res.Result.Should().NotBeNull();
            res.RawResult.Should().BeNull();
            var actualJson = JsonConvert.SerializeObject(res.Result).ToUnicodeQuotes();
            actualJson.Should().Be(json);
        }

        [CompositeTest]
        [Category.R.RtvsPackage]
        [InlineData("NaN")]
        [InlineData("+Inf")]
        [InlineData("-Inf")]
        [InlineData("1i")]
        [InlineData("pairlist()")]
        [InlineData("function() {}")]
        [InlineData("quote(symbol)")]
        [InlineData("quote(1 + 2)")]
        [InlineData("c(1, 2)")]
        [InlineData("structure(list(1, 2), names = c('x', NA))")]
        [InlineData("list(x = 1, x = 2)")]
        [InlineData("as.environment(list(x = 1, x = 2))")]
        public async Task SerializeError(string expr) {
            var res = await _session.EvaluateAsync($"rtvs::toJSON({expr})", REvaluationKind.Normal);
            res.Error.Should().NotBeNullOrEmpty();
            res.RawResult.Should().BeNull();
        }
    }
}
