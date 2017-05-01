// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.OS;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Browsers;
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
    }
}
