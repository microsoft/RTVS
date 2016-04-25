// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.R.Components.Settings.Mirrors;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    public class CranMirrorListTest {
        [Test]
        [Category.R.Package]
        public void CranMirrorList_DownloadTest() {
            ManualResetEventSlim evt = new ManualResetEventSlim();
            int eventCount = 0;

            CranMirrorList.DownloadComplete += (e, args) => {
                eventCount++;
                CranMirrorList.MirrorNames.Should().NotBeEmpty();
                CranMirrorList.MirrorUrls.Should().NotBeEmpty();
                CranMirrorList.UrlFromName(null).Should().Be(null);
                evt.Set();
            };

            CranMirrorList.Download();
            evt.Wait(10000);
            eventCount.Should().Be(1);

            CranMirrorList.Download();
            eventCount.Should().Be(2);
        }
    }
}
