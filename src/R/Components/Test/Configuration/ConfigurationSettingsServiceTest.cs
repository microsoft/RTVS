// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Microsoft.R.Components.Test.Configuration {
    [ExcludeFromCodeCoverage]
    public class ConfigurationSettingsServiceTest {
        [Test]
        [Category.Configuration]
        public void Test01() {
            string file = Path.GetTempFileName();
            var content = "x <- 1";

            var fs = Substitute.For<IFileSystem>();
            fs.FileExists(file).Returns(true);

            var coreShell = Substitute.For<ICoreShell>();
            var css = new ConfigurationSettingsService(coreShell);
            css.FileSystem = fs;

            css.Settings.Should().BeEmpty();
            css.Save(file);
            var fi = new FileInfo(file);
            fi.Length.Should().Be(0);

            try {
                using (var sw = new StreamWriter(file)) {
                    sw.Write(content);
                }

                css.Load(file);

                css.Settings.FirstOrDefault(x => x.Name.EqualsOrdinal("foo")).Should().BeNull();
                var setting = css.GetSetting("x");
                setting.Should().NotBeNull();
                setting.Name.Should().Be("x");
                setting.Value.Should().Be("1");
                setting.ValueType.Should().Be(ConfigurationSettingValueType.Expression);
                setting.Attributes.Should().BeEmpty();

            } finally {
                if (File.Exists(file)) {
                    File.Delete(file);
                }
            }
        }
    }
}
