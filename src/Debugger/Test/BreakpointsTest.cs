using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    public class BreakpointsTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly RSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public BreakpointsTest(TestMethodInfoFixture testMethodInfo) {
            _testMethod = testMethodInfo.Method;
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
        public async Task SetRemoveBreakpoint() {
            using (var debugSession = new DebugSession(_session)) {
                var content =
@"x <- 1
y <- 2
z <- 3
";
                using (var sf = new SourceFile(content)) {
                    var bpl1 = new DebugBreakpointLocation(sf.FilePath, 1);
                    DebugBreakpoint bp1 = await debugSession.CreateBreakpointAsync(bpl1);

                    bp1.Location.Should().Be(bpl1);
                    bp1.Session.Should().Be(debugSession);

                    debugSession.Breakpoints.Count.Should().Be(1);

                    var bpl2 = new DebugBreakpointLocation(sf.FilePath, 3);
                    DebugBreakpoint bp2 = await debugSession.CreateBreakpointAsync(bpl2);

                    bp2.Location.Should().Be(bpl2);
                    bp2.Session.Should().Be(debugSession);

                    debugSession.Breakpoints.Count.Should().Be(2);

                    await bp1.DeleteAsync();
                    debugSession.Breakpoints.Count.Should().Be(1);
                }
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task HitBreakpoint() {
            using (var debugSession = new DebugSession(_session)) {
                string content =
@"x <- 1
y <- 2
z <- 3
";
                using (var sf = new SourceFile(content)) {
                    await debugSession.EnableBreakpointsAsync(true);

                    var bpl = new DebugBreakpointLocation(sf.FilePath, 2);
                    DebugBreakpoint bp = await debugSession.CreateBreakpointAsync(bpl);

                    int eventCount = 0;
                    bp.BreakpointHit += (s, e) => {
                        eventCount++;
                    };

                    await sf.Source(_session);

                    // Allow pending thread transitions and async/awaits to complete
                    EventsPump.DoEvents(3000);

                    eventCount.Should().Be(1);
                }
            }
        }
    }
}
