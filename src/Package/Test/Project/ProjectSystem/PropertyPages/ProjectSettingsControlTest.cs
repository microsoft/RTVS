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
        private readonly PackageTestFilesFixture _files;

        public ProjectSettingsControlTest(PackageTestFilesFixture files) {
            _files = files;
        }

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

            (await control.SaveSettingsAsync()).Should().BeFalse(); // No op
            control.IsDirty.Should().BeTrue();

            await control.SetProjectAsync(Path.GetTempPath(), null);
            (await control.SaveSettingsAsync()).Should().BeTrue();
            control.IsDirty.Should().BeFalse();
        }

        [Test]
        public async Task PropertyGridSingle() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            var fileName = "PropertyGridSingle.settings.r";
            var file = Path.Combine(_files.DestinationPath, fileName);
            fs.GetFileSystemEntries(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories).Returns(new string[] { file });

            await control.SetProjectAsync(_files.DestinationPath, null);

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
        }

        [Test]
        public async Task AddVariable() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            await control.SetProjectAsync(Path.GetTempPath(), null);
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
        public async Task PropertyGridMultiple01() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            var fileName1 = "PropertyGridMultiple01-1.Settings.R";
            var file1 = Path.Combine(_files.DestinationPath, fileName1);
            var fileName2 = "PropertyGridMultiple01-2.Settings.R";
            var file2 = Path.Combine(_files.DestinationPath, fileName2);
            fs.GetFileSystemEntries(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories).Returns(new string[] { file1, file2 });

            await control.SetProjectAsync(_files.DestinationPath, null);

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
            shell.ShowMessage(Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel).Returns(MessageButtons.Cancel);

            control.FileListCombo.SelectedIndex = 1;
            shell.Received().ShowMessage(Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel);

            control.IsDirty.Should().BeTrue();
            control.FileListCombo.SelectedIndex.Should().Be(0);
        }

        [Test]
        public async Task PropertyGridMultiple02() {
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var control = new SettingsPageControl(shell, fs);

            var fileName1 = "PropertyGridMultiple02-1.Settings.R";
            var file1 = Path.Combine(_files.DestinationPath, fileName1);
            var fileName2 = "PropertyGridMultiple02-2.Settings.R";
            var file2 = Path.Combine(_files.DestinationPath, fileName2);
            fs.GetFileSystemEntries(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories).Returns(new string[] { file1, file2 });

            await control.SetProjectAsync(_files.DestinationPath, null);

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

            shell.ShowMessage(Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel).Returns(MessageButtons.Yes);
            control.FileListCombo.SelectedIndex = 1;
            shell.Received().ShowMessage(Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel);

            control.IsDirty.Should().BeTrue(); // Changing between setting files makes page dirty
            control.FileListCombo.SelectedIndex.Should().Be(1);
        }
    }
}
