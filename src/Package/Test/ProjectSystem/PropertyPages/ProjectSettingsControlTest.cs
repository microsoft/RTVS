// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.ProjectSystem.PropertyPages {
    [ExcludeFromCodeCoverage]
    [Category.Configuration]
    public class ProjectSettingsControlTest {
        [Test]
        public void Construction() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            control.IsDirty.Should().BeFalse();
            control.CreateControl();

            control.FileListCombo.Items.Count.Should().Be(1);
            control.FileListCombo.Items[0].Should().Be(Microsoft.VisualStudio.R.Package.Resources.NoSettingFiles);

            control.VariableName.Text.Should().BeNullOrEmpty();
            control.VariableValue.Text.Should().BeNullOrEmpty();
            control.VariableTypeCombo.Items.Should().HaveCount(2);
            control.AddButton.Enabled.Should().BeFalse();
        }

        [Test]
        public async Task DirtyState() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();

            var control = new SettingsPageControl(shell, fs);
            int count = 0;
            control.DirtyStateChanged += (s, e) => {
                count++;
            };
            control.IsDirty = true;
            control.IsDirty.Should().BeTrue();
            count.Should().Be(1);

            (await control.SaveAsync()).Should().BeFalse(); // No op
            control.IsDirty.Should().BeTrue();

            control.SetProject(Path.GetTempPath(), null);
            (await control.SaveAsync()).Should().BeTrue();
            control.IsDirty.Should().BeFalse();
        }

        [Test]
        public void PropertyGridSingle() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            var folder = Path.GetTempPath();
            var fileName = "534.Settings.R";
            var file = Path.Combine(folder, fileName);
            try {
                using (var sw = new StreamWriter(file)) {
                    sw.WriteLine("x <- 1");
                }
                fs.GetFileSystemEntries(Arg.Any<string>()).Returns(new string[] { file });

                control.SetProject(folder, null);

                control.CreateControl();
                control.FileListCombo.Items.Count.Should().BeGreaterThan(0);
                control.FileListCombo.Items.Should().Contain("~/" + fileName);

                control.PropertyGrid.SelectedObject.Should().NotBeNull();

                var desc = control.PropertyGrid.SelectedObject as SettingsTypeDescriptor;
                desc.Should().NotBeNull();
                desc.GetProperties().Should().HaveCount(1);

                var prop = desc.GetProperties()[0] as SettingPropertyDescriptor;
                prop.Should().NotBeNull();
                prop.Setting.Name.Should().Be("x");
                prop.Setting.Value.Should().Be("1");
                prop.Setting.ValueType.Should().Be(ConfigurationSettingValueType.Expression);
            } finally {
                if (File.Exists(file)) {
                    File.Delete(file);
                }
            }
        }

        [Test]
        public void AddVariable() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            control.SetProject(Path.GetTempPath(), null);
            control.CreateControl();

            control.PropertyGrid.SelectedObject.Should().BeNull();

            control.AddButton.Enabled.Should().BeFalse();
            control.VariableName.Text = "x";
            control.AddButton.Enabled.Should().BeFalse();
            control.VariableValue.Text = " ";
            control.AddButton.Enabled.Should().BeFalse();
            control.VariableValue.Text = "1";
            control.AddButton.Enabled.Should().BeTrue();
            control.VariableName.Text = " ";
            control.AddButton.Enabled.Should().BeFalse();

            control.VariableName.Text = "x";
            control.AddButton.Enabled.Should().BeTrue();
            control.AddButton.PerformClick();

            var desc = control.PropertyGrid.SelectedObject as SettingsTypeDescriptor;
            desc.Should().NotBeNull();
            desc.GetProperties().Should().HaveCount(1);

            var prop = desc.GetProperties()[0] as SettingPropertyDescriptor;
            prop.Should().NotBeNull();
            prop.Setting.Name.Should().Be("x");
            prop.Setting.Value.Should().Be("1");
            prop.Setting.ValueType.Should().Be(ConfigurationSettingValueType.String);
        }

        [Test]
        public void PropertyGridMultiple01() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            var folder = Path.GetTempPath();
            var fileName1 = "0123.Settings.R";
            var file1 = Path.Combine(folder, fileName1);
            var fileName2 = "0456.Settings.R";
            var file2 = Path.Combine(folder, fileName2);
            try {
                using (var sw = new StreamWriter(file1)) {
                    sw.WriteLine("x <- 1");
                }
                using (var sw = new StreamWriter(file2)) {
                    sw.WriteLine("y <- 2");
                }
                fs.GetFileSystemEntries(Arg.Any<string>()).Returns(new string[] { file1, file2 });

                control.SetProject(folder, null);

                control.CreateControl();
                control.FileListCombo.Items.Count.Should().BeGreaterThan(0);
                control.FileListCombo.Items.Should().Contain("~/" + fileName1);
                control.FileListCombo.Items.Should().Contain("~/" + fileName2);

                control.PropertyGrid.SelectedObject.Should().NotBeNull();
                var desc = control.PropertyGrid.SelectedObject as SettingsTypeDescriptor;
                desc.Should().NotBeNull();
                desc.GetProperties().Should().HaveCount(1);

                var prop = desc.GetProperties()[0] as SettingPropertyDescriptor;
                prop.Should().NotBeNull();
                prop.SetValue(null, "42");
                prop.Setting.Value.Should().Be("42");

                var pg = control.PropertyGrid;
                var mi = pg.GetType().GetMethod("OnPropertyValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(pg, new object[] { new PropertyValueChangedEventArgs(pg.SelectedGridItem, "1") });

                control.IsDirty.Should().BeTrue();
                shell.ShowMessage(Microsoft.VisualStudio.R.Package.Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel).Returns(MessageButtons.Cancel);

                control.FileListCombo.SelectedIndex = 1;
                shell.Received().ShowMessage(Microsoft.VisualStudio.R.Package.Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel);

                control.IsDirty.Should().BeTrue();
                control.FileListCombo.SelectedIndex.Should().Be(0);
            } finally {
                if (File.Exists(file1)) {
                    File.Delete(file1);
                }
                if (File.Exists(file2)) {
                    File.Delete(file2);
                }
            }
        }

        [Test]
        public void PropertyGridMultiple02() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            var folder = Path.GetTempPath();
            var fileName1 = "1123.Settings.R";
            var file1 = Path.Combine(folder, fileName1);
            var fileName2 = "1456.Settings.R";
            var file2 = Path.Combine(folder, fileName2);
            try {
                using (var sw = new StreamWriter(file1)) {
                    sw.WriteLine("x <- 1");
                }
                using (var sw = new StreamWriter(file2)) {
                    sw.WriteLine("y <- 2");
                }
                fs.GetFileSystemEntries(Arg.Any<string>()).Returns(new string[] { file1, file2 });

                control.SetProject(folder, null);

                control.CreateControl();
                control.FileListCombo.Items.Count.Should().BeGreaterThan(0);
                control.FileListCombo.Items.Should().Contain("~/" + fileName1);
                control.FileListCombo.Items.Should().Contain("~/" + fileName2);

                control.PropertyGrid.SelectedObject.Should().NotBeNull();
                var desc = control.PropertyGrid.SelectedObject as SettingsTypeDescriptor;
                desc.Should().NotBeNull();
                desc.GetProperties().Should().HaveCount(1);

                var prop = desc.GetProperties()[0] as SettingPropertyDescriptor;
                prop.Should().NotBeNull();
                prop.SetValue(null, "42");
                prop.Setting.Value.Should().Be("42");

                var pg = control.PropertyGrid;
                var mi = pg.GetType().GetMethod("OnPropertyValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(pg, new object[] { new PropertyValueChangedEventArgs(pg.SelectedGridItem, "1") });

                control.IsDirty.Should().BeTrue();

                shell.ShowMessage(Microsoft.VisualStudio.R.Package.Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel).Returns(MessageButtons.Yes);
                control.FileListCombo.SelectedIndex = 1;
                shell.Received().ShowMessage(Microsoft.VisualStudio.R.Package.Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel);

                control.IsDirty.Should().BeFalse();
                control.FileListCombo.SelectedIndex.Should().Be(1);

            } finally {
                if (File.Exists(file1)) {
                    File.Delete(file1);
                }
                if (File.Exists(file2)) {
                    File.Delete(file2);
                }
            }
        }
    }
}
