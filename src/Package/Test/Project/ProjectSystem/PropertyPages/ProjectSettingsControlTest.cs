// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.ProjectSystem.PropertyPages {
    [ExcludeFromCodeCoverage]
    [Category.Configuration]
    public class ProjectSettingsControlTest {
        private readonly PackageTestFilesFixture _files;
        private readonly ICoreShell _shell;
        private readonly IProjectConfigurationSettingsProvider _csp;
        private readonly IFileSystem _fs;
        private readonly IRProjectProperties _properties;
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly ConfigurationSettingCollection _coll = new ConfigurationSettingCollection();
        private readonly IProjectConfigurationSettingsAccess _access;

        public ProjectSettingsControlTest(PackageTestFilesFixture files) {
            _files = files;
            _shell = Substitute.For<ICoreShell>();
            _fs = Substitute.For<IFileSystem>();

            _access = Substitute.For<IProjectConfigurationSettingsAccess>();
            _access.Settings.Returns(_coll);

            _csp = Substitute.For<IProjectConfigurationSettingsProvider>();
            _csp.OpenProjectSettingsAccessAsync(null, null).ReturnsForAnyArgs(Task.FromResult(_access));

            _unconfiguredProject = Substitute.For<UnconfiguredProject>();
            _unconfiguredProject.FullPath.Returns(@"C:\file.rproj");

            _properties = Substitute.For<IRProjectProperties>();
            _properties.GetSettingsFileAsync().Returns(Task.FromResult<string>(null));
        }

        [Test]
        public void Construction() {
            var control = new SettingsPageControl(_csp, _shell, _fs);

            control.IsDirty.Should().BeFalse();
            control.CreateControl();

            control.FileListCombo.Items.Count.Should().Be(1);
            control.FileListCombo.Items[0].Should().Be(Resources.NoSettingFiles);

            control.VariableName.Text.Should().BeNullOrEmpty();
            control.VariableValue.Text.Should().BeNullOrEmpty();
            control.VariableTypeCombo.Items.Should().HaveCount(2);
            control.AddButton.Enabled.Should().BeFalse();
        }

        [Test]
        public void Font() {
            var fontSvc = Substitute.For<IUIHostLocale2>();
            var logFont = new UIDLGLOGFONT[1];
            logFont[0].lfItalic = 1;

            fontSvc.When(x => x.GetDialogFont(Arg.Any<UIDLGLOGFONT[]>())).Do(x => {
                var fontName = "Arial";
                var lf = x.Args()[0] as UIDLGLOGFONT[];
                lf[0].lfItalic = 1;
                lf[0].lfHeight = 16;
                lf[0].lfFaceName = new ushort[fontName.Length];
                int i = 0;
                foreach (var ch in fontName) {
                    lf[0].lfFaceName[i++] = (ushort)ch;
                }
            });
            fontSvc.GetDialogFont(Arg.Any<UIDLGLOGFONT[]>()).Returns(VSConstants.S_OK);

            var shell = Substitute.For<ICoreShell>();
            shell.GetService<IUIHostLocale2>(typeof(SUIHostLocale)).Returns(fontSvc);

            var control = new SettingsPageControl(_csp, shell, _fs);
            control.CreateControl();
            control.Font.Italic.Should().BeTrue();
            control.Font.Name.Should().Be("Arial");
        }

        [Test]
        public async Task DirtyState() {
            var control = new SettingsPageControl(_csp, _shell, _fs);
            int count = 0;
            control.DirtyStateChanged += (s, e) => {
                count++;
            };
            control.IsDirty = true;
            control.IsDirty.Should().BeTrue();
            count.Should().Be(1);

            await control.SetProjectAsync(_unconfiguredProject, _properties);
            (await control.SaveSettingsAsync()).Should().BeTrue();
            control.IsDirty.Should().BeFalse();
        }

        [Test]
        public async Task PropertyGridSingle() {
            var fs = Substitute.For<IFileSystem>();
            var up = Substitute.For<UnconfiguredProject>();
            up.FullPath.Returns(Path.Combine(_files.DestinationPath, "file.rproj"));

            var control = new SettingsPageControl(_csp, _shell, fs);

            var fileName = "PropertyGridSingle.settings.r";
            var file = Path.Combine(_files.DestinationPath, fileName);
            fs.GetFileSystemEntries(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories).Returns(new string[] { file });

            await control.SetProjectAsync(up, _properties);

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
            var control = new SettingsPageControl(_csp, _shell, _fs);

            await control.SetProjectAsync(_unconfiguredProject, _properties);
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
            var up = Substitute.For<UnconfiguredProject>();
            up.FullPath.Returns(Path.Combine(_files.DestinationPath, "file.rproj"));

            var control = new SettingsPageControl(_csp, shell, fs);

            var fileName1 = "PropertyGridMultiple01-1.Settings.R";
            var file1 = Path.Combine(_files.DestinationPath, fileName1);
            var fileName2 = "PropertyGridMultiple01-2.Settings.R";
            var file2 = Path.Combine(_files.DestinationPath, fileName2);
            fs.GetFileSystemEntries(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories).Returns(new string[] { file1, file2 });

            await control.SetProjectAsync(up, _properties);

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
            var up = Substitute.For<UnconfiguredProject>();
            up.FullPath.Returns(Path.Combine(_files.DestinationPath, "file.rproj"));

            var control = new SettingsPageControl(_csp, shell, fs);

            var fileName1 = "PropertyGridMultiple02-1.Settings.R";
            var file1 = Path.Combine(_files.DestinationPath, fileName1);
            var fileName2 = "PropertyGridMultiple02-2.Settings.R";
            var file2 = Path.Combine(_files.DestinationPath, fileName2);
            fs.GetFileSystemEntries(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories).Returns(new string[] { file1, file2 });

            await control.SetProjectAsync(up, _properties);

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

            var uis = shell.UI();
            uis.ShowMessage(Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel).Returns(MessageButtons.Yes);
            control.FileListCombo.SelectedIndex = 1;
            uis.Received().ShowMessage(Resources.SettingsPage_SavePrompt, MessageButtons.YesNoCancel);

            control.IsDirty.Should().BeTrue(); // Changing between setting files makes page dirty
            control.FileListCombo.SelectedIndex.Should().Be(1);
        }
    }
}
