// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.R.Platform.Host;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Session {
    [Category.R.Session]
    public partial class BrokerExecutableLocatorTest {
        private readonly IFileSystem _fs;

        public BrokerExecutableLocatorTest() {
            _fs = Substitute.For<IFileSystem>();
        }

        [Test]
        public void Empty() {
            var locator = new BrokerExecutableLocator(_fs, OSPlatform.Windows);
            locator.GetBrokerExecutablePath().Should().BeNull();
            locator.GetHostExecutablePath().Should().BeNull();
        }

        [CompositeTest]
        [InlineData(BrokerExecutableLocator.WindowsBrokerName, BrokerExecutableLocator.HostName + BrokerExecutableLocator.WindowsExtension)]
        [InlineData(BrokerExecutableLocator.WindowsBrokerName, @"Host\Windows\" + BrokerExecutableLocator.HostName + BrokerExecutableLocator.WindowsExtension)]
        [InlineData(@"Broker\Windows\" + BrokerExecutableLocator.WindowsBrokerName, @"Host\Windows\" + BrokerExecutableLocator.HostName + BrokerExecutableLocator.WindowsExtension)]
        public void Windows(string brokerSubPath, string hostSubPath)
            => TestPlatform(brokerSubPath, hostSubPath, OSPlatform.Windows);

        [CompositeTest]
        [InlineData(BrokerExecutableLocator.UnixBrokerName, @"Host/Mac/" + BrokerExecutableLocator.HostName)]
        public void Mac(string brokerSubPath, string hostSubPath)
            => TestPlatform(brokerSubPath, hostSubPath, OSPlatform.OSX);

        [CompositeTest]
        [InlineData(BrokerExecutableLocator.UnixBrokerName, BrokerExecutableLocator.HostName)]
        [InlineData(BrokerExecutableLocator.UnixBrokerName, @"Host/Linux/" + BrokerExecutableLocator.HostName)]
        public void Linux(string brokerSubPath, string hostSubPath)
            => TestPlatform(brokerSubPath, hostSubPath, OSPlatform.Linux);

        private void TestPlatform(string brokerSubPath, string hostSubPath, OSPlatform platform) {
            var locator = new BrokerExecutableLocator(_fs, platform);
            var brokerPath = Path.Combine(locator.BaseDirectory, brokerSubPath);
            var hostPath = Path.Combine(locator.BaseDirectory, hostSubPath);

            _fs.FileExists(brokerPath).Returns(true);
            locator.GetBrokerExecutablePath().Should().Be(brokerPath);
            locator.GetHostExecutablePath().Should().BeNull();

            _fs.FileExists(hostPath).Returns(true);
            locator.GetBrokerExecutablePath().Should().Be(brokerPath);
            locator.GetHostExecutablePath().Should().Be(hostPath);
        }
    }
}
