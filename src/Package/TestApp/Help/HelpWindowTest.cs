// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.R.Components.Help;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.Test;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.R.Packages.R;
using mshtml;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    [ExcludeFromCodeCoverage]
    [Category.Interactive]
    [Collection(CollectionNames.NonParallel)]
    public class HelpWindowTest : HostBasedInteractiveTest {
        private const string darkThemeCssColor = "rgb(36,36,36)";

        public HelpWindowTest() : base(true) { }

        [Test]
        public async Task HelpTest() {
            var clientApp = new RHostClientHelpTestApp();
            await HostScript.InitializeAsync(clientApp);
            using (var script = new ControlTestScript(typeof(HelpVisualComponent))) {
                DoIdle(100);

                var component = ControlWindow.Component as IHelpVisualComponent;
                component.Should().NotBeNull();

                component.VisualTheme = "Dark.css";
                await UIThreadHelper.Instance.InvokeAsync(() => {
                    clientApp.Component = component;
                });

                await ShowHelpAsync("plot", HostScript, clientApp);

                clientApp.Uri.IsLoopback.Should().Be(true);
                clientApp.Uri.PathAndQuery.Should().Be("/library/graphics/html/plot.html");
                (await GetBackgroundColorAsync(component, clientApp)).Should().Be(darkThemeCssColor);

                component.VisualTheme = "Light.css";
                await ShowHelpAsync("lm", HostScript, clientApp);
                clientApp.Uri.PathAndQuery.Should().Be("/library/stats/html/lm.html");

                (await GetBackgroundColorAsync(component, clientApp)).Should().Be("white");

                await ExecCommandAsync(clientApp, RPackageCommandId.icmdHelpPrevious);
                clientApp.Uri.PathAndQuery.Should().Be("/library/graphics/html/plot.html");

                await ExecCommandAsync(clientApp, RPackageCommandId.icmdHelpNext);
                clientApp.Uri.PathAndQuery.Should().Be("/library/stats/html/lm.html");

                await ExecCommandAsync(clientApp, RPackageCommandId.icmdHelpHome);
                clientApp.Uri.PathAndQuery.Should().Be("/doc/html/index.html");
            }
        }

        private async Task ShowHelpAsync(string command, VsRHostScript hostScript, RHostClientHelpTestApp clientApp) {
            clientApp.Reset();
            await hostScript.Session.ExecuteAsync($"rtvs:::show_help({command.ToRStringLiteral()})").SilenceException<RException>();
            await clientApp.WaitForReadyAndRenderedAsync((ms) => DoIdle(ms), nameof(HelpTest));
        }

        private async Task ExecCommandAsync(RHostClientHelpTestApp clientApp, int commandId) {
            clientApp.Reset();
            await UIThreadHelper.Instance.InvokeAsync(() => {
                object o = new object();
                clientApp.Component.Controller.Invoke(RGuidList.RCmdSetGuid, commandId, null, ref o);
            });
            await clientApp.WaitForReadyAndRenderedAsync((ms) => DoIdle(ms), nameof(HelpTest));
        }

        private async Task<string> GetBackgroundColorAsync(IHelpVisualComponent component, RHostClientHelpTestApp clientApp) {
            string color = "red";

            await clientApp.WaitForReadyAndRenderedAsync((ms) => DoIdle(ms), nameof(HelpTest));
            await UIThreadHelper.Instance.InvokeAsync(() => {
                IHTMLElement2 body = component.Browser.Document.Body.DomElement as IHTMLElement2;
                color = body.currentStyle.backgroundColor as string;
            });

            return color;
        }
    }
}
