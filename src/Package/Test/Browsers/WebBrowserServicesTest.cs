// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Components.Settings;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Browsers;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Category.R.Package]
    public class WebBrowserServicesTest {
        private const string _url = "microsoft.com";

        [Test]
        public void ExternaBrowser() {
            var shell = Substitute.For<ICoreShell>();
            var ps = Substitute.For<IProcessServices>();
            shell.Process().Returns(ps);
            
            var wbs = new WebBrowserServices(shell);
            wbs.OpenBrowser(WebBrowserRole.External, _url);
            ps.Received().Start(_url);
        }

        [Test]
        public void WebHelpBrowser() {
            var externalSettings = Substitute.For<IRSettings>();
            externalSettings.WebHelpSearchBrowserType.Returns(BrowserType.External);

            var internalSettings = Substitute.For<IRSettings>();
            internalSettings.WebHelpSearchBrowserType.Returns(BrowserType.Internal);

            RunBrowserTest(WebBrowserRole.Help, RGuidList.WebHelpWindowGuid, Resources.WebHelpWindowTitle, externalSettings, internalSettings);
        }

        [Test]
        public void ShinyBrowser() {
            var externalSettings = Substitute.For<IRSettings>();
            externalSettings.HtmlBrowserType.Returns(BrowserType.External);

            var internalSettings = Substitute.For<IRSettings>();
            internalSettings.HtmlBrowserType.Returns(BrowserType.Internal);

            RunBrowserTest(WebBrowserRole.Shiny, RGuidList.ShinyWindowGuid, Resources.ShinyWindowTitle, externalSettings, internalSettings);
        }

        public void RunBrowserTest(WebBrowserRole role, Guid guid, string title, IRSettings externalSettings, IRSettings internalSettings) {
            var shell = TestCoreShell.CreateSubstitute();
            var vswbs = Substitute.For<IVsWebBrowsingService>();
            shell.ServiceManager
                .AddService(vswbs, typeof(SVsWebBrowsingService))
                .AddService(externalSettings);
            var ps = shell.GetService<IProcessServices>();

            var wbs = new WebBrowserServices(shell);
            wbs.OpenBrowser(role, _url);
            ps.Received().Start(_url);

            ps.ClearReceivedCalls();
            shell.ServiceManager.RemoveService<IRSettings>();
            shell.ServiceManager.AddService(internalSettings);

            wbs = new WebBrowserServices(shell);
            wbs.OpenBrowser(role, _url);

            UIThreadHelper.Instance.DoEvents();
            ps.DidNotReceive().Start(_url);

            vswbs.Received().CreateWebBrowser(Arg.Any<uint>(), guid, title, _url, null, out var vswb, out var frame);
        }
    }
}
