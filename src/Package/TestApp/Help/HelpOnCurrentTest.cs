// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.Test;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    [ExcludeFromCodeCoverage]
    [Category.Interactive]
    [Collection(CollectionNames.NonParallel)]
    public class HelpOnCurrentTest : HostBasedInteractiveTest {
        public HelpOnCurrentTest(IServiceContainer services): base(services) { }

        [Test]
        public async Task HelpTest() {
            var clientApp = new RHostClientHelpTestApp();
            await HostScript.InitializeAsync(clientApp);
            using (new ControlTestScript(typeof(HelpVisualComponent), Services)) {
                DoIdle(100);

                var activeViewTrackerMock = new ActiveTextViewTrackerMock("  plot", RContentTypeDefinition.ContentType);
                var activeReplTrackerMock = new ActiveRInteractiveWindowTrackerMock();

                var interactiveWorkflow = Substitute.For<IRInteractiveWorkflow>();
                interactiveWorkflow.RSession.Returns(HostScript.Session);

                var component = ControlWindow.Component as IHelpVisualComponent;
                component.Should().NotBeNull();

                component.VisualTheme = "Light.css";
                await UIThreadHelper.Instance.InvokeAsync(() => {
                    clientApp.Component = component;

                    var view = activeViewTrackerMock.GetLastActiveTextView(RContentTypeDefinition.ContentType);
                    var cmd = new ShowHelpOnCurrentCommand(interactiveWorkflow, activeViewTrackerMock, activeReplTrackerMock);
                    cmd.Should().BeVisibleAndDisabled();

                    view.Caret.MoveTo(new SnapshotPoint(view.TextBuffer.CurrentSnapshot, 3));

                    cmd.Should().BeVisibleAndEnabled();
                    cmd.Text.Should().EndWith("plot");

                    clientApp.Ready.Reset();
                    cmd.Invoke();
                });

                await clientApp.WaitForReadyAndRenderedAsync((ms) => DoIdle(ms), nameof(HelpTest));

                clientApp.Uri.IsLoopback.Should().Be(true);
                clientApp.Uri.PathAndQuery.Should().Be("/library/graphics/html/plot.html");
            }
        }
    }
}
