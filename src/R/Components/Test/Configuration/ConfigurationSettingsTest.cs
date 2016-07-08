// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Components.Application.Configuration.Parser;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Components.Test.Configuration {
    [ExcludeFromCodeCoverage]
    [Category.Configuration]
    public sealed class ConfigurationSettingsTest {
        [CompositeTest]
        [InlineData("x <- 1", "x", "1", ConfigurationSettingValueType.Expression)]
        [InlineData("ab <- x + 3", "ab", "x + 3", ConfigurationSettingValueType.Expression)]
        [InlineData("`x y` <- 'abc'", "`x y`", "abc", ConfigurationSettingValueType.String)]
        [InlineData("c.d <- \"1 0\"", "c.d", "1 0", ConfigurationSettingValueType.String)]
        [InlineData("x <- 1", "x", "1", ConfigurationSettingValueType.Expression)]
        [InlineData("z <- c(1:100)", "z", "c(1:100)", ConfigurationSettingValueType.Expression)]
        public void LoadSingle(string content, string expectedName, string expectedValue, ConfigurationSettingValueType expectedValueType) {
            var settings = new List<IConfigurationSetting>();

            using (var sr = new StreamReader(ToStream(content))) {
                var cp = new ConfigurationParser(sr);
                while (true) {
                    var s = cp.ReadSetting();
                    if (s == null) {
                        break;
                    }
                    settings.Add(s);
                }
            }
            settings.Should().HaveCount(1);
            settings[0].Name.Should().Be(expectedName);
            settings[0].Value.Should().Be(expectedValue);
            settings[0].ValueType.Should().Be(expectedValueType);
        }

        [Test]
        public void LoadMultiple01() {
            string content =
@"
x <- 1

# comment
y <- x + 3
# [Category] Category 1
z <- 'ab
c'
";
            ConfigurationParser cp;
            var settings = GetSettings(content, out cp);

            settings.Should().HaveCount(3);

            settings[0].Name.Should().Be("x");
            settings[0].Value.Should().Be("1");
            settings[0].ValueType.Should().Be(ConfigurationSettingValueType.Expression);
            settings[0].Category.Should().BeNull();
            settings[0].Description.Should().BeNull();
            settings[0].EditorType.Should().BeNull();

            settings[1].Name.Should().Be("y");
            settings[1].Value.Should().Be("x + 3");
            settings[1].ValueType.Should().Be(ConfigurationSettingValueType.Expression);

            settings[2].Name.Should().Be("z");
            settings[2].Value.Should().Be("ab\r\nc");
            settings[2].ValueType.Should().Be(ConfigurationSettingValueType.String);
            settings[2].Category.Should().Be("Category 1");
        }

        [Test]
        public void LoadMultiple02() {
            string content =
@"
# [Category] SQL
# [Description] Database connection string
# [Editor] ConnectionStringEditor
c1 <- 'DSN'
";
            using (var sr = new StreamReader(ToStream(content))) {
                using (var css = new ConfigurationSettingsReader(sr)) {
                    var settings = css.LoadSettings();
                    settings.Should().HaveCount(1);

                    settings[0].Name.Should().Be("c1");
                    settings[0].Value.Should().Be("DSN");
                    settings[0].ValueType.Should().Be(ConfigurationSettingValueType.String);
                    settings[0].Category.Should().Be("SQL");
                    settings[0].Description.Should().Be("Database connection string");
                    settings[0].EditorType.Should().Be("ConnectionStringEditor");
                }
            }
        }

        [Test]
        public void LoadMultiple03() {
            string content =
@"# [Category] SQL
# [Editor] ConnectionStringEditor
# [Editor] ConnectionStringEditor
c1 <- 'DSN'
";
            ConfigurationParser cp;
            var settings = GetSettings(content, out cp);

            settings.Should().HaveCount(1);

            settings[0].Name.Should().Be("c1");
            settings[0].Value.Should().Be("DSN");
            settings[0].ValueType.Should().Be(ConfigurationSettingValueType.String);
            settings[0].Category.Should().Be("SQL");
            settings[0].EditorType.Should().Be("ConnectionStringEditor");
        }

        [CompositeTest]
        [InlineData("x <- ", 1, 1)]
        [InlineData("", 0, 0)]
        [InlineData(@"

                        x < 1 ", 1, 3)]
        [InlineData("#", 0, 0)]
        [InlineData("# x <- 1", 0, 0)]
        [InlineData(" <- 1", 1, 1)]
        [InlineData(@"
                      <- 1

                      x <- ", 1, 2)]
        public void LoadErrors(string content, int expectedCount, int expectedLineNumber) {
            ConfigurationParser cp;
            var settings = GetSettings(content, out cp);

            settings.Should().HaveCount(0);
            cp.Errors.Should().HaveCount(expectedCount);
            if (expectedCount > 0) {
                cp.Errors[0].LineNumber.Should().Be(expectedLineNumber);
            }
        }

        [Test]
        public void Write01() {
            string content =
@"# [Category] SQL
# [Description] Database connection string
# [Editor] ConnectionString
c1 <- 'DSN'

x <- 1
";
            IReadOnlyList<IConfigurationSetting> settings;
            var sr = new StreamReader(ToStream(content));
            using (var csr = new ConfigurationSettingsReader(sr)) {
                settings = csr.LoadSettings();
            }

            var stream = new MemoryStream();
            using (var csw = new ConfigurationSettingsWriter(new StreamWriter(stream))) {
                csw.SaveSettings(settings);

                stream.Seek(0, SeekOrigin.Begin);
                using (var r = new StreamReader(stream)) {
                    var s = r.ReadToEnd();
                    s.Should().StartWith(Resources.SettingsFileHeader);
                    s.Should().Contain(content);
                }
            }
        }

        private List<IConfigurationSetting> GetSettings(string content, out ConfigurationParser cp) {
            var settings = new List<IConfigurationSetting>();
            using (var sr = new StreamReader(ToStream(content))) {
                cp = new ConfigurationParser(sr);
                while (true) {
                    var s = cp.ReadSetting();
                    if (s == null) {
                        break;
                    }
                    settings.Add(s);
                }
            }
            return settings;
        }

        private Stream ToStream(string s) {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }
    }
}
