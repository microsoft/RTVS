// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.OS;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Browsers;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    public class WebBrowserServicesTest {
        private const string _url = "microsoft.com";

        [Test]
        [Category.R.Package]
        public void ExternaBrowser() {
            var vswbs = Substitute.For<IVsWebBrowsingService>();
            var ps = Substitute.For<IProcessServices>();
            var settings = Substitute.For<IRToolsSettings>();
            
            var wbs = new WebBrowserServices(vswbs, ps, settings);
            wbs.OpenBrowser(WebBrowserRole.External, _url);
            ps.Received().Start(_url);
        }

        [Test]
        [Category.R.Package]
        public void WebHelpBrowser() {
            var externalSettings = Substitute.For<IRToolsSettings>();
            externalSettings.WebHelpSearchBrowserType.Returns(BrowserType.External);

            var internalSettings = Substitute.For<IRToolsSettings>();
            internalSettings.WebHelpSearchBrowserType.Returns(BrowserType.Internal);

            RunBrowserTest(WebBrowserRole.Help, RGuidList.WebHelpWindowGuid, Resources.WebHelpWindowTitle, externalSettings, internalSettings);
        }

        [Test]
        [Category.R.Package]
        public void ShinyBrowser() {
            var externalSettings = Substitute.For<IRToolsSettings>();
            externalSettings.HtmlBrowserType.Returns(BrowserType.External);

            var internalSettings = Substitute.For<IRToolsSettings>();
            internalSettings.HtmlBrowserType.Returns(BrowserType.Internal);

            RunBrowserTest(WebBrowserRole.Shiny, RGuidList.ShinyWindowGuid, Resources.ShinyWindowTitle, externalSettings, internalSettings);
        }

        public void RunBrowserTest(WebBrowserRole role, Guid guid, string title, IRToolsSettings externalSettings, IRToolsSettings internalSettings) {
            var vswbs = Substitute.For<IVsWebBrowsingService>();
            var ps = Substitute.For<IProcessServices>();

            var wbs = new WebBrowserServices(vswbs, ps, externalSettings);
            wbs.OpenBrowser(role, _url);
            ps.Received().Start(_url);

            ps.ClearReceivedCalls();
            wbs = new WebBrowserServices(vswbs, ps, internalSettings);
            wbs.OpenBrowser(role, _url);

            UIThreadHelper.Instance.DoEvents();
            ps.DidNotReceive().Start(_url);

            IVsWebBrowser vswb;
            IVsWindowFrame frame;
            vswbs.Received().CreateWebBrowser(Arg.Any<uint>(), guid, title, _url, null, out vswb, out frame);
        }
    }
}
