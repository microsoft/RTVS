// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class PublishOptionsDialogTest {
        [Test(ThreadType.UI)]
        public async Task Constructor() {
            var appShell = Substitute.For<IApplicationShell>();
            var pss = Substitute.For<IProjectSystemServices>();
            var pcsp = Substitute.For<IProjectConfigurationSettingsProvider>();
            var storage = Substitute.For<IWritableEditorSettingsStorage>();
            var fs = Substitute.For<IFileSystem>();
            var s = Substitute.For<ISettingsStorage>();

            var dlg = await SqlPublshOptionsDialog.CreateAsync(appShell, pss, fs, pcsp, s);
            dlg.Title.Should().Be(Resources.SqlPublishDialog_Title);
            dlg.DataContext.Should().BeOfType(typeof(SqlPublishOptionsDialogViewModel));

            var model = dlg.DataContext as SqlPublishOptionsDialogViewModel;

            model.Settings.TargetType.Should().Be(PublishTargetType.Dacpac);
            model.TargetHasName.Should().BeFalse();
            model.GenerateTable.Should().BeFalse();
            model.Settings.Should().NotBeNull();
        }
    }
}
