using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class BreakpointsTest {
        [Test]
        [Category.R.Debugger]
        public async Task SetRemoveBreakpoint() {
            var sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            using (new RHostScript(sessionProvider)) {
                IRSession session = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, new RHostClientTestApp());
                using (var debugSession = new DebugSession(session)) {
                    string content =
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
        }

        [Test]
        [Category.R.Debugger]
        public async Task HitBreakpoint() {
            var sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            using (new RHostScript(sessionProvider)) {
                IRSession session = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, new RHostClientTestApp());
                using (var debugSession = new DebugSession(session)) {
                    string content =
@"x <- 1
  y <- 2
  z <- 3
";
                    using (var sf = new SourceFile(content)) {
                        await debugSession.EnableBreakpoints(true);

                        var bpl = new DebugBreakpointLocation(sf.FilePath, 2);
                        DebugBreakpoint bp = await debugSession.CreateBreakpointAsync(bpl);

                        int eventCount = 0;
                        bp.BreakpointHit += (s, e) => {
                            eventCount++;
                        };

                        await sf.Source(session);

                        // Allow pending thread transitions and async/awaits to complete
                        EventsPump.DoEvents(3000);

                        eventCount.Should().Be(1);
                    }
                }
            }
        }

        sealed class SourceFile : IDisposable {
            public string FilePath { get; }

            public SourceFile(string content) {
                FilePath = Path.GetTempFileName();
                using (var sw = new StreamWriter(FilePath)) {
                    sw.Write(content);
                }
            }

            public async Task Source(IRSession session) {
                using (IRSessionInteraction eval = await session.BeginInteractionAsync()) {
                    await eval.RespondAsync($"rtvs::debug_source({FilePath.ToRStringLiteral()})" + Environment.NewLine);
                }
            }

            public void Dispose() {
                try {
                    File.Delete(FilePath);
                } catch (IOException) { }
            }
        }
    }
}
