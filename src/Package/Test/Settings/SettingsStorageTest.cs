// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Mocks;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Settings {
    [ExcludeFromCodeCoverage]
    [Category.VsPackage.Settings]
    public sealed class SettingsStorageTest {
        [Test]
        public async Task SaveRestore() {
            await SaveRestoreAsync("name", -2);
            await SaveRestoreAsync("name", true);
            await SaveRestoreAsync("name", false);
            await SaveRestoreAsync("name", (uint)1);
            await SaveRestoreAsync("name", "string");
            await SaveRestoreAsync("name", DateTime.Now);
            await SaveRestoreAsync("name", new TestSetting("p1", 1));
        }

        public async Task SaveRestoreAsync<T>(string name, T value) {
            var sm = Substitute.For<ISettingsManager>();

            var storage = new VsSettingsStorage(new VsSettingsPersistenceManagerMock());
            storage.SettingExists(name).Should().BeFalse();
            storage.SetSetting(name, value);
            storage.SettingExists(name).Should().BeTrue();
            storage.GetSetting(name, value.GetType()).Should().Be(value);

            await storage.PersistAsync();
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
