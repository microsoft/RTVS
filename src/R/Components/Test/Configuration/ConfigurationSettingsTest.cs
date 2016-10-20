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
        [InlineData("settings$x <- 'a\\\\'", "x", "a\\", ConfigurationSettingValueType.String)]
        [InlineData("settings$x <- 'a\\n'", "x", "a\n", ConfigurationSettingValueType.String)]
        [InlineData("settings$x <- 'a\\x20'", "x", "a ", ConfigurationSettingValueType.String)]
        [InlineData("settings$x <- 'a\\40'", "x", "a ", ConfigurationSettingValueType.String)]
        [InlineData("settings$x <- 'a\\040'", "x", "a ", ConfigurationSettingValueType.String)]
        [InlineData("settings$x <- 'a\\n'", "x", "a\n", ConfigurationSettingValueType.String)]
        [InlineData("settings$x <- 1", "x", "1", ConfigurationSettingValueType.Expression)]
        [InlineData("settings$ab <- x + 3", "ab", "x + 3", ConfigurationSettingValueType.Expression)]
        [InlineData("settings$`x y` <- 'abc'", "`x y`", "abc", ConfigurationSettingValueType.String)]
        [InlineData("settings$c.d <- \"1 0\"", "c.d", "1 0", ConfigurationSettingValueType.String)]
        [InlineData("settings$x <- 1", "x", "1", ConfigurationSettingValueType.Expression)]
        [InlineData("settings$z <- c(1:100)", "z", "c(1:100)", ConfigurationSettingValueType.Expression)]
        [InlineData("x <- 'a\\\\'", "x", "a\\", ConfigurationSettingValueType.String)]
        [InlineData("x <- 'a\\n'", "x", "a\n", ConfigurationSettingValueType.String)]
        [InlineData("x <- 'a\\x20'", "x", "a ", ConfigurationSettingValueType.String)]
        [InlineData("x <- 'a\\40'", "x", "a ", ConfigurationSettingValueType.String)]
        [InlineData("x <- 'a\\040'", "x", "a ", ConfigurationSettingValueType.String)]
        [InlineData("x <- 'a\\n'", "x", "a\n", ConfigurationSettingValueType.String)]
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
            string content1 =
@"
settings$x <- 1

# comment
settings$y <- x + 3
# [Category] Category 1
settings$z <- 'ab
c'
";
            string content2 =
@"
x <- 1

# comment
y <- x + 3
# [Category] Category 1
z <- 'ab
c'
";

            foreach (var content in new string[] { content1, content2 }) {
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
        }

        [Test]
        public void LoadMultiple02() {
            string content1 =
@"
# [Category] SQL
# [Description] Database connection string
# [Editor] ConnectionStringEditor
settings$c1 <- 'DSN'
";
            string content2 =
@"
# [Category] SQL
# [Description] Database connection string
# [Editor] ConnectionStringEditor
c1 <- 'DSN'
";
            foreach (var content in new string[] { content1, content2 }) {
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
        }

        [Test]
        public void LoadMultiple03() {
            string content1 =
@"# [Category] SQL
# [Editor] ConnectionStringEditor
# [Editor] ConnectionStringEditor
settings$c1 <- 'DSN'
";

            string content2 =
@"# [Category] SQL
# [Editor] ConnectionStringEditor
# [Editor] ConnectionStringEditor
c1 <- 'DSN'
";

            foreach (var content in new string[] { content1, content2 }) {
                ConfigurationParser cp;
                var settings = GetSettings(content, out cp);

                settings.Should().HaveCount(1);

                settings[0].Name.Should().Be("c1");
                settings[0].Value.Should().Be("DSN");
                settings[0].ValueType.Should().Be(ConfigurationSettingValueType.String);
                settings[0].Category.Should().Be("SQL");
                settings[0].EditorType.Should().Be("ConnectionStringEditor");
            }
        }

        [CompositeTest]
        [InlineData("x <- '\\x'", 1, 1)]
        [InlineData("x <- '\\xg'", 1, 1)]
        [InlineData("x <- '\\p'", 1, 1)]
        [InlineData("x <- '\\9'", 1, 1)]
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

            settings.Should().BeEmpty();
            cp.Errors.Should().HaveCount(expectedCount);
            if (expectedCount > 0) {
                cp.Errors[0].LineNumber.Should().Be(expectedLineNumber);
            }
        }

        [Test]
        public void Rewrite() {
            string content = string.Format(
@"settings <- as.environment(list())

# [Category] SQL
# [Description] Database connection string
# [Editor] ConnectionString
settings$c1 <- 'DSN'

settings$x <- 1

settings$y <- 'MACHINE\\INSTANCE'

settings$dq <- 'double{0}quote'

settings$q <- {0}single'quote{0}

settings$qplusdq <- 'single\'quote+double{0}quote'

settings$tab <- 'hello\tworld'

settings$newline <- 'hello\nworld'

settings$cr <- 'hello\rworld'

", '"');
            LoadAndWrite(content, content);
        }

        [Test]
        public void RewriteAndUpgradeFromV05() {
            string content =
@"# [Category] SQL
# [Description] Database connection string
# [Editor] ConnectionString
c1 <- 'DSN'

x <- 1
";

            string updatedContent =
@"settings <- as.environment(list())

# [Category] SQL
# [Description] Database connection string
# [Editor] ConnectionString
settings$c1 <- 'DSN'

settings$x <- 1
";
            LoadAndWrite(content, updatedContent);
        }

        private void LoadAndWrite(string originalContent, string expectedContent) {
            IReadOnlyList<IConfigurationSetting> settings;
            var sr = new StreamReader(ToStream(originalContent));
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
                    s.Should().Contain(expectedContent);
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
