// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Settings {
    [ExcludeFromCodeCoverage]
    [Category.VsPackage.Settings]
    public sealed class SettingsStorageTest {
        [Test]
        public void SaveRestore() {
            SaveRestore("name", -2);
            SaveRestore("name", true);
            SaveRestore("name", false);
            SaveRestore("name", (uint)1);
            SaveRestore("name", "string");
            SaveRestore("name", DateTime.Now);
            SaveRestore("name", new TestSetting("p1", 1));
        }

        public void SaveRestore<T>(string name, T value) {
            var storage = new VsSettingsStorage(new TestSettingsManager());
            storage.SettingExists(name).Should().BeFalse();
            storage.SetSetting(name, value);
            storage.SettingExists(name).Should().BeTrue();
            storage.GetSetting(name, value.GetType()).Should().Be(value);

            storage.Persist();
            storage.ClearCache();

            storage.SettingExists(name).Should().BeTrue();
            storage.GetSetting(name, value.GetType()).Should().Be(value);
        }

        class TestSetting {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }

            public TestSetting(string p1, int p2) {
                Prop1 = p1;
                Prop2 = p2;
            }

            public override bool Equals(object obj) {
                var other = (TestSetting)obj;
                return other.Prop1 == this.Prop1 && other.Prop2 == this.Prop2;
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }
    }
}
