// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test.Configuration {
    [ExcludeFromCodeCoverage]
    [Category.Configuration]
    public class ConfigurationSettingsCollectionTest {
        [Test]
        public void Test01() {
            string file = Path.GetTempFileName();
            var content = "x <- 1";

            var coll = new ConfigurationSettingCollection();
            coll.Save(file);

            var fi = new FileInfo(file);
            fi.Length.Should().Be(0);

            try {
                using (var sw = new StreamWriter(file)) {
                    sw.Write(content);
                }

                coll.Load(file);
                coll.GetSetting("foo").Should().BeNull();

                var setting = coll.GetSetting("x");
                setting.Should().NotBeNull();
                setting.Name.Should().Be("x");
                setting.Value.Should().Be("1");
                setting.ValueType.Should().Be(ConfigurationSettingValueType.Expression);

            } finally {
                if (File.Exists(file)) {
                    File.Delete(file);
                }
            }
        }
    }
}
