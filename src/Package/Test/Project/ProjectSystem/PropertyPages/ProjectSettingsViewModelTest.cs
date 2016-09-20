// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.ProjectSystem.PropertyPages {
    [ExcludeFromCodeCoverage]
    [Category.Configuration]
    public class ProjectSettingsViewModelTest {
        [Test]
        public async Task EmptyViewModel() {
            var css = Substitute.For<IConfigurationSettingCollection>();
            var shell = Substitute.For<ICoreShell>();

            string file = Path.GetTempFileName();
            var fs = Substitute.For<IFileSystem>();
            fs.GetFileSystemEntries(file, Arg.Any<string>(), SearchOption.AllDirectories).Returns(new string[] { file });
            fs.DirectoryExists(file).Returns(false);
            fs.FileExists(file).Returns(true);

            var model = new SettingsPageViewModel(css, shell, fs);
            await model.SetProjectPathAsync(Path.GetDirectoryName(file), null);
            model.CurrentFile.Should().BeNull();
            (await model.SaveAsync()).Should().BeFalse(); // nothing should happen
        }

        [Test]
        public async Task CreateNewFile() {
            var css = new ConfigurationSettingCollection();
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();

            var model = new SettingsPageViewModel(css, shell, fs);
            string file = Path.GetTempFileName();
            await model.SetProjectPathAsync("C:\\", null);
            model.CreateNewSettingsFile();
            model.CurrentFile.Should().Be("~/Settings.R");
        }

        [Test]
        public async Task EnumerateFiles() {
            var css = Substitute.For<IConfigurationSettingCollection>();
            var shell = Substitute.For<ICoreShell>();

            string folder = @"C:\";
            string file1 = @"C:\Settings.R";
            string file2 = @"C:\Debug.Settings.R";
            string file3 = @"C:\foo.bar";
            var fs = Substitute.For<IFileSystem>();
            fs.GetFileSystemEntries(folder, Arg.Any<string>(), SearchOption.AllDirectories).Returns(new string[] { file1, file2, file3 });
            fs.DirectoryExists(Arg.Any<string>()).Returns(false);
            fs.FileExists(file1).Returns(true);
            fs.FileExists(file2).Returns(true);
            fs.FileExists(file3).Returns(true);

            var model = new SettingsPageViewModel(css, shell, fs);
            await model.SetProjectPathAsync(folder, null);
            model.CurrentFile.Should().Be("~/Settings.R");
            model.Files.Should().HaveCount(2);
            model.Files.ToArray()[0].Should().Be("~/Settings.R");
            model.Files.ToArray()[1].Should().Be("~/Debug.Settings.R");
        }

        [Test]
        public async Task LoadSave() {
            var settings = Substitute.For<IConfigurationSettingCollection>();
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var pp = Substitute.For<IRProjectProperties>();
            fs.GetFileSystemEntries(null, null, SearchOption.AllDirectories).ReturnsForAnyArgs(new string[] { @"C:\Settings22.R" });
            pp.GetSettingsFileAsync().Returns(Task.FromResult("~/Settings22.R"));

            var model = new SettingsPageViewModel(settings, shell, fs);
            await model.SetProjectPathAsync(@"C:\", pp);
            await pp.Received().GetSettingsFileAsync();
            model.CurrentFile.Should().Be("~/Settings22.R");

            (await model.SaveAsync()).Should().BeTrue();
            settings.Received().Save(@"C:\Settings22.R");
        }
    }
}
