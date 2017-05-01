// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.DataInspection;
using Microsoft.R.ExecutionTracing;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using NSubstitute;
using Xunit;

namespace Microsoft.R.StackTracing.Test {
    [ExcludeFromCodeCoverage]
    public class CallStackTest : IAsyncLifetime {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public CallStackTest(IServiceContainer services, TestMethodFixture testMethod) {
            _sessionProvider = new RSessionProvider(services);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(nameof(CallStackTest));
            await _session.StartHostAsync(new RHostStartupInfo(isInteractive: true), new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [Test]
        [Category.R.StackTracing]
        public async Task CallStack() {
            var tracer = await _session.TraceExecutionAsync();
            const string code1 =
@"f <- function(n) {
     if (n > 0) {
        g(n - 1)
     } else {
        return()
     }
  }";

            const string code2 =
@"g <- function(n) {
     if (n > 0) {
        f(n - 1)
     } else {
        return()
     }
  }";

            using (var sf1 = new SourceFile(code1))
            using (var sf2 = new SourceFile(code2)) {
                await tracer.EnableBreakpointsAsync(true);

                await sf1.Source(_session);
                await sf2.Source(_session);

                var bp = await tracer.CreateBreakpointAsync(sf1, 5);
                var bpHit = new BreakpointHitDetector(bp);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f(4)\n");
                }
                await bpHit.ShouldBeHitAtNextPromptAsync();

                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { null, null, "f(4)", "<environment: R_GlobalEnv>" },
                    { sf1, 3, "g(n - 1)", null },
                    { sf2, 3, "f(n - 1)", null },
                    { sf1, 3, "g(n - 1)", null },
                    { sf2, 3, "f(n - 1)", null },
                    { sf1, 5, TracebackBuilder.Any, null }
                });
            }
        }

        [Test]
        [Category.R.StackTracing]
        public async Task FrameChildren() {
            var tracer = await _session.TraceExecutionAsync();
            _session.IsHostRunning.Should().BeTrue(because: "Host is not running.");

            using (var sf = new SourceFile("x <- 1; y <- 2; browser()")) {
                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                var frame = (await _session.TracebackAsync()).Last();
                var frameChildren = await frame.DescribeChildrenAsync(REvaluationResultProperties.None, null);

                var frameEnv = await frame.DescribeEnvironmentAsync();
                var frameEnvChildren = await frameEnv.DescribeChildrenAsync(REvaluationResultProperties.None, null);

                frameEnv.Length.Should().Be(2);
                frameChildren.Should().HaveCount(2);
                frameChildren.Should().Contain(info => info.Name == "x");
                frameChildren.Should().Contain(info => info.Name == "y");
                frameChildren.ShouldAllBeEquivalentTo(frameEnvChildren, options => options.WithStrictOrdering());
            }
        }

        [CompositeTest]
        [Category.R.StackTracing]
        [InlineData(false)]
        [InlineData(true)]
        public async Task HideSourceFrames(bool debug) {
            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile("0")) {
                await tracer.EnableBreakpointsAsync(true);

                var bp = await tracer.CreateBreakpointAsync(sf, 1);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session, debug);
                await bpHit.ShouldBeHitAtNextPromptAsync();

                var frame = (await _session.TracebackAsync()).Single();
                frame.IsGlobal.Should().BeTrue();
            }
        }

        [Test]
        [Category.R.StackTracing]
        public async Task EnvironmentNames() {
            const string code =
    @"f <- function() eval(quote(browser()), .GlobalEnv)
  g <- function(f) eval(as.call(list(f)), getNamespace('utils'))
  h <- function() eval(as.call(list(g, f)), as.environment('package:utils'))
  h()";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                var funcFrame = Substitute.For<IRStackFrame>();
                funcFrame.IsGlobal.Returns(false);
                funcFrame.EnvironmentName.Returns((string)null);

                var globalEnv = Substitute.For<IRStackFrame>();
                globalEnv.IsGlobal.Returns(true);
                globalEnv.EnvironmentName.Returns("<environment: R_GlobalEnv>");

                var packageUtils = Substitute.For<IRStackFrame>();
                packageUtils.IsGlobal.Returns(false);
                packageUtils.EnvironmentName.Returns("<environment: package:utils>");

                var namespaceUtils = Substitute.For<IRStackFrame>();
                namespaceUtils.IsGlobal.Returns(false);
                namespaceUtils.EnvironmentName.Returns("<environment: namespace:utils>");

                var expectedFrames = new[] {
                    globalEnv,
                    funcFrame, // h
                    funcFrame, // eval
                    packageUtils,
                    funcFrame, // g
                    funcFrame, // eval
                    namespaceUtils,
                    funcFrame, // f
                    funcFrame, // eval
                    globalEnv
                };

                var actualFrames = await _session.TracebackAsync();

                actualFrames.ShouldAllBeEquivalentTo(expectedFrames, options => options
                    .Including(frame => frame.IsGlobal)
                    .Including(frame => frame.EnvironmentName)
                    .WithStrictOrdering());
            }
        }
    }
}
