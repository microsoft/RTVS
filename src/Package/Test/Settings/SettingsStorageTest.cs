// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Settings;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Settings {
    [ExcludeFromCodeCoverage]
    [Category.VsPackage.Settings]
    public sealed class SettingsStorageTest {
        private readonly TestSettingsManager _sm = new TestSettingsManager();

        [Test]
        public void SaveRestore(int value) {
            SaveRestore("name", -2);
            SaveRestore("name", true);
            SaveRestore("name", false);
            SaveRestore("name", (uint)1);
            SaveRestore("name", "string");
            SaveRestore("name", DateTime.Now);
            SaveRestore("name", new TestSetting("p1", 1));
        }

        public void SaveRestore<T>(string name, T value) {
            var storage = new VsSettingsStorage(_sm);
            storage.SettingExists(name).Should().BeFalse();
            storage.SetSetting(name, value);
            storage.SettingExists(name).Should().BeTrue();
            storage.GetSetting(name, value.GetType()).Should().Be(value);
        }

        class TestSetting {
            public string Prop1 { get; }
            public int Prop2 { get; }

            public TestSetting(string p1, int p2) {
                Prop1 = p1;
                Prop2 = p2;
            }
        }
    }
}
