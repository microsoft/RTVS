// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.R.Packages.R;
using mshtml;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class HelpWindowTest : InteractiveTest {
        private const string darkThemeCssColor = "rgb(36,36,36)";

        [Test]
        [Category.Interactive]
        public void HelpTest() {
            var clientApp = new RHostClientHelpTestApp();
            using (var hostScript = new VsRHostScript(clientApp)) {
                using (var script = new ControlTestScript(typeof(HelpWindowVisualComponent))) {
                    DoIdle(100);

                    var component = ControlWindow.Component as IHelpWindowVisualComponent;
                    component.Should().NotBeNull();

                    component.VisualTheme = "Dark.css";
                    clientApp.Component = component;

                    ShowHelp("?plot\n", hostScript, clientApp);
                    clientApp.Uri.IsLoopback.Should().Be(true);
                    clientApp.Uri.PathAndQuery.Should().Be("/library/graphics/html/plot.html");

                    GetBackgroundColor(component.Browser).Should().Be(darkThemeCssColor);

                    UIThreadHelper.Instance.Invoke(() => {
                        component.Browser.Refresh();
                        WaitForDocumentComplete(component.Browser);
                    });
                    GetBackgroundColor(component.Browser).Should().Be(darkThemeCssColor);

                    component.VisualTheme = "Light.css";
                    ShowHelp("?lm\n", hostScript, clientApp);
                    clientApp.Uri.PathAndQuery.Should().Be("/library/stats/html/lm.html");

                    GetBackgroundColor(component.Browser).Should().Be("white");

                    ExecCommand(clientApp, RPackageCommandId.icmdHelpPrevious);
                    clientApp.Uri.PathAndQuery.Should().Be("/library/graphics/html/plot.html");

                    ExecCommand(clientApp, RPackageCommandId.icmdHelpNext);
                    clientApp.Uri.PathAndQuery.Should().Be("/library/stats/html/lm.html");

                    ExecCommand(clientApp, RPackageCommandId.icmdHelpHome);
                    clientApp.Uri.PathAndQuery.Should().Be("/doc/html/index.html");

                }
            }
        }

        private void ShowHelp(string command, VsRHostScript hostScript, RHostClientHelpTestApp clientApp) {
            clientApp.Ready = false;
            using (var request = hostScript.Session.BeginInteractionAsync().Result) {
                request.RespondAsync(command).SilenceException<RException>();
            }
            WaitForAppReady(clientApp);
        }

        private void ExecCommand(RHostClientHelpTestApp clientApp, int commandId) {
            UIThreadHelper.Instance.Invoke(() => {
                clientApp.Ready = false;
                object o = new object();
                clientApp.Component.Controller.Invoke(RGuidList.RCmdSetGuid, commandId, null, ref o);
            });
            WaitForAppReady(clientApp);
        }

        private void WaitForAppReady(RHostClientHelpTestApp clientApp) {
            for (int i = 0; i < 100 && !clientApp.Ready; i++) {
                DoIdle(200);
            }
        }

        private void WaitForDocumentComplete(WebBrowser wb) {
            for (int i = 0; i < 100 && wb.ReadyState != WebBrowserReadyState.Loading; i++) {
                DoIdle(50);
            }
            for (int i = 0; i < 100 && wb.ReadyState != WebBrowserReadyState.Complete; i++) {
                DoIdle(50);
            }
        }

        private string GetBackgroundColor(WebBrowser browser) {
            string color = "red";
            UIThreadHelper.Instance.Invoke(() => {
                IHTMLElement2 body = browser.Document.Body.DomElement as IHTMLElement2;
                color = body.currentStyle.backgroundColor as string;
            });
            return color;
        }
    }
}
