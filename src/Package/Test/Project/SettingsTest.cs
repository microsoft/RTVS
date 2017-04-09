// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using NSubstitute;
using Microsoft.R.Components.Application.Configuration;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Category.Configuration]
    public class SettingsProviderTest {

        private readonly PackageTestFilesFixture _files;

        public SettingsProviderTest(PackageTestFilesFixture files) {
            _files = files;
        }

        [Test]
        public async Task ConcurrentAccess() {
            string folder = Path.Combine(_files.DestinationPath, "ConcurrentAccess");
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
            string projFile = Path.Combine(folder, "file.rproj");
            var up = Substitute.For<UnconfiguredProject>();
            up.FullPath.Returns(projFile);

            var props = Substitute.For<IRProjectProperties>();
            var p = new ProjectConfigurationSettingsProvider();

            var s1 = new ConfigurationSetting("s1", "1", ConfigurationSettingValueType.String);
            var s2 = new ConfigurationSetting("s2", "2", ConfigurationSettingValueType.String);

            using (var access1 = await p.OpenProjectSettingsAccessAsync(up, props)) {
                access1.Settings.Should().NotBeNull();
                access1.Settings.Should().BeEmpty();

                using (var access2 = await p.OpenProjectSettingsAccessAsync(up, props)) {
                    access2.Settings.Should().NotBeNull();
                    access2.Settings.Add(s1);
                    access1.Settings.Count.Should().Be(1);
                }

                access1.Settings.Add(s2);
                access1.Settings.Count.Should().Be(2);
            }

            var settingsFile = Path.Combine(folder, "Settings.R");
            File.Exists(settingsFile).Should().BeTrue();

            var c = new ConfigurationSettingCollection();
            c.Load(settingsFile);

            c.Count.Should().Be(2);
            c.GetSetting("s1").Value.Should().Be("1");
            c.GetSetting("s2").Value.Should().Be("2");

            File.Delete(settingsFile);
        }
    }
}
