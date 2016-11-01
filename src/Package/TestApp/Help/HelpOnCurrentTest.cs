// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test;
using Microsoft.VisualStudio.R.Package.Test.FakeFactories;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.Text;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class HelpOnCurrentTest : HostBasedInteractiveTest {

        [Test(Skip = "https://github.com/Microsoft/RTVS/issues/1983")]
        [Category.Interactive]
        public void HelpTest() {
            var clientApp = new RHostClientHelpTestApp();
            using (new ControlTestScript(typeof(HelpVisualComponent))) {
                DoIdle(100);

                var activeViewTrackerMock = new ActiveTextViewTrackerMock("  plot", RContentTypeDefinition.ContentType);
                var activeReplTrackerMock = new ActiveRInteractiveWindowTrackerMock();
                var interactiveWorkflowProvider = TestRInteractiveWorkflowProviderFactory.Create(SessionProvider, activeTextViewTracker: activeViewTrackerMock);
                var interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();

                var component = ControlWindow.Component as IHelpVisualComponent;
                component.Should().NotBeNull();

                component.VisualTheme = "Light.css";
                clientApp.Component = component;

                var view = activeViewTrackerMock.GetLastActiveTextView(RContentTypeDefinition.ContentType);

                var cmd = new ShowHelpOnCurrentCommand(interactiveWorkflow, activeViewTrackerMock, activeReplTrackerMock);

                cmd.Should().BeVisibleAndDisabled();
                view.Caret.MoveTo(new SnapshotPoint(view.TextBuffer.CurrentSnapshot, 3));

                cmd.Should().BeVisibleAndEnabled();
                cmd.Text.Should().EndWith("plot");

                cmd.Invoke();
                WaitForAppReady(clientApp);

                clientApp.Uri.IsLoopback.Should().Be(true);
                clientApp.Uri.PathAndQuery.Should().Be("/library/graphics/html/plot.html");

                DoIdle(500);
            }
        }

        private void WaitForAppReady(RHostClientHelpTestApp clientApp) {
            for (int i = 0; i < 100 && !clientApp.Ready; i++) {
                DoIdle(200);
            }
        }
    }
}
