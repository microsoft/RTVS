// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;
using static System.FormattableString;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    public class ValuesTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public ValuesTest(TestMethodFixture testMethod) {
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

        [Test]
        [Category.R.Debugger]
        public async Task MultilinePromise() {
            const string code = @"
f <- function(p, d) {
    force(d)
    browser()
}
x <- quote({{{}}})
eval(substitute(f(P, x), list(P = x)))
";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var paused = new TaskCompletionSource<bool>();
                debugSession.Browse += delegate {
                    paused.SetResult(true);
                };

                await sf.Source(_session);
                await paused.Task;

                var stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                stackFrames.Should().NotBeEmpty();

                var evalResult = await stackFrames[0].GetEnvironmentAsync();
                evalResult.Should().BeAssignableTo<DebugValueEvaluationResult>();

                var frame = (DebugValueEvaluationResult)evalResult;
                var children = (await frame.GetChildrenAsync()).ToDictionary(er => er.Name);

                children.Should().ContainKey("p");
                children["p"].Should().BeAssignableTo<DebugPromiseEvaluationResult>();
                var p = (DebugPromiseEvaluationResult)children["p"];

                children.Should().ContainKey("d");
                children["d"].Should().BeAssignableTo<DebugValueEvaluationResult>();
                var d = (DebugValueEvaluationResult)children["d"];

                p.Code.Should().Be(d.GetRepresentation(DebugValueRepresentationKind.Raw).Deparse);
            }
        }

        [CompositeTest]
        [Category.R.Debugger]
        [InlineData("NULL", "NULL", " NULL", "")]
        [InlineData("TRUE", "TRUE", "TRUE", "TRUE")]
        [InlineData("FALSE", "FALSE", "FALSE", "FALSE")]
        [InlineData("NA", "NA", "NA", "NA")]
        [InlineData("NA_integer_", "NA_integer_", "NA", "NA")]
        [InlineData("NA_real_", "NA_real_", "NA", "NA")]
        [InlineData("NA_complex_", "NA_complex_", "NA", "NA")]
        [InlineData("NA_character_", "NA_character_", "NA", "NA")]
        [InlineData("+Inf", "Inf", "Inf", "Inf")]
        [InlineData("-Inf", "-Inf", "-Inf", "-Inf")]
        [InlineData("NaN", "NaN", "NaN", "NaN")]
        [InlineData("-NaN", "NaN", "NaN", "NaN")]
        [InlineData("+0", "0", "0", "0")]
        [InlineData("-0", "0", "0", "0")]
        [InlineData("0L", "0L", "0", "0")]
        [InlineData("42", "42", "42", "42")]
        [InlineData("42L", "42L", "42", "42")]
        [InlineData("-42", "-42", "-42", "-42")]
        [InlineData("-42L", "-42L", "-42", "-42")]
        [InlineData("42.24", "42.24", "42.2", "42.24")]
        [InlineData("-0.42", "-0.42", "-0.42", "-0.42")]
        [InlineData("4.2e10", "4.2e+10", "4.2e+10", "4.2e+10")]
        [InlineData("-4.2e10", "-4.2e+10", "-4.2e+10", "-4.2e+10")]
        [InlineData("4.2e-10", "4.2e-10", "4.2e-10", "4.2e-10")]
        [InlineData("-4.2e-10", "-4.2e-10", "-4.2e-10", "-4.2e-10")]
        [InlineData("0i", "0+0i", "0+0i", "0+0i")]
        [InlineData("4+2i", "4+2i", "4+2i", "4+2i")]
        [InlineData("4-2.4i", "4-2.4i", "4-2.4i", "4-2.4i")]
        [InlineData("-4.2+4i", "-4.2+4i", "-4.2+4i", "-4.2+4i")]
        [InlineData("-4.2-2.4i", "-4.2-2.4i", "-4.2-2.4i", "-4.2-2.4i")]
        [InlineData("-4.2e+10+2.4e-10i", "-4.2e+10+2.4e-10i", "-4.2e+10+2.4e-10i", "-4.2e+10+2.4e-10i", Skip = "https://bugs.r-project.org/bugzilla/show_bug.cgi?id=16752")]
        public async Task Representation(string expr, string deparse, string str, string toString) {
            using (var debugSession = new DebugSession(_session)) {
                var res = (await debugSession.EvaluateAsync(expr)).As<DebugValueEvaluationResult>();
                var repr = res.GetRepresentation(DebugValueRepresentationKind.Normal);

                repr.Deparse.Should().Be(deparse);
                repr.Str.Should().Be(str);
                repr.ToString.Should().Be(toString);
            }
        }

    }
}
