using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.ContentType;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public sealed class ReplHistoryTest {
        [Test]
        [Category.Repl]
        public async Task HistoryTest01() {
            using (new VsRHostScript()) {
                var workflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
                var workflow = workflowProvider.GetOrCreate();
                var history = workflow.History;
                var session = workflow.RSession;
                var componentFactory = new InteractiveWindowComponentFactoryMock();
                using (await workflow.CreateInteractiveWindowAsync(componentFactory)) {
                    workflow.ActiveWindow.Should().NotBeNull();
                    session.IsHostRunning.Should().BeTrue();

                    var eval = workflow.ActiveWindow.InteractiveWindow.Evaluator;
                    var result = await eval.ExecuteCodeAsync("x <- c(1:10)");
                    result.Should().Be(ExecutionResult.Success);
                    history.HasEntries.Should().BeTrue();
                    history.HasSelectedEntries.Should().BeFalse();
                    history.SelectHistoryEntry(0);
                    history.HasSelectedEntries.Should().BeTrue();


                    var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
                    var tv = new WpfTextViewMock(tb);

                    tb.Clear();
                    history.SendSelectedToTextView(tv);
                    string text = tb.CurrentSnapshot.GetText();
                    text.Should().Be("x <- c(1:10)");
                }
            }
        }

        [Test]
        [Category.Repl]
        public async Task HistoryTest02() {
            VsRHostScript hostscript = null;

            try {
                hostscript = new VsRHostScript();
                var workflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
                var workflow = workflowProvider.GetOrCreate();
                var history = workflow.History;
                var session = workflow.RSession;
                var componentFactory = new InteractiveWindowComponentFactoryMock();

                using (await workflow.CreateInteractiveWindowAsync(componentFactory)) {
                    workflow.ActiveWindow.Should().NotBeNull();
                    session.IsHostRunning.Should().BeTrue();

                    var textEditorFactory = VsAppShell.Current.ExportProvider.GetExportedValue<ITextEditorFactoryService>();
                    var visualComponentFactoryMock = new RHistoryWindowVisualComponentFactoryMock(textEditorFactory);
                    history.CreateVisualComponent(visualComponentFactoryMock);
                    history.VisualComponent.Should().NotBeNull();

                    var eval = workflow.ActiveWindow.InteractiveWindow.Evaluator;
                    var historyTextBuffer = history.VisualComponent.TextView.TextBuffer;
                    var interactiveWindowTextBuffer = workflow.ActiveWindow.InteractiveWindow.CurrentLanguageBuffer;

                    var result = await eval.ExecuteCodeAsync("x <- c(1:10)");
                    result.Should().Be(ExecutionResult.Success);

                    await eval.ExecuteCodeAsync("\r\n");

                    result = await eval.ExecuteCodeAsync("x <- c(1:20)");
                    result.Should().Be(ExecutionResult.Success);

                    history.HasEntries.Should().BeTrue();
                    history.HasSelectedEntries.Should().BeFalse();

                    int eventCount = 0;
                    history.SelectionChanged += (s, e) => {
                        eventCount++;
                    };

                    history.SelectHistoryEntry(1);
                    history.HasSelectedEntries.Should().BeTrue();
                    eventCount.Should().Be(1);

                    history.SelectPreviousHistoryEntry();
                    eventCount.Should().Be(3); // event fires twice?
                    historyTextBuffer.CurrentSnapshot.GetText().Should().Be("x <- c(1:10)");

                    history.SelectNextHistoryEntry();
                    eventCount.Should().Be(5);
                    historyTextBuffer.CurrentSnapshot.GetText().Should().Be("x <- c(1:20)");

                    history.PreviousEntry();
                    interactiveWindowTextBuffer.CurrentSnapshot.GetText().Should().Be("x <- c(1:20)");

                    history.PreviousEntry();
                    interactiveWindowTextBuffer.CurrentSnapshot.GetText().Should().Be("x <- c(1:10)");

                    history.NextEntry();
                    interactiveWindowTextBuffer.CurrentSnapshot.GetText().Should().Be("x <- c(1:20)");

                    history.SelectHistoryEntries(new[] {0, 1});
                    historyTextBuffer.CurrentSnapshot.GetText().Should().Be("x <- c(1:10)\r\nx <- c(1:20)");
                    eventCount.Should().Be(6);

                    history.ToggleHistoryEntrySelection(1);
                    historyTextBuffer.CurrentSnapshot.GetText().Should().Be("x <- c(1:10)");
                    eventCount.Should().Be(7);

                    history.DeselectHistoryEntry(0);
                    history.HasSelectedEntries.Should().BeFalse();
                    eventCount.Should().Be(8);

                    history.SelectAllEntries();
                    history.HasSelectedEntries.Should().BeTrue();
                    string text = history.GetSelectedText();
                    text.Should().Be("x <- c(1:10)\r\nx <- c(1:20)");

                    var spans = history.GetSelectedHistoryEntrySpans();
                    spans.Count.Should().Be(1);

                    spans[0].Start.Position.Should().Be(0);
                    spans[0].End.Position.Should().Be(26);

                    history.DeselectHistoryEntry(0);
                    history.DeselectHistoryEntry(1);
                    history.HasSelectedEntries.Should().BeFalse();

                    history.SelectAllEntries();
                    history.ToggleHistoryEntrySelection(1);

                    history.DeleteSelectedHistoryEntries();
                    history.SelectAllEntries();

                    text = history.GetSelectedText();
                    text.Should().Be("x <- c(1:20)");

                    history.DeleteAllHistoryEntries();

                    history.HasEntries.Should().BeFalse();
                    history.HasSelectedEntries.Should().BeFalse();

                    text = history.GetSelectedText();
                    text.Should().Be(string.Empty);
                }
            } finally {
                hostscript?.Dispose();
            }
        }
    }
}
