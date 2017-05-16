// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Controls;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.Help.Commands;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.Test;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using mshtml;
using Microsoft.R.Components.InteractiveWorkflow;
using Xunit;

namespace Microsoft.VisualStudio.R.Interactive.Test.Help {
    [ExcludeFromCodeCoverage]
    [Category.Interactive]
    [Collection(CollectionNames.NonParallel)]
    public class HelpWindowTest : HostBasedInteractiveTest {
        private const string darkThemeCssColor = "rgb(36,36,36)";
        private RHostClientHelpTestApp _clientApp;
        private VsRHostScript _hostScript;

        public HelpWindowTest(IServiceContainer services) : base(services, true) {
            _hostScript = GetScript<VsRHostScript>();
        }

        public override async Task InitializeAsync() {
            _clientApp = new RHostClientHelpTestApp();
            await _hostScript.InitializeAsync(_clientApp);
        }

        [Test]
        public async Task HelpTest() {
            using (new ControlTestScript(typeof(HelpVisualComponent), Services)) {
                DoIdle(100);

                var component = ControlWindow.Component as IHelpVisualComponent;
                component.Should().NotBeNull();

                component.VisualTheme = "Dark.css";
                await UIThreadHelper.Instance.InvokeAsync(() => {
                    _clientApp.Component = component;
                });

                await ShowHelpAsync("plot", _hostScript, _clientApp);

                _clientApp.Uri.IsLoopback.Should().Be(true);
                _clientApp.Uri.PathAndQuery.Should().Be("/library/graphics/html/plot.html");
                (await GetBackgroundColorAsync(component, _clientApp)).Should().Be(darkThemeCssColor);

                component.VisualTheme = "Light.css";
                await ShowHelpAsync("lm", _hostScript, _clientApp);
                _clientApp.Uri.PathAndQuery.Should().Be("/library/stats/html/lm.html");

                (await GetBackgroundColorAsync(component, _clientApp)).Should().Be("white");

                await ExecCommandAsync(_clientApp, new HelpPreviousCommand(component));
                _clientApp.Uri.PathAndQuery.Should().Be("/library/graphics/html/plot.html");

                await ExecCommandAsync(_clientApp, new HelpNextCommand(component));
                _clientApp.Uri.PathAndQuery.Should().Be("/library/stats/html/lm.html");

                await ExecCommandAsync(_clientApp, new HelpHomeCommand(Services));
                _clientApp.Uri.PathAndQuery.Should().Be("/doc/html/index.html");
            }
        }

        private async Task ShowHelpAsync(string command, VsRHostScript hostScript, RHostClientHelpTestApp clientApp) {
            clientApp.Reset();
            await hostScript.Session.ExecuteAsync($"rtvs:::show_help({command.ToRStringLiteral()})").SilenceException<RException>();
            await clientApp.WaitForReadyAndRenderedAsync(DoIdle, nameof(HelpTest));
        }

        private async Task ExecCommandAsync(RHostClientHelpTestApp clientApp, IAsyncCommand command) {
            clientApp.Reset();
            await UIThreadTools.InUI(command.InvokeAsync);
            await clientApp.WaitForReadyAndRenderedAsync(DoIdle, nameof(HelpTest));
        }

        private async Task<string> GetBackgroundColorAsync(IHelpVisualComponent component, RHostClientHelpTestApp clientApp) {
            var color = "red";

            await clientApp.WaitForReadyAndRenderedAsync(DoIdle, nameof(HelpTest));
            await UIThreadHelper.Instance.InvokeAsync(() => {
                var body = component.Browser.Document.Body.DomElement as IHTMLElement2;
                color = body.currentStyle.backgroundColor as string;
            });

            return color;
        }
    }
}
